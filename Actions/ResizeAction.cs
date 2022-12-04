using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DiagramNet.Events;
using DiagramNet.Elements;


namespace DiagramNet.Actions {

	/// <summary>
	/// This class control the size of elements
	/// </summary>
	internal class ResizeAction
	{
		public delegate void OnElementResizingDelegate(ElementEventArgs e);
		private OnElementResizingDelegate? onElementResizingDelegate;

		private bool isResizing = false;
		private IResizeController? resizeCtrl = null;
		private Document? document = null;

		public ResizeAction()
		{
		}

		public bool IsResizing
		{
			get
			{
				return isResizing;
			}
		}

		public bool IsResizingLink
		{
			get
			{
				return ((resizeCtrl != null) && (resizeCtrl.OwnerElement is BaseLinkElement));
			}
		}

		public void Select(Document document)
		{ 
			bool isResizeSet = false;
			if (document is not null) { 
				this.document = document;
				if (document.SelectedElements.Count > 0) { 					
					var elementCollection = document.SelectedElements[0];
					if ((elementCollection is not null) && (elementCollection is IControllable controllable))
					{	
						IController? ctrl = controllable.GetController();
						if (ctrl is IResizeController controller)
						{
							ctrl.OwnerElement.Invalidate();

							resizeCtrl = controller;
							ShowResizeCorner(true);
							isResizeSet = true;
						}
					}					
				}
			}
			if (!isResizeSet) {
			  resizeCtrl = null;
			}
		}

		public void Start(Point mousePoint, OnElementResizingDelegate onElementResizingDelegate)
		{
			isResizing = false;

			if (resizeCtrl == null) return;

			this.onElementResizingDelegate = onElementResizingDelegate;

			resizeCtrl.OwnerElement.Invalidate();

			CornerPosition corPos = resizeCtrl.HitTestCorner(mousePoint);

			if (corPos != CornerPosition.Nothing)
			{
				//Events
				ElementEventArgs eventResizeArg = new(resizeCtrl.OwnerElement);
				onElementResizingDelegate(eventResizeArg);

				resizeCtrl.Start(mousePoint, corPos);

				UpdateResizeCorner();

				isResizing = true;
			}

		}

		public void Resize(Point dragPoint)
		{
			if ((resizeCtrl != null) && (resizeCtrl.CanResize))
			{				
				//Events
				ElementEventArgs eventResizeArg = new(resizeCtrl.OwnerElement);
				if (onElementResizingDelegate is not null) onElementResizingDelegate(eventResizeArg);

				resizeCtrl.OwnerElement.Invalidate();

				resizeCtrl.Resize(dragPoint);

				
				ILabelController? lblCtrl = resizeCtrl.OwnerElement.GetLabelController();
				if (lblCtrl != null)
					lblCtrl.SetLabelPosition();
				else
				{
					if (resizeCtrl.OwnerElement is ILabelElement element)
					{
						LabelElement label = element.Label;
						label.PositionBySite(resizeCtrl.OwnerElement);
					}
				}

				UpdateResizeCorner();
			}
		}

		public void End(Point posEnd)
		{
			if (resizeCtrl != null)
			{
				resizeCtrl.OwnerElement.Invalidate();

				resizeCtrl.End(posEnd);

				//Events
				ElementEventArgs eventResizeArg = new(resizeCtrl.OwnerElement);
				if(onElementResizingDelegate is not null) onElementResizingDelegate(eventResizeArg);

				isResizing = false;
			}
		}

		public void DrawResizeCorner(Graphics g)
		{
      if (resizeCtrl is not null) {
				if (document is not null) { 
					foreach(RectangleElement r in resizeCtrl.Corners)
					{
						if (document.Action == DesignerAction.Select)
						{
							if (r.Visible) r.Draw(g);
						}
						else if (document.Action == DesignerAction.Connect)
						{
							// if is Connect Mode, then resize only Links.
							if (resizeCtrl.OwnerElement is BaseLinkElement)
								if (r.Visible) r.Draw(g);
						}
					}
				}
			}
		}

		public void UpdateResizeCorner()
		{
			if (resizeCtrl != null)
				resizeCtrl.UpdateCornersPos();
		}

		public Cursor UpdateResizeCornerCursor(Point mousePoint)
		{
			if ((resizeCtrl == null) || (!resizeCtrl.CanResize)) return Cursors.Default;

			CornerPosition corPos = resizeCtrl.HitTestCorner(mousePoint);

      return corPos switch {
        CornerPosition.TopLeft => Cursors.SizeNWSE,
        CornerPosition.TopCenter => Cursors.SizeNS,
        CornerPosition.TopRight => Cursors.SizeNESW,
        CornerPosition.MiddleLeft or CornerPosition.MiddleRight => Cursors.SizeWE,
        CornerPosition.BottomLeft => Cursors.SizeNESW,
        CornerPosition.BottomCenter => Cursors.SizeNS,
        CornerPosition.BottomRight => Cursors.SizeNWSE,
        _ => Cursors.Default,
      };
    }

    public void ShowResizeCorner(bool show)
		{
			if (resizeCtrl != null)
			{
				bool canResize = resizeCtrl.CanResize;
				for(int i = 0; i < resizeCtrl.Corners.Length; i++)
				{
					if (canResize)
						resizeCtrl.Corners[i].Visible = show;
					else
						resizeCtrl.Corners[i].Visible = false;
				}

				if (resizeCtrl.Corners.Length >= (int) CornerPosition.MiddleCenter)
					resizeCtrl.Corners[(int) CornerPosition.MiddleCenter].Visible = false;
			}
		}
  }
}
