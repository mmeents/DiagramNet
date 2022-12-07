using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using DiagramNet.Elements;
using DiagramNet.Events;


namespace DiagramNet.Actions
{
	/// <summary>
	/// This class control the elements motion.
	/// </summary>
	
	internal class MoveAction
	{
		public delegate void OnElementMovingDelegate(ElementEventArgs e);
		private OnElementMovingDelegate? onElementMovingDelegate;

		private bool isMoving = false;
		private IMoveController?[]? moveCtrl;
		private Point upperSelPoint = Point.Empty;
		private Point upperSelPointDragOffset = Point.Empty;
		private Document? document = null;

		public MoveAction()
		{
		}
		
		public bool IsMoving
		{
			get
			{
				return isMoving;
			}
		}

		public void Start(Point mousePoint, Document document, OnElementMovingDelegate onElementMovingDelegate)
		{
			this.document = document;
			this.onElementMovingDelegate = onElementMovingDelegate;

			// Get Controllers
			moveCtrl = new IMoveController?[document.SelectedElements.Count];
			IMoveController?[] moveLabelCtrl = new IMoveController?[document.SelectedElements.Count];
			for(int i = document.SelectedElements.Count - 1; i >= 0; i--)
			{
				var elementCollection = document?.SelectedElements[i];
				var ctrl = elementCollection?.GetMoveController();
				moveCtrl[i] = ctrl;
				
				if ((ctrl != null) && (ctrl.CanMove)&&(elementCollection is not null))
				{
					onElementMovingDelegate(new ElementEventArgs(elementCollection));
					ctrl.Start(mousePoint);
					
					if ((document is not null) &&
						  (document.SelectedElements[i] is ILabelElement) &&
						  (document.SelectedElements[i]?.GetLabelController() == null))
					{						
						LabelElement label = ((ILabelElement) elementCollection).Label;
						var moveCtrl = label.GetMoveController();
						moveLabelCtrl[i] = moveCtrl;
						if ((moveCtrl != null) && (moveCtrl.CanMove))
							moveCtrl.Start(mousePoint);
						else
							moveLabelCtrl[i] = null;
					}
				}
				else
					moveCtrl[i] = null;
			}

			moveCtrl = (IMoveController[]) DiagramUtil.ArrayHelper.Append(moveCtrl, moveLabelCtrl);
			moveCtrl = (IMoveController[]) DiagramUtil.ArrayHelper.Shrink(moveCtrl, null);

			// Can't move only links
			bool isOnlyLink = true;
			foreach (IMoveController? ctrl in moveCtrl)
			{
				// Verify
				if (ctrl != null)
				{
					ctrl.OwnerElement.Invalidate();

					if (ctrl.OwnerElement is not BaseLinkElement && ctrl.OwnerElement is not LabelElement)
					{
						isOnlyLink = false;
						break;
					}
				}
			}
			if (isOnlyLink)
			{
				//End Move the Links
				foreach (IMoveController? ctrl in moveCtrl)
				{
					if (ctrl !=null)
						ctrl.End();
				}
				moveCtrl = new IMoveController?[] { null };
			}

			//Upper selecion point controller
			UpdateUpperSelectionPoint();
			upperSelPointDragOffset.X = upperSelPoint.X - mousePoint.X;
			upperSelPointDragOffset.Y = upperSelPoint.Y - mousePoint.Y;

			isMoving = true;
		}

		public void Move(Point dragPoint)
		{
			//Upper selecion point controller
			Point dragPointEl = dragPoint;
			dragPointEl.Offset(upperSelPointDragOffset.X, upperSelPointDragOffset.Y);
					
			upperSelPoint = dragPointEl;
					
			if (dragPointEl.X < 0) dragPointEl.X = 0;
			if (dragPointEl.Y < 0) dragPointEl.Y = 0;

			//Move Controller
			if (dragPointEl.X == 0) dragPoint.X -= upperSelPoint.X;					
			if (dragPointEl.Y == 0) dragPoint.Y -= upperSelPoint.Y;

			if(moveCtrl is not null) { 
				foreach(IMoveController? ctrl in moveCtrl){
					if (ctrl != null)
					{
						BaseElement controlsOwner = ctrl.OwnerElement as BaseElement;
						controlsOwner.Invalidate();

						if (onElementMovingDelegate is not null) {
							onElementMovingDelegate(new ElementEventArgs(ctrl.OwnerElement));
						}
					
						ctrl.Move(dragPoint);

						if (controlsOwner is NodeElement)
						{
							UpdateLinkPosition((NodeElement) ctrl.OwnerElement);
						}
						
						ILabelController? lblCtrl = controlsOwner.GetLabelController();
						if (lblCtrl != null)
							lblCtrl.SetLabelPosition();
					}
				}
			}
		}

		public void End()
		{
			upperSelPoint = Point.Empty;
			upperSelPointDragOffset = Point.Empty;
				
//			ElementEventArgs eventClickArg = new ElementEventArgs(selectedElement);
//			OnElementClick(eventClickArg);

			if (moveCtrl is not null) { 
				foreach(IMoveController? ctrl in moveCtrl)
				{
					if (ctrl !=null)
					{
						if (ctrl.OwnerElement is NodeElement element)
						{
							UpdateLinkPosition(element);
						}

						ctrl.End();

						if (onElementMovingDelegate is not null) onElementMovingDelegate(new ElementEventArgs(ctrl.OwnerElement));
					}
				}
			}

			isMoving = false;

//			ElementMouseEventArgs eventMouseUpArg = new ElementMouseEventArgs(selectedElement, e.X, e.Y);
//			OnElementMouseUp(eventMouseUpArg);
		}

		private void UpdateUpperSelectionPoint()
		{
			if (document is not null) { 				
				Point[] points = new Point[document.SelectedElements.Count];
				int p = 0;
				foreach(BaseElement el in document.SelectedElements)
				{
					points[p] = el.Location;
					p++;
				}
				upperSelPoint = DiagramUtil.GetUpperPoint(points);
			}
		}

		private static void UpdateLinkPosition(NodeElement node)
		{
			foreach(ConnectorElement conn in node.Connectors)
			{
				foreach (BaseElement el in conn.Links)
				{
					BaseLinkElement lnk = (BaseLinkElement) el;
					IController ctrl = ((IControllable) lnk).GetController();
          if (ctrl is IMoveController mctrl) {
            if (!mctrl.IsMoving) lnk.NeedCalcLink = true;
          } else lnk.NeedCalcLink = true;

          if (lnk is ILabelElement element)
					{
						LabelElement label = element.Label;

						ILabelController? lblCtrl = lnk.GetLabelController();
						if (lblCtrl != null)
							lblCtrl.SetLabelPosition();
						else
						{
							label.PositionBySite(lnk);
						}		
						label.Invalidate();
					}
				}
			}
		}
	}
}
