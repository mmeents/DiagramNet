using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DiagramNet.Elements {
	/// <summary>
	/// This class is the controller for ElipseElement
	/// </summary>
	internal class ElipseController : RectangleController, IController {
		public ElipseController(BaseElement element) : base(element) {
		}

		#region IController Members

		public override bool HitTest(System.Drawing.Point p) {
			GraphicsPath gp = new();
			Matrix mtx = new();

			gp.AddEllipse(new Rectangle(el.Location.X,
				el.Location.Y,
				el.Size.Width,
				el.Size.Height));
			gp.Transform(mtx);

			return gp.IsVisible(p);
		}

		public override void DrawSelection(System.Drawing.Graphics g) {
			int border = 3;

			Rectangle r = BaseElement.GetUnsignedRectangle(
				new Rectangle(
					el.Location.X - border, el.Location.Y - border,
					el.Size.Width + (border * 2), el.Size.Height + (border * 2)));

			HatchBrush brush = new(HatchStyle.SmallCheckerBoard, Color.LightGray, Color.Transparent);
			Pen p = new(brush, border);
			g.DrawEllipse(p, r);

			p.Dispose();
			brush.Dispose();
		}

		#endregion
	}
}
