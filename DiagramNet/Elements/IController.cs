using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiagramNet.Elements {

	public interface IContainer {
		ElementCollection Elements {
			get;
		}
	}
	/// <summary>
	/// To be a controller, needs to implements this interface.
	/// </summary>
	internal interface IController {
		BaseElement OwnerElement { get; }

		bool HitTest(Point p);

		bool HitTest(Rectangle r);

		void DrawSelection(Graphics g);
	}

	/// <summary>
	/// When a class implements this interface, then it can be controlled.
	/// </summary>
	internal interface IControllable {
		IController GetController();
	}

	/// <summary>
	/// If a class controller implements this interface, it can move the element.
	/// </summary>
	internal interface IMoveController : IController {
		bool IsMoving { get; }

		void Start(Point posStart);
		void Move(Point posCurrent);
		void End();

		bool CanMove { get; }
	}

	/// <summary>
	/// If a class controller implements this interface, it can resize the element.
	/// </summary>
	internal interface IResizeController : IController {
		RectangleElement[] Corners { get; }
		void UpdateCornersPos();

		CornerPosition HitTestCorner(Point p);

		void Start(Point posStart, CornerPosition corner);
		void Resize(Point posCurrent);
		void End(Point posEnd);

		bool IsResizing { get; }

		bool CanResize { get; }

	}

	public interface ILabelElement {
		LabelElement Label { get; set; }
	}
	/// <summary>
	/// If a class controller implements this interface, then it can control
	/// the label inside the element.
	/// Without this interface, the label will be controled be default controller.
	/// </summary>
	public interface ILabelController {
		void SetLabelPosition();
	}


}
