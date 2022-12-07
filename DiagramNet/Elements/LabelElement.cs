using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Reflection;

namespace DiagramNet.Elements {
	[
	Serializable,
	TypeConverter(typeof(ExpandableObjectConverter))
	]
	public class LabelElement : BaseElement, ISerializable, IControllable {
		protected Color foreColor1 = Color.Black;
		protected Color foreColor2 = Color.Empty;

		protected Color backColor1 = Color.Empty;
		protected Color backColor2 = Color.Empty;

		[NonSerialized]
		private RectangleController? controller;

		protected string text = "";

		protected bool autoSize = false;

		[NonSerialized]
		protected Font font = new(FontFamily.GenericSansSerif, 10);

		[NonSerialized]
		private readonly StringFormat format = new(StringFormatFlags.NoWrap);

		private StringAlignment alignment;
		private StringAlignment lineAlignment;
		private StringTrimming trimming;
		private bool wrap;
		private bool vertical;
		protected bool readOnly = false;

		public LabelElement() : this(0, 0, 100, 100) { }

		public LabelElement(Rectangle rec) : this(rec.Location, rec.Size) { }

		public LabelElement(Point l, Size s) : this(l.X, l.Y, s.Width, s.Height) { }

		public LabelElement(int top, int left, int width, int height) : base(top, left, width, height) {
			this.Alignment = StringAlignment.Center;
			this.LineAlignment = StringAlignment.Center;
			this.Trimming = StringTrimming.Character;
			this.Vertical = false;
			this.Wrap = true;
			borderColor = Color.Transparent;
		}

		#region Properties
		public string Text {
			get {
				return text;
			}
			set {
				text = value;
				if (autoSize) DoAutoSize();
				OnAppearanceChanged(new EventArgs());
			}
		}

		public Font Font {
			get {
				return font;
			}
			set {
				font = value;
				if (autoSize) DoAutoSize();
				OnAppearanceChanged(new EventArgs());
			}
		}

		public StringAlignment Alignment {
			get {
				return alignment;
			}
			set {
				alignment = value;
				format.Alignment = alignment;
				if (autoSize) DoAutoSize();
				OnAppearanceChanged(new EventArgs());
			}
		}

		public StringAlignment LineAlignment {
			get {
				return lineAlignment;
			}
			set {
				lineAlignment = value;
				format.LineAlignment = lineAlignment;
				if (autoSize) DoAutoSize();
				OnAppearanceChanged(new EventArgs());
			}
		}

		public StringTrimming Trimming {
			get {
				return trimming;
			}
			set {
				trimming = value;
				format.Trimming = trimming;
				if (autoSize) DoAutoSize();
				OnAppearanceChanged(new EventArgs());
			}
		}

		public bool Wrap {
			get {
				return wrap;
			}
			set {
				wrap = value;
				if (wrap)
					format.FormatFlags &= ~StringFormatFlags.NoWrap;
				else
					format.FormatFlags |= StringFormatFlags.NoWrap;

				if (autoSize) DoAutoSize();
				OnAppearanceChanged(new EventArgs());
			}
		}

		public bool Vertical {
			get {
				return vertical;
			}
			set {
				vertical = value;
				if (vertical)
					format.FormatFlags |= StringFormatFlags.DirectionVertical;
				else
					format.FormatFlags &= ~StringFormatFlags.DirectionVertical;

				if (autoSize) DoAutoSize();
				OnAppearanceChanged(new EventArgs());
			}

		}

		public bool ReadOnly {
			get {
				return readOnly;
			}
			set {
				readOnly = value;

				OnAppearanceChanged(new EventArgs());
			}
		}

		public virtual Color ForeColor1 {
			get {
				return foreColor1;
			}
			set {
				foreColor1 = value;
				OnAppearanceChanged(new EventArgs());
			}
		}

		public virtual Color ForeColor2 {
			get {
				return foreColor2;
			}
			set {
				foreColor2 = value;
				OnAppearanceChanged(new EventArgs());
			}
		}

		public virtual Color BackColor1 {
			get {
				return backColor1;
			}
			set {
				backColor1 = value;
				OnAppearanceChanged(new EventArgs());
			}
		}

		public virtual Color BackColor2 {
			get {
				return backColor2;
			}
			set {
				backColor2 = value;
				OnAppearanceChanged(new EventArgs());
			}
		}

		public virtual bool AutoSize {
			get {
				return autoSize;
			}
			set {
				autoSize = value;
				if (autoSize) DoAutoSize();
				OnAppearanceChanged(new EventArgs());
			}
		}

		public override Size Size {
			get {
				return base.Size;
			}
			set {
				size = value;
				if (autoSize) DoAutoSize();
				base.Size = size;
			}
		}

		internal StringFormat Format {
			get {
				return format;
			}
		}

		#endregion

		public void DoAutoSize() {
			if (text.Length == 0) return;

			Bitmap bmp = new(1, 1);
			Graphics g = Graphics.FromImage(bmp);
			SizeF sizeF = g.MeasureString(text, font, size.Width, format);
			Size sizeTmp = Size.Round(sizeF);

			if (size.Height < sizeTmp.Height)
				size.Height = sizeTmp.Height;
		}

		protected virtual Brush GetBrushBackColor(Rectangle r) {
			//Fill rectangle
			Color fill1;
			Color fill2;
			Brush b;
			if (opacity == 100) {
				fill1 = backColor1;
				fill2 = backColor2;
			} else {
				fill1 = Color.FromArgb((int)(255.0f * (opacity / 100.0f)), backColor1);
				fill2 = Color.FromArgb((int)(255.0f * (opacity / 100.0f)), backColor2);
			}

			if (backColor2 == Color.Empty)
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

		protected virtual Brush GetBrushForeColor(Rectangle r) {
			//Fill rectangle
			Color fill1;
			Color fill2;
			Brush b;
			if (opacity == 100) {
				fill1 = foreColor1;
				fill2 = foreColor2;
			} else {
				fill1 = Color.FromArgb((int)(255.0f * (opacity / 100.0f)), foreColor1);
				fill2 = Color.FromArgb((int)(255.0f * (opacity / 100.0f)), foreColor2);
			}

			if (foreColor2 == Color.Empty)
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

		internal override void Draw(System.Drawing.Graphics g) {
			Rectangle r = GetUnsignedRectangle();

			g.FillRectangle(GetBrushBackColor(r), r);
			Brush b = GetBrushForeColor(r);
			g.DrawString(text, font, b, (RectangleF)r, format);
			DrawBorder(g, r);
			b.Dispose();
		}

		protected virtual void DrawBorder(Graphics g, Rectangle r) {
			//Border
			Pen p = new(borderColor, borderWidth);
			g.DrawRectangle(p, r);
			p.Dispose();
		}

		#region ISerializable Members
		protected LabelElement(SerializationInfo info, StreamingContext context) {

			// Get the set of serializable members for our class and base classes
			Type thisType = typeof(LabelElement);
			MemberInfo[] mi = FormatterServices.GetSerializableMembers(thisType, context);

			// Deserialize the base class's fields from the info object
			for (Int32 i = 0; i < mi.Length; i++) {
				// Don't deserialize fields for this class
				if (mi[i].DeclaringType == thisType) continue;

				// To ease coding, treat the member as a FieldInfo object
				FieldInfo fi = (FieldInfo)mi[i];

				// Set the field to the deserialized value
				fi.SetValue(this, info.GetValue(fi.Name, fi.FieldType));
			}

			// Deserialize the values that were serialized for this class
			object? aFC1 = info.GetValue("foreColor1", typeof(Color));
			if (aFC1 != null) {
				this.ForeColor1 = (Color)aFC1;
			} else {
				this.ForeColor1 = Color.Black;
      }
			
			object? aFC2 = info.GetValue("foreColor2", typeof(Color));
			if (aFC2 is not null) { 
			  this.ForeColor2 = (Color)aFC2;
			} else { 
				this.ForeColor2 = Color.Blue;
		  }

			object? aBC1 = info.GetValue("backColor1", typeof(Color));
			if (aBC1 is not null) { 
				this.backColor1 = (Color)aBC1;
			} else { 
				this.backColor1 = Color.White;
			}

			object? aBC2 = info.GetValue("backColor1", typeof(Color));
			if (aBC2 is not null) {
				this.backColor2 = (Color)aBC2;
			} else {
				this.backColor2 = Color.White;
			}
			
			this.Text = info.GetString("text") ?? "";

			object? aSA = info.GetValue("alignment", typeof(StringAlignment));
			if (aSA is not null) {
				this.Alignment = (StringAlignment)aSA;
			} else {
				this.Alignment = StringAlignment.Center;
			}

			object? aLA = info.GetValue("lineAlignment", typeof(StringAlignment));
			if (aLA is not null) {
				this.LineAlignment = (StringAlignment)aLA;
			} else {
				this.LineAlignment = StringAlignment.Center;
			}

			object? aST = info.GetValue("trimming", typeof(StringTrimming));
			if (aST is not null) {
				this.Trimming = (StringTrimming)aST;
			} else {
				this.Trimming = StringTrimming.None;
			}
			
			this.Wrap = info.GetBoolean("wrap");
			this.Vertical = info.GetBoolean("vertical");
			this.ReadOnly = info.GetBoolean("readOnly");
			this.AutoSize = info.GetBoolean("autoSize");

			FontConverter fc = new();
			string fontString = info.GetString("font") ?? "";
			if (fontString != "") { 
				Font? thisFont = (Font?)fc.ConvertFromString(fontString);
				if (thisFont != null) {
					this.Font = thisFont;
				} else {
					this.Font =	SystemFonts.DefaultFont;
				}
			}
		}

//		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
			// Serialize the desired values for this class
			info.AddValue("foreColor1", foreColor1);
			info.AddValue("foreColor2", foreColor2);
			info.AddValue("backColor1", backColor1);
			info.AddValue("backColor2", backColor2);
			info.AddValue("text", text);
			info.AddValue("alignment", alignment);
			info.AddValue("lineAlignment", lineAlignment);
			info.AddValue("trimming", trimming);
			info.AddValue("wrap", wrap);
			info.AddValue("vertical", vertical);
			info.AddValue("readOnly", readOnly);
			info.AddValue("autoSize", autoSize);

			FontConverter fc = new();
			info.AddValue("font", fc.ConvertToString(font));

			// Get the set of serializable members for our class and base classes
			Type thisType = typeof(LabelElement);
			MemberInfo[] mi = FormatterServices.GetSerializableMembers(thisType, context);

			// Serialize the base class's fields to the info object
			for (int i = 0; i < mi.Length; i++) {
				// Don't serialize fields for this class
				if (mi[i].DeclaringType == thisType) continue;
				info.AddValue(mi[i].Name, ((FieldInfo)mi[i]).GetValue(this));
			}
		}
		#endregion

		IController IControllable.GetController() {
			if (controller == null)
				controller = new RectangleController(this);
			return controller;
		}

		internal void PositionBySite(BaseElement site) {
			Point newLocation = Point.Empty;

			Point siteLocation = site.Location;
			Size siteSize = site.Size;
			Size thisSize = this.Size;

			newLocation.X = (siteLocation.X + (siteSize.Width / 2)) - (thisSize.Width / 2);
			newLocation.Y = (siteLocation.Y + (siteSize.Height / 2)) - (thisSize.Height / 2);

			this.Location = newLocation;
		}
	}
}
