﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiagramNet.Elements {
	/// <summary>
	/// This is the base for all element the will be draw on the
	/// document.
	/// </summary>
	[Serializable]
	public abstract class BaseElement {
		protected Point location;
		protected Size size;
		protected bool visible = true;
		protected Color borderColor = Color.Black;
		protected int borderWidth = 1;
		protected int opacity = 100;
		internal protected Rectangle invalidateRec = Rectangle.Empty;
		internal protected bool IsInvalidated = true;

		protected BaseElement() {
			location = new Point(0, 0);
			size = new Size(0, 0);
		}

		protected BaseElement(int top, int left, int width, int height) {
			location = new Point(top, left);
			size = new Size(width, height);
		}
		public string Name { get; set; } ="";
		public virtual Point Location {
			get {
				return location;
			}
			set {
				location = value;
        OnAppearanceChanged(EventArgs.Empty);
			}
		}

		public virtual Size Size {
			get {
				return size;
			}
			set {
				size = value;
        OnAppearanceChanged(EventArgs.Empty);
			}
		}

		public virtual bool Visible {
			get {
				return visible;
			}
			set {
				visible = value;
        OnAppearanceChanged(EventArgs.Empty);
			}
		}

		public virtual Color BorderColor {
			get {
				return borderColor;
			}
			set {
				borderColor = value;
        OnAppearanceChanged(EventArgs.Empty);
			}
		}

		public virtual int BorderWidth {
			get {
				return borderWidth;
			}
			set {
				borderWidth = value;
        OnAppearanceChanged(EventArgs.Empty);
			}
		}

		public virtual int Opacity {
			get {
				return opacity;
			}
			set {
				if ((value >= 0) || (value <= 100))
					opacity = value;
				else
					throw new Exception("'" + value + "' is not a valid value for 'Opacity'. 'Opacity' should be between 0 and 100.");

        OnAppearanceChanged(EventArgs.Empty);
			}
		}
		internal virtual void Draw(Graphics g) {
			IsInvalidated = false;
		}


		public virtual void Invalidate() {
			if (IsInvalidated)
				invalidateRec = Rectangle.Union(invalidateRec, GetUnsignedRectangle());
			else
				invalidateRec = GetUnsignedRectangle();

			IsInvalidated = true;
		}

		public virtual Rectangle GetRectangle() {
			return new Rectangle(this.Location, this.Size);
		}

		public virtual Rectangle GetUnsignedRectangle() {

			return GetUnsignedRectangle(GetRectangle());
		}

		internal static Rectangle GetUnsignedRectangle(Rectangle rec) {
			Rectangle retRectangle = rec;
			if (rec.Width < 0) {
				retRectangle.X = rec.X + rec.Width;
				retRectangle.Width = -rec.Width;
			}

			if (rec.Height < 0) {
				retRectangle.Y = rec.Y + rec.Height;
				retRectangle.Height = -rec.Height;
			}

			return retRectangle;
		}

		#region Events
		[field: NonSerialized]
		public event EventHandler? AppearanceChanged;

		protected virtual void OnAppearanceChanged(EventArgs e) {
      AppearanceChanged?.Invoke(this, e);
    }
		#endregion

	}
}
