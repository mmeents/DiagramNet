using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiagramNet.Elements {
	/// <summary>
	/// This class is the controller for CommentBoxElement
	/// </summary>

	internal class CommentBoxController : RectangleController, ILabelController {
		public CommentBoxController(BaseElement element) : base(element) { }

		public void SetLabelPosition() {
			LabelElement label = ((ILabelElement)el).Label;
			label.Location = el.Location;
			label.Size = el.Size;
		}
	}
}
