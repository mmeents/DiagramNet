using System;
using DiagramNet.Elements;

namespace DiagramNet.Events {
  public class ElementEventArgs : EventArgs {
    private readonly BaseElement _element;
    private readonly BaseElement? _previousElement;

    public ElementEventArgs(BaseElement el) => _element = el;

    public ElementEventArgs(BaseElement el, BaseElement previousEl)
    {
    _element = el;
    _previousElement = previousEl;
    }

    public BaseElement Element => _element;

    public BaseElement? PreviousElement => _previousElement;

    public override string ToString() => "el: " + _element.GetHashCode();
  }
}
