using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using DiagramNet.Elements;
using DiagramNet.Actions;
using DiagramNet.Events;
using StaticExtensions;

namespace DiagramNet {
  public partial class Diagram : UserControl {

    //Document
    private Document document = new();
    // Drag and Drop
    MoveAction? moveAction = null;

    // Selection
    BaseElement? selectedElement;
    private bool isMultiSelection = false;
    private readonly RectangleElement selectionArea = new(0, 0, 0, 0);
    private IController[]? controllers;
    private BaseElement? mousePointerElement;

    // Resize
    private ResizeAction? resizeAction = null;

    private readonly bool isAddSelection = false;
    // Link
    private bool isAddLink = false;
    private ConnectorElement? connStart;
    private ConnectorElement? connEnd;
    private BaseLinkElement? linkLine;

    // Label    
    private readonly bool isEditLabel = false;
    //private LabelElement? selectedLabel;
    private readonly System.Windows.Forms.TextBox labelTextBox = new();
    private EditLabelAction? editLabelAction = null;

    //Undo
    [NonSerialized]
    private readonly UndoManager undo = new(5);
    private readonly bool changed = false;
    public Diagram() {
      InitializeComponent();

      // This change control to not flick
      this.SetStyle(ControlStyles.UserPaint, true);
      this.SetStyle(ControlStyles.ResizeRedraw, true);
      this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      this.SetStyle(ControlStyles.DoubleBuffer, true);

      // Selection Area Properties
      selectionArea.Opacity = 40;
      selectionArea.FillColor1 = SystemColors.Control;
      selectionArea.FillColor2 = Color.Empty;
      selectionArea.BorderColor = SystemColors.Control;

      // Link Line Properties
      //linkLine.BorderColor = Color.FromArgb(127, Color.DarkGray);
      //linkLine.BorderWidth = 4;

      // Label Edit
      labelTextBox.BorderStyle = BorderStyle.FixedSingle;
      labelTextBox.Multiline = true;
      labelTextBox.Hide();
      this.Controls.Add(labelTextBox);

      //EventsHandlers
      RecreateEventsHandlers();
    }

    #region Invalidate Overrides
    public new void Invalidate() {
      if (document is not null && document.Elements.Count > 0) {
        for (int i = 0; i <= document.Elements.Count - 1; i++) {
          BaseElement? el = document.Elements[i];
          
          if (el is not null){
            if (el is ILabelElement element) { 
              Invalidate(element.Label);
            } else {
              Invalidate(el);
            }
          }
          

            
        }
      } else
        base.Invalidate();

      if ((moveAction != null) && (moveAction.IsMoving) && (document is not null))
        this.AutoScrollMinSize = new Size((int)((document.Location.X + document.Size.Width) * document.Zoom), (int)((document.Location.Y + document.Size.Height) * document.Zoom));

    }

    private void Invalidate(BaseElement el) {
      this.Invalidate(el, false);
    }

    private void Invalidate(BaseElement el, bool force) {
      if (el == null) return;

      if ((force) || (el.IsInvalidated)) {
        Rectangle invalidateRec = Goc2Gsc(el.invalidateRec);
        invalidateRec.Inflate(10, 10);
        base.Invalidate(invalidateRec);
      }
    }

    #endregion
    #region Events Overrides
    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
      Graphics g = e.Graphics;
      GraphicsContainer gc;
      Matrix mtx;
      g.PageUnit = GraphicsUnit.Pixel;

      Point scrollPoint = this.AutoScrollPosition;
      g.TranslateTransform(scrollPoint.X, scrollPoint.Y);

      //Zoom
      mtx = g.Transform;
      gc = g.BeginContainer();

      g.SmoothingMode = document.SmoothingMode;
      g.PixelOffsetMode = document.PixelOffsetMode;
      g.CompositingQuality = document.CompositingQuality;

      g.ScaleTransform(document.Zoom, document.Zoom);

      Rectangle clipRectangle = Gsc2Goc(e.ClipRectangle);

      document.DrawElements(g, clipRectangle);

      if (!((resizeAction != null) && (resizeAction.IsResizing)))
        document.DrawSelections(g);

      if ((isMultiSelection) || (isAddSelection))
        DrawSelectionRectangle(g);

      if (isAddLink) {
        linkLine?.CalcLink();
        linkLine?.Draw(g);
      }
      if ((resizeAction != null) && (!((moveAction != null) && (moveAction.IsMoving))))
        resizeAction.DrawResizeCorner(g);

      if (mousePointerElement != null) {
        if (mousePointerElement is IControllable controllable) {
          IController ctrl = controllable.GetController();
          ctrl.DrawSelection(g);
        }
      }

      g.EndContainer(gc);
      g.Transform = mtx;

      base.OnPaint(e);

    }

    protected override void OnPaintBackground(PaintEventArgs e) {
      base.OnPaintBackground(e);

      Graphics g = e.Graphics;
      GraphicsContainer gc;
      Matrix mtx;
      g.PageUnit = GraphicsUnit.Pixel;
      mtx = g.Transform;
      gc = g.BeginContainer();

      //Rectangle clipRectangle = Gsc2Goc(e.ClipRectangle);

      document.DrawGrid(g);

      g.EndContainer(gc);
      g.Transform = mtx;

    }


    protected override void OnKeyDown(KeyEventArgs e) {
      //Delete element
      if (e.KeyCode == Keys.Delete) {
        DeleteSelectedElements();
        EndGeneralAction();
        base.Invalidate();
      }

      //Undo
      if (e.Control && e.KeyCode == Keys.Z) {
        if (undo.CanUndo)
          Undo();
      }

      //Copy
      if ((e.Control) && (e.KeyCode == Keys.C)) {
        this.Copy();
      }

      //Paste
      if ((e.Control) && (e.KeyCode == Keys.V)) {
        this.Paste();
      }

      //Cut
      if ((e.Control) && (e.KeyCode == Keys.X)) {
        this.Cut();
      }

      base.OnKeyDown(e);
    }

    protected override void OnResize(EventArgs e) {
      base.OnResize(e);
      document.WindowSize = this.Size;
    }

    #region Mouse Events
    protected override void OnMouseDown(MouseEventArgs e) {
      Point mousePoint;

      //ShowSelectionCorner((document.Action==DesignerAction.Select));

      switch (document.Action) {
        // SELECT
        case DesignerAction.Connect:
        case DesignerAction.Select:
          if (e.Button == MouseButtons.Left) {
            mousePoint = Gsc2Goc(new Point(e.X, e.Y));

            //Verify resize action
            StartResizeElement(mousePoint);
            if ((resizeAction != null) && (resizeAction.IsResizing)) break;

            //Verify label editing
            if (isEditLabel) {
              EndEditLabel();
            }

            // Search element by click
            selectedElement = document.FindElement(mousePoint);

            if (selectedElement != null) {
              //Events
              ElementMouseEventArgs eventMouseDownArg = new(selectedElement, e.X, e.Y);
              OnElementMouseDown(eventMouseDownArg);

              // Double-click to edit Label
              if ((e.Clicks == 2) && (selectedElement is ILabelElement)) {
                //selectedLabel = element.Label;
                StartEditLabel();
                break;
              }

              // Element selected
              if (selectedElement is ConnectorElement element1) {
                StartAddLink(element1, mousePoint);
                selectedElement = null;
              } else
                StartSelectElements(selectedElement, mousePoint);
            } else {
              // If click is on neutral area, clear selection
              document.ClearSelection();
              Point p = Gsc2Goc(new Point(e.X, e.Y)); ;
              isMultiSelection = true;
              selectionArea.Visible = true;
              selectionArea.Location = p;
              selectionArea.Size = new Size(0, 0);

              if (resizeAction != null)
                resizeAction.ShowResizeCorner(false);
            }
            base.Invalidate();
          }
          break;

        // ADD
        case DesignerAction.Add:

          if (e.Button == MouseButtons.Left) {
            mousePoint = Gsc2Goc(new Point(e.X, e.Y));
            StartAddElement(mousePoint);
          }
          break;

        // DELETE
        case DesignerAction.Delete:
          if (e.Button == MouseButtons.Left) {
            mousePoint = Gsc2Goc(new Point(e.X, e.Y));
            DeleteElement(mousePoint);
          }
          break;
      }

      base.OnMouseDown(e);

    }

    protected override void OnMouseMove(MouseEventArgs e) {

      if (e.Button == MouseButtons.None) {
        this.Cursor = Cursors.Arrow;
        Point mousePoint = Gsc2Goc(new Point(e.X, e.Y));

        if ((resizeAction != null)
            && ((document.Action == DesignerAction.Select)
                || ((document.Action == DesignerAction.Connect)
                    && (resizeAction.IsResizingLink)))) {
          this.Cursor = resizeAction.UpdateResizeCornerCursor(mousePoint);
        }

        if (document.Action == DesignerAction.Connect) {
          BaseElement? mousePointerElementTMP = document.FindElement(mousePoint);
          if (mousePointerElement != mousePointerElementTMP) {
            if (mousePointerElementTMP is ConnectorElement) {
              mousePointerElement = mousePointerElementTMP;
              mousePointerElement.Invalidate();
              this.Invalidate(mousePointerElement, true);
            } else if (mousePointerElement != null) {
              mousePointerElement.Invalidate();
              this.Invalidate(mousePointerElement, true);
              mousePointerElement = null;
            }

          }
        } else {
          if (mousePointerElement is not null) 
            this.Invalidate(mousePointerElement, true);
          mousePointerElement = null;
        }
      }

      if (e.Button == MouseButtons.Left) {
        Point dragPoint = Gsc2Goc(new Point(e.X, e.Y));

        if ((resizeAction != null) && (resizeAction.IsResizing)) {
          resizeAction.Resize(dragPoint);
          this.Invalidate();
        }

        if ((moveAction != null) && (moveAction.IsMoving)) {
          moveAction.Move(dragPoint);
          this.Invalidate();
        }

        if ((isMultiSelection) || (isAddSelection)) {
          Point p = Gsc2Goc(new Point(e.X, e.Y));
          selectionArea.Size = new Size(p.X - selectionArea.Location.X, p.Y - selectionArea.Location.Y);
          selectionArea.Invalidate();
          this.Invalidate(selectionArea, true);
        }

        if (isAddLink && linkLine is not null && connEnd is not null) {
          selectedElement = document.FindElement(dragPoint);
          if ( (selectedElement is not null)
              && (connStart is not null)
              && (selectedElement is ConnectorElement element)
              && (Document.CanAddLink(connStart, element)))
            linkLine.Connector2 = element;
          else
            linkLine.Connector2 = connEnd;

          IMoveController ctrl = (IMoveController)(connEnd as IControllable).GetController();
          ctrl.Move(dragPoint);

          //this.Invalidate(linkLine, true); //TODO
          base.Invalidate();
        }
      }

      base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
      Rectangle selectionRectangle = selectionArea.GetUnsignedRectangle();

      if ((moveAction != null) && (moveAction.IsMoving) && selectedElement is not null) {
        ElementEventArgs eventClickArg = new(selectedElement);
        OnElementClick(eventClickArg);

        moveAction.End();
        moveAction = null;

        ElementMouseEventArgs eventMouseUpArg = new(selectedElement, e.X, e.Y);
        OnElementMouseUp(eventMouseUpArg);

        if (changed)
          AddUndo();
      }

      // Select
      if (isMultiSelection) {
        EndSelectElements(selectionRectangle);
      }
      // Add element
      else if (isAddSelection) {
        EndAddElement(selectionRectangle);
      }

      // Add link
      else if (isAddLink) {
        //Point mousePoint = Gsc2Goc(new Point(e.X, e.Y));
        EndAddLink();
        AddUndo();
      }

      // Resize
      if (resizeAction != null) {
        if (resizeAction.IsResizing) {
          Point mousePoint = Gsc2Goc(new Point(e.X, e.Y));
          resizeAction.End(mousePoint);

          AddUndo();
        }
        resizeAction.UpdateResizeCorner();
      }

      RestartInitValues();

      base.Invalidate();

      base.OnMouseUp(e);
    }
    #endregion

    #endregion
    #region Events Raising

    // element handler
    public delegate void ElementEventHandler(object sender, ElementEventArgs e);

    #region Element Mouse Events

    // CLICK
    [Category("Element")]
    public event ElementEventHandler? ElementClick;

    protected virtual void OnElementClick(ElementEventArgs e) {
      ElementClick?.Invoke(this, e);
    }

    // mouse handler
    public delegate void ElementMouseEventHandler(object sender, ElementMouseEventArgs e);

    // MOUSE DOWN
    [Category("Element")]
    public event ElementMouseEventHandler? ElementMouseDown;

    protected virtual void OnElementMouseDown(ElementMouseEventArgs e) {
      ElementMouseDown?.Invoke(this, e);
    }

    // MOUSE UP
    [Category("Element")]
    public event ElementMouseEventHandler? ElementMouseUp;

    protected virtual void OnElementMouseUp(ElementMouseEventArgs e) {
      ElementMouseUp?.Invoke(this, e);
    }

    #endregion

    #region Element Move Events
    // Before Move
    [Category("Element")]
    public event ElementEventHandler? ElementMoving;

    protected virtual void OnElementMoving(ElementEventArgs e) {
      ElementMoving?.Invoke(this, e);
    }

    // After Move
    [Category("Element")]
    public event ElementEventHandler? ElementMoved;

    protected virtual void OnElementMoved(ElementEventArgs e) {
      ElementMoved?.Invoke(this, e);
    }
    #endregion

    #region Element Resize Events
    // Before Resize
    [Category("Element")]
    public event ElementEventHandler? ElementResizing;

    protected virtual void OnElementResizing(ElementEventArgs e) {
      ElementResizing?.Invoke(this, e);
    }

    // After Resize
    [Category("Element")]
    public event ElementEventHandler? ElementResized;

    protected virtual void OnElementResized(ElementEventArgs e) {
      ElementResized?.Invoke(this, e);
    }
    #endregion

    #region Element Connect Events
    // connect handler
    public delegate void ElementConnectEventHandler(object sender, ElementConnectEventArgs e);

    // Before Connect
    [Category("Element")]
    public event ElementConnectEventHandler? ElementConnecting;

    protected virtual void OnElementConnecting(ElementConnectEventArgs e) {
      ElementConnecting?.Invoke(this, e);
    }

    // After Connect
    [Category("Element")]
    public event ElementConnectEventHandler? ElementConnected;

    protected virtual void OnElementConnected(ElementConnectEventArgs e) {
      ElementConnected?.Invoke(this, e);
    }
    #endregion

    #region Element Selection Events
    // connect handler
    public delegate void ElementSelectionEventHandler(object sender, ElementSelectionEventArgs e);

    // Selection
    [Category("Element")]
    public event ElementSelectionEventHandler? ElementSelection;

    protected virtual void OnElementSelection(ElementSelectionEventArgs e) {
      ElementSelection?.Invoke(this, e);
    }

    #endregion

    #endregion
    #region Events Handling
    private void Document_PropertyChanged(object? sender, EventArgs e) {
      if (!IsChanging()) {
        base.Invalidate();
      }
    }

    private void Document_AppearancePropertyChanged(object? sender, EventArgs e) {
      if (!IsChanging()) {
        AddUndo();
        base.Invalidate();
      }
    }

    private void Document_ElementPropertyChanged(object? sender, EventArgs e) {
      if (!IsChanging()) {
        AddUndo();
        base.Invalidate();
      }
    }

    private void Document_ElementSelection(object? sender, ElementSelectionEventArgs e) {
      OnElementSelection(e);
    }
    #endregion
    #region Properties

    public Document Document {
      get {
        return document;
      }
    }

    public bool CanUndo {
      get {
        return undo.CanUndo;
      }
    }

    public bool CanRedo {
      get {
        return undo.CanRedo;
      }
    }


    private bool IsChanging() {
      return (
              ((moveAction != null) && (moveAction.IsMoving)) //isDragging
              || isAddLink || isMultiSelection ||
              ((resizeAction != null) && (resizeAction.IsResizing)) //isResizing
              );
    }
    #endregion
    #region Draw Methods

    /// <summary>
    /// Graphic surface coordinates to graphic object coordinates.
    /// </summary>
    /// <param name="p">Graphic surface point.</param>
    /// <returns></returns>
    public Point Gsc2Goc(Point gsp) {
      float zoom = document.Zoom;
      gsp.X = (int)((gsp.X - this.AutoScrollPosition.X) / zoom);
      gsp.Y = (int)((gsp.Y - this.AutoScrollPosition.Y) / zoom);
      return gsp;
    }

    public Rectangle Gsc2Goc(Rectangle gsr) {
      float zoom = document.Zoom;
      gsr.X = (int)((gsr.X - this.AutoScrollPosition.X) / zoom);
      gsr.Y = (int)((gsr.Y - this.AutoScrollPosition.Y) / zoom);
      gsr.Width = (int)(gsr.Width / zoom);
      gsr.Height = (int)(gsr.Height / zoom);
      return gsr;
    }

    public Rectangle Goc2Gsc(Rectangle gsr) {
      float zoom = document.Zoom;
      gsr.X = (int)((gsr.X + this.AutoScrollPosition.X) * zoom);
      gsr.Y = (int)((gsr.Y + this.AutoScrollPosition.Y) * zoom);
      gsr.Width = (int)(gsr.Width * zoom);
      gsr.Height = (int)(gsr.Height * zoom);
      return gsr;
    }

    internal void DrawSelectionRectangle(Graphics g) {
      selectionArea.Draw(g);
    }
    #endregion

    #region Open/Save File
    public void Save(string fileName) {
      if (document is not null) {
        document.Save(fileName);
      }
    }

    public void Open(string fileName) {      
      if (File.Exists(fileName)) {
        document.Open(fileName);
      }
    }
    public void Save(System.IO.MemoryStream ms) {
      if (document is not null) {
        string? documentJson = document.AsJsonString();
        if (documentJson is not null) {
          byte[] buffer = System.Text.Encoding.UTF8.GetBytes(documentJson);
          ms.Write(buffer, 0, buffer.Length);
        }
      }
    }
    public void Open(System.IO.MemoryStream ms) {
      byte[] buffer = ms.ToArray();
      string documentJson = System.Text.Encoding.UTF8.GetString(buffer);
      var document1 = (Document?)documentJson.AsFromJsonString();
      if (document1 is not null) { 
        document = document1;
        RecreateEventsHandlers();
      }      
    }
    public void Save(out byte[] buffer) {
      var ms = new System.IO.MemoryStream();
      Save(ms);
      buffer = ms.ToArray();
    }
    public void Open(byte[] buffer) {
      Open(new MemoryStream(buffer));
    }
    #endregion

    #region Copy/Paste
    public void Copy() {
      if (document.SelectedElements.Count == 0) return;
      BaseElement?[] selectedArray = document.SelectedElements.GetArray();
      if (selectedArray is not null) {
        string? selectedArrayJson = selectedArray.AsJsonString();
        if (selectedArrayJson is not null) { 
          Stream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(selectedArrayJson));
          DataObject data = new(DataFormats.GetFormat("Diagram.NET Element Collection").Name, stream);
          Clipboard.SetDataObject(data);
        }
      }
    }

    public void Paste() {
      const int pasteStep = 20;

      undo.Enabled = false;
      IDataObject iData = Clipboard.GetDataObject();
      DataFormats.Format format = DataFormats.GetFormat("Diagram.NET Element Collection");
      if (iData.GetDataPresent(format.Name)) {
        MemoryStream stream = (MemoryStream)iData.GetData(format.Name);
        string documentJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        BaseElement[]? elCol = (BaseElement[]?)documentJson.AsFromJsonString();
        stream.Close();

        if(elCol is not null) { 
          foreach (BaseElement el in elCol) {
            el.Location = new Point(el.Location.X + pasteStep, el.Location.Y + pasteStep);
          }

          document.AddElements(elCol);
          document.ClearSelection();
          document.SelectElements(elCol);
        }
      }
      undo.Enabled = true;

      AddUndo();
      EndGeneralAction();
    }

    public void Cut() {
      this.Copy();
      DeleteSelectedElements();
      EndGeneralAction();
    }
    #endregion

    #region Start/End Actions and General Functions

    #region General
    private void EndGeneralAction() {
      RestartInitValues();

      if (resizeAction != null) resizeAction.ShowResizeCorner(false);
    }

    private void RestartInitValues() {

      // Reinitialize status
      moveAction = null;

      isMultiSelection = false;
      isAddLink = false;

      connStart = null;

      selectionArea.FillColor1 = SystemColors.Control;
      selectionArea.BorderColor = SystemColors.Control;
      selectionArea.Visible = false;

      document.CalcWindow(true);
    }

    #endregion

    #region Selection
    private void StartSelectElements(BaseElement selectedElement, Point mousePoint) {
      // Vefiry if element is in selection
      if (!document.SelectedElements.Contains(selectedElement)) {
        //Clear selection and add new element to selection
        document.ClearSelection();
        document.SelectElement(selectedElement);
      }

      moveAction = new MoveAction();
      MoveAction.OnElementMovingDelegate onElementMovingDelegate = new(OnElementMoving);
      moveAction.Start(mousePoint, document, onElementMovingDelegate);


      // Get Controllers
      controllers = new IController[document.SelectedElements.Count];
      for (int i = document.SelectedElements.Count - 1; i >= 0; i--) {
        if (document.SelectedElements[i] is IControllable) {
          // Get General Controller
          IControllable? controllable = (IControllable?)document.SelectedElements[i];
          if (controllable is not null) { 
              controllers[i] = controllable.GetController();
          }         
        }
      }

      resizeAction = new ResizeAction();
      resizeAction.Select(document);
    }

    private void EndSelectElements(Rectangle selectionRectangle) {
      document.SelectElements(selectionRectangle);
    }
    #endregion

    #region Resize
    private void StartResizeElement(Point mousePoint) {
      if ((resizeAction != null)
        && ((document.Action == DesignerAction.Select)
          || ((document.Action == DesignerAction.Connect)
            && (resizeAction.IsResizingLink)))) {
        ResizeAction.OnElementResizingDelegate onElementResizingDelegate = new(OnElementResizing);
        resizeAction.Start(mousePoint, onElementResizingDelegate);
        if (!resizeAction.IsResizing)
          resizeAction = null;
      }
    }
    #endregion

    #region Link
    private void StartAddLink(ConnectorElement connStart, Point mousePoint) {
      if (document.Action == DesignerAction.Connect) {
        this.connStart = connStart;
        this.connEnd = new ConnectorElement(connStart.ParentElement);

        connEnd.Location = connStart.Location;
        IMoveController ctrl = (IMoveController)((IControllable)connEnd).GetController();
        ctrl.Start(mousePoint);

        isAddLink = true;

        switch (document.LinkType) {
          case (LinkType.Straight):
            linkLine = new StraightLinkElement(connStart, connEnd);
            break;
          case (LinkType.RightAngle):
            linkLine = new RightAngleLinkElement(connStart, connEnd);
            break;
        }
        if (linkLine != null) {
          linkLine.Visible = true;
          linkLine.BorderColor = Color.FromArgb(150, Color.Black);
          linkLine.BorderWidth = 1;
          this.Invalidate(linkLine, true);
          OnElementConnecting(new ElementConnectEventArgs(connStart.ParentElement, null, linkLine));
        }                       
      }
    }

    private void EndAddLink() {
      if(linkLine != null) {
        BaseLinkElement thisLinkLine = linkLine as BaseLinkElement;
        if (connEnd != linkLine.Connector2) {
          linkLine.Connector1.RemoveLink(linkLine);
          linkLine = document.AddLink(linkLine.Connector1, linkLine.Connector2);
          OnElementConnected(new ElementConnectEventArgs(thisLinkLine.Connector1.ParentElement, thisLinkLine.Connector2.ParentElement, thisLinkLine));
        }

        connStart = null;
        connEnd = null;
        linkLine = null;
      }
    }
    #endregion

    #region Add Element
    private void StartAddElement(Point mousePoint) {
      document.ClearSelection();

      //Change Selection Area Color
      selectionArea.FillColor1 = Color.LightSteelBlue;
      selectionArea.BorderColor = Color.WhiteSmoke;

      selectionArea.Visible = true;
      selectionArea.Location = mousePoint;
      selectionArea.Size = new Size(0, 0);
    }

    private void EndAddElement(Rectangle selectionRectangle) {
      BaseElement el = document.ElementType switch {
        ElementType.Rectangle => new RectangleElement(selectionRectangle),
        ElementType.RectangleNode => new RectangleNode(selectionRectangle),
        ElementType.Elipse => new ElipseElement(selectionRectangle),
        ElementType.ElipseNode => new ElipseNode(selectionRectangle),
        ElementType.CommentBox => new CommentBoxElement(selectionRectangle),
        ElementType.CommentBoxNode => new CommentBoxNode(selectionRectangle),
        _ => new RectangleNode(selectionRectangle),
      };
      document.AddElement(el);

      document.Action = DesignerAction.Select;
    }
    #endregion

    #region Edit Label
    private void StartEditLabel() {
      if (selectedElement != null) {

        if (resizeAction != null) {
          resizeAction.ShowResizeCorner(false);
          resizeAction = null;
        }

        editLabelAction = new EditLabelAction();
        editLabelAction.StartEdit(selectedElement, labelTextBox);
      }
    }

    private void EndEditLabel() {
      if (editLabelAction != null) {
        editLabelAction.EndEdit();
        editLabelAction = null;
      }
    }
    #endregion

    #region Delete
    private void DeleteElement(Point mousePoint) {
      document.DeleteElement(mousePoint);
      selectedElement = null;
      document.Action = DesignerAction.Select;
    }

    private void DeleteSelectedElements() {
      document.DeleteSelectedElements();
    }
    #endregion

    #endregion

    #region Undo/Redo
    public void Undo() {
      Document? document1 = (Document?)undo.Undo();
      if (document1 is not null) {
        document = document1;
        RecreateEventsHandlers();
        if (resizeAction != null) resizeAction.UpdateResizeCorner();
        base.Invalidate();
      }      
    }

    public void Redo() {
      Document? document1 = (Document?)undo.Redo();
      if (document1 is not null) {
        document = document1;
        RecreateEventsHandlers();
        if (resizeAction != null) 
          resizeAction.UpdateResizeCorner();
        base.Invalidate();
      }
    }

    private void AddUndo() {
      undo.AddUndo(document);
    }
    #endregion
    private void RecreateEventsHandlers() {
      document.PropertyChanged += new EventHandler(Document_PropertyChanged);
      document.AppearancePropertyChanged += new EventHandler(Document_AppearancePropertyChanged);
      document.ElementPropertyChanged += new EventHandler(Document_ElementPropertyChanged);
      document.ElementSelection += new Document.ElementSelectionEventHandler(Document_ElementSelection);
    }


  }
}