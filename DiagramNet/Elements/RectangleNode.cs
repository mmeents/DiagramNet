﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DiagramNet.Elements {
	[Serializable]
	public class RectangleNode : NodeElement, IControllable, ILabelElement {
		protected RectangleElement rectangle;
		protected LabelElement label = new();

		[NonSerialized]
		private RectangleController? controller;

		public RectangleNode() : this(0, 0, 100, 100) { }

		public RectangleNode(Rectangle rec) : this(rec.Location, rec.Size) { }

		public RectangleNode(Point l, Size s) : this(l.X, l.Y, s.Width, s.Height) { }

		public RectangleNode(int top, int left, int width, int height) : base(top, left, width, height) {
			rectangle = new RectangleElement(top, left, width, height);
			SyncContructors();
		}

		public override Color BorderColor {
			get {
				return base.BorderColor;
			}
			set {
				rectangle.BorderColor = value;
				base.BorderColor = value;
			}
		}

		public Color FillColor1 {
			get {
				return rectangle.FillColor1;
			}
			set {
				rectangle.FillColor1 = value;
			}
		}

		public Color FillColor2 {
			get {
				return rectangle.FillColor2;
			}
			set {
				rectangle.FillColor2 = value;
			}
		}

		public override int Opacity {
			get {
				return base.Opacity;
			}
			set {
				rectangle.Opacity = value;
				base.Opacity = value;
			}
		}

		public override bool Visible {
			get {
				return base.Visible;
			}
			set {
				rectangle.Visible = value;
				base.Visible = value;
			}
		}

		public override Point Location {
			get {
				return base.Location;
			}
			set {
				label.Location = value;
				rectangle.Location = value;
				base.Location = value;
				OnAppearanceChanged(new EventArgs());
			}
		}

		public override Size Size {
			get {
				return base.Size;
			}
			set {
				rectangle.Size = value;
				base.Size = value;
			}
		}

		public override int BorderWidth {
			get {
				return base.BorderWidth;
			}
			set {
				rectangle.BorderWidth = value;
				base.BorderWidth = value;
			}
		}
		Image? __Backupgroup = null;
		[DefaultValue(null)]
		public Image? Backgroup {
			get { return __Backupgroup; }
			set {
				rectangle.Background = value;
				__Backupgroup = value;

			}
		}
		public virtual LabelElement Label {
			get {
				return label;
			}
			set {
				label = value;
				OnAppearanceChanged(new EventArgs());
			}
		}

		private void SyncContructors() {
			base.location = rectangle.Location;
			base.size = rectangle.Size;
			base.borderColor = rectangle.BorderColor;
			base.borderWidth = rectangle.BorderWidth;
			base.opacity = rectangle.Opacity;
			base.visible = rectangle.Visible;
		}

		internal override void Draw(Graphics g) {
			IsInvalidated = false;

			rectangle.Draw(g);
		}

		IController IControllable.GetController() {
			if (controller == null)
				controller = new RectangleController(this);
			return controller;
		}

	}
}
