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
  partial class Designer {
    #region Designer Control Initialization
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    //Document
    private Document document = new Document();
    // Drag and Drop
    MoveAction moveAction = null;

    // Selection
    BaseElement selectedElement;
    private bool isMultiSelection = false;
    private RectangleElement selectionArea = new RectangleElement(0, 0, 0, 0);
    private IController[] controllers;

    // Resize
    private ResizeAction resizeAction = null;
    
    // Link
    private bool isAddLink = false;
    private ConnectorElement connStart;
    private ConnectorElement connEnd;
    private BaseLinkElement linkLine;

    // Label    
    private System.Windows.Forms.TextBox labelTextBox = new TextBox();
    private EditLabelAction editLabelAction = null;

    //Undo
    [NonSerialized]
    private UndoManager undo = new UndoManager(5);    

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }
    #endregion
    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.SuspendLayout();
      // 
      // Designer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Name = "Designer";
      this.Size = new System.Drawing.Size(471, 450);
      this.ResumeLayout(false);

    }



    #endregion
    #region Invalidate Overrides
    public new void Invalidate() {
      if (document.Elements.Count > 0) {
        for (int i = 0; i <= document.Elements.Count - 1; i++) {
          BaseElement el = document.Elements[i];

          Invalidate(el);

          if (el is ILabelElement)
            Invalidate(((ILabelElement)el).Label);
        }
      } else
        base.Invalidate();

      if ((moveAction != null) && (moveAction.IsMoving))
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
    #region Events Raising

    // element handler
    public delegate void ElementEventHandler(object sender, ElementEventArgs e);

    #region Element Mouse Events

    // CLICK
    [Category("Element")]
    public event ElementEventHandler ElementClick;

    protected virtual void OnElementClick(ElementEventArgs e) {
      if (ElementClick != null) {
        ElementClick(this, e);
      }
    }

    // mouse handler
    public delegate void ElementMouseEventHandler(object sender, ElementMouseEventArgs e);

    // MOUSE DOWN
    [Category("Element")]
    public event ElementMouseEventHandler ElementMouseDown;

    protected virtual void OnElementMouseDown(ElementMouseEventArgs e) {
      if (ElementMouseDown != null) {
        ElementMouseDown(this, e);
      }
    }

    // MOUSE UP
    [Category("Element")]
    public event ElementMouseEventHandler ElementMouseUp;

    protected virtual void OnElementMouseUp(ElementMouseEventArgs e) {
      if (ElementMouseUp != null) {
        ElementMouseUp(this, e);
      }
    }

    #endregion

    #region Element Move Events
    // Before Move
    [Category("Element")]
    public event ElementEventHandler ElementMoving;

    protected virtual void OnElementMoving(ElementEventArgs e) {
      if (ElementMoving != null) {
        ElementMoving(this, e);
      }
    }

    // After Move
    [Category("Element")]
    public event ElementEventHandler ElementMoved;

    protected virtual void OnElementMoved(ElementEventArgs e) {
      if (ElementMoved != null) {
        ElementMoved(this, e);
      }
    }
    #endregion

    #region Element Resize Events
    // Before Resize
    [Category("Element")]
    public event ElementEventHandler ElementResizing;

    protected virtual void OnElementResizing(ElementEventArgs e) {
      if (ElementResizing != null) {
        ElementResizing(this, e);
      }
    }

    // After Resize
    [Category("Element")]
    public event ElementEventHandler ElementResized;

    protected virtual void OnElementResized(ElementEventArgs e) {
      if (ElementResized != null) {
        ElementResized(this, e);
      }
    }
    #endregion

    #region Element Connect Events
    // connect handler
    public delegate void ElementConnectEventHandler(object sender, ElementConnectEventArgs e);

    // Before Connect
    [Category("Element")]
    public event ElementConnectEventHandler ElementConnecting;

    protected virtual void OnElementConnecting(ElementConnectEventArgs e) {
      if (ElementConnecting != null) {
        ElementConnecting(this, e);
      }
    }

    // After Connect
    [Category("Element")]
    public event ElementConnectEventHandler ElementConnected;

    protected virtual void OnElementConnected(ElementConnectEventArgs e) {
      if (ElementConnected != null) {
        ElementConnected(this, e);
      }
    }
    #endregion

    #region Element Selection Events
    // connect handler
    public delegate void ElementSelectionEventHandler(object sender, ElementSelectionEventArgs e);

    // Selection
    [Category("Element")]
    public event ElementSelectionEventHandler ElementSelection;

    protected virtual void OnElementSelection(ElementSelectionEventArgs e) {
      if (ElementSelection != null) {
        ElementSelection(this, e);
      }
    }

    #endregion

    #endregion
    #region Events Handling
    private void document_PropertyChanged(object sender, EventArgs e) {
      if (!IsChanging()) {
        base.Invalidate();
      }
    }

    private void document_AppearancePropertyChanged(object sender, EventArgs e) {
      if (!IsChanging()) {
        AddUndo();
        base.Invalidate();
      }
    }

    private void document_ElementPropertyChanged(object sender, EventArgs e) {
      if (!IsChanging()) {
        AddUndo();
        base.Invalidate();
      }
    }

    private void document_ElementSelection(object sender, ElementSelectionEventArgs e) 
    {
      OnElementSelection(e);
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
        if (File.Exists(fileName)) { 
          File.Delete(fileName);
        }                
        using StreamWriter w = File.CreateText(fileName);
        w.WriteLine(document.AsJsonString()); 
      }
    }

    public void Open(string fileName) {
      string sRawFileTxt = "";
      if (File.Exists(fileName)) { 
        sRawFileTxt = File.ReadAllText(fileName);
        document = (Document)sRawFileTxt.AsFromJsonString();
        RecreateEventsHandlers();
      }      
    }
    public void Save(System.IO.MemoryStream ms) {
      if (document != null) { 
        string documentJson = document.AsJsonString();
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes( documentJson);        
        ms.Write(buffer, 0, buffer.Length);
      }
    }
    public void Open(System.IO.MemoryStream ms) {
       byte[] buffer = ms.ToArray();
       string documentJson = System.Text.Encoding.UTF8.GetString(buffer);
       document = (Document)documentJson.AsFromJsonString();
       RecreateEventsHandlers();
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
      BaseElement[] selectedArray = document.SelectedElements.GetArray();  
      if (selectedArray is not null) { 
        Stream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(selectedArray.AsJsonString()));      
        DataObject data = new DataObject(DataFormats.GetFormat("Diagram.NET Element Collection").Name, stream);
        Clipboard.SetDataObject(data);
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
        BaseElement[] elCol = (BaseElement[])documentJson.AsFromJsonString();
        stream.Close();

        foreach (BaseElement el in elCol) {
          el.Location = new Point(el.Location.X + pasteStep, el.Location.Y + pasteStep);
        }

        document.AddElements(elCol);
        document.ClearSelection();
        document.SelectElements(elCol);
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
      MoveAction.OnElementMovingDelegate onElementMovingDelegate = new MoveAction.OnElementMovingDelegate(OnElementMoving);
      moveAction.Start(mousePoint, document, onElementMovingDelegate);


      // Get Controllers
      controllers = new IController[document.SelectedElements.Count];
      for (int i = document.SelectedElements.Count - 1; i >= 0; i--) {
        if (document.SelectedElements[i] is IControllable) {
          // Get General Controller
          controllers[i] = ((IControllable)document.SelectedElements[i]).GetController();
        } else {
          controllers[i] = null;
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
        ResizeAction.OnElementResizingDelegate onElementResizingDelegate = new ResizeAction.OnElementResizingDelegate(OnElementResizing);
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
        linkLine.Visible = true;
        linkLine.BorderColor = Color.FromArgb(150, Color.Black);
        linkLine.BorderWidth = 1;

        this.Invalidate(linkLine, true);

        OnElementConnecting(new ElementConnectEventArgs(connStart.ParentElement, null, linkLine));
      }
    }

    private void EndAddLink() {
      if (connEnd != linkLine.Connector2) {
        linkLine.Connector1.RemoveLink(linkLine);
        linkLine = document.AddLink(linkLine.Connector1, linkLine.Connector2);
        OnElementConnected(new ElementConnectEventArgs(linkLine.Connector1.ParentElement, linkLine.Connector2.ParentElement, linkLine));
      }

      connStart = null;
      connEnd = null;
      linkLine = null;
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
      BaseElement el;
      switch (document.ElementType) {
        case ElementType.Rectangle:
          el = new RectangleElement(selectionRectangle);
          break;
        case ElementType.RectangleNode:
          el = new RectangleNode(selectionRectangle);
          break;
        case ElementType.Elipse:
          el = new ElipseElement(selectionRectangle);
          break;
        case ElementType.ElipseNode:
          el = new ElipseNode(selectionRectangle);
          break;
        case ElementType.CommentBox:
          el = new CommentBoxElement(selectionRectangle);
          break;
        case ElementType.CommentBoxNode:
          el = new CommentBoxNode(selectionRectangle);
          break;
        default:
          el = new RectangleNode(selectionRectangle);
          break;
      }

      document.AddElement(el);

      document.Action = DesignerAction.Select;
    }
    #endregion

    #region Edit Label
    private void StartEditLabel() {
      // Disable resize
      if (resizeAction != null) {
        resizeAction.ShowResizeCorner(false);
        resizeAction = null;
      }

      editLabelAction = new EditLabelAction();
      editLabelAction.StartEdit(selectedElement, labelTextBox);
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
      document = (Document)undo.Undo();
      RecreateEventsHandlers();
      if (resizeAction != null) resizeAction.UpdateResizeCorner();
      base.Invalidate();
    }

    public void Redo() {
      document = (Document)undo.Redo();
      RecreateEventsHandlers();
      if (resizeAction != null) resizeAction.UpdateResizeCorner();
      base.Invalidate();
    }

    private void AddUndo() {
      undo.AddUndo(document);
    }
    #endregion
    private void RecreateEventsHandlers() {
      document.PropertyChanged += new EventHandler(document_PropertyChanged);
      document.AppearancePropertyChanged += new EventHandler(document_AppearancePropertyChanged);
      document.ElementPropertyChanged += new EventHandler(document_ElementPropertyChanged);
      document.ElementSelection += new Document.ElementSelectionEventHandler(document_ElementSelection);
    }

  }
}