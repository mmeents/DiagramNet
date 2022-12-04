using System;
using DiagramNet.Elements;

namespace DiagramNet.Events {
	public class ElementEventArgs: EventArgs
	{
		private BaseElement element;

		public ElementEventArgs(BaseElement el)
		{
			element = el;
		}

		public BaseElement Element
		{
			get
			{
				return element;
			}
		}

		public override string ToString()
		{
			return "el: " + element.GetHashCode();
		}


	}
}
