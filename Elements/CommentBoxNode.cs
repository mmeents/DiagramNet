﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DiagramNet.Elements {
	[Serializable]
	public class CommentBoxNode : RectangleNode, IControllable, ILabelElement {
		protected Color fillColor1 = Color.White;
		protected Color fillColor2 = Color.DodgerBlue;
		[NonSerialized]
		private RectangleController? controller;

		protected Size foldSize = new(10, 15);

		public CommentBoxNode() : this(0, 0, 100, 100) { }

		public CommentBoxNode(Rectangle rec) : this(rec.Location, rec.Size) { }

		public CommentBoxNode(Point l, Size s) : this(l.X, l.Y, s.Width, s.Height) { }

		public CommentBoxNode(int top, int left, int width, int height) : base(top, left, width, height) {
			fillColor1 = Color.LemonChiffon;
			fillColor2 = Color.FromArgb(255, 255, 128);

			label.Opacity = 100;
		}

		public override Point Location {
			get {
				return base.Location;
			}
			set {
				base.Location = value;
			}
		}


		public override Size Size {
			get {
				return base.Size;
			}
			set {
				base.Size = value;
			}
		}
		protected virtual Brush GetBrush(Rectangle r) {
			//Fill rectangle
			Color fill1;
			Color fill2;
			Brush b;
			if (opacity == 100) {
				fill1 = fillColor1;
				fill2 = fillColor2;
			} else {
				fill1 = Color.FromArgb((int)(255.0f * (opacity / 100.0f)), fillColor1);
				fill2 = Color.FromArgb((int)(255.0f * (opacity / 100.0f)), fillColor2);
			}

			if (fillColor2 == Color.Empty)
				b = new SolidBrush(fill1);
			else {
				Rectangle rb = new(r.X, r.Y, r.Width + 1, r.Height + 1);
				b = new LinearGradientBrush(
						rb,
						fill1,
						fill2,
						LinearGradientMode.Horizontal);
			}

			return b;
		}

		internal override void Draw(Graphics g) {
			IsInvalidated = false;

			Rectangle r = BaseElement.GetUnsignedRectangle(new Rectangle(location, size));
			if (base.Backgroup != null) {
				g.DrawImage(base.Backgroup, r.X, r.Y, r.Width, r.Height);
			}
			Point[] points = new Point[5];
			points[0] = new Point(r.X + 0, r.Y + 0);
			points[1] = new Point(r.X + 0, r.Y + r.Height);
			points[2] = new Point(r.X + r.Width, r.Y + r.Height);

			//Fold
			points[3] = new Point(r.X + r.Width, r.Y + foldSize.Height);
			points[4] = new Point(r.X + r.Width - foldSize.Width, r.Y + 0);

			//foreach(Point p in points) p.Offset(location.X, location.Y);

			g.FillPolygon(GetBrush(r), points, FillMode.Alternate);
			g.DrawPolygon(new Pen(borderColor, borderWidth), points);

			g.DrawLine(new Pen(borderColor, borderWidth),
								 new Point(r.X + r.Width - foldSize.Width, r.Y + foldSize.Height),
								 new Point(r.X + r.Width, r.Y + foldSize.Height));

			g.DrawLine(new Pen(borderColor, borderWidth),
								 new Point(r.X + r.Width - foldSize.Width, r.Y + 0),
								 new Point(r.X + r.Width - foldSize.Width, r.Y + 0 + foldSize.Height));
		}

		IController IControllable.GetController() {
			if (controller == null)
				controller = new CommentBoxController(this);
			return controller;
		}

	}
}
