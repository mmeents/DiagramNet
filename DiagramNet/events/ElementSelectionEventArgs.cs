using System;

namespace DiagramNet.Events {
  public class ElementSelectionEventArgs : EventArgs {
    private readonly ElementCollection _elements;

    public ElementSelectionEventArgs(ElementCollection elements) => _elements = elements;

    public ElementCollection Elements => _elements;

    public override string ToString() => "ElementCollection: " + _elements.Count;
  }

}