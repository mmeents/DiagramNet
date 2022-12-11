using System;
using DiagramNet.Elements;

namespace DiagramNet.Events {
  public class ElementConnectEventArgs : EventArgs
  {
    private readonly NodeElement _node1;
    private readonly NodeElement? _node2;
    private readonly BaseLinkElement _link;

    public ElementConnectEventArgs(NodeElement node1, NodeElement? node2, BaseLinkElement link)
    {
      _node1 = node1;
      _node2 = node2;
      _link = link;
    }

    public NodeElement Node1 => _node1;

    public NodeElement? Node2 => _node2;

    public BaseLinkElement Link => _link;

    public override string ToString()
    {
      var str = "";
      if (_node1 != null)
        str = str + "Node1:" + _node1;
      if (_node2 != null)
        str = str + "Node2:" + _node2;
      if (_link != null)
        str = str + "Link:" + _link;
      return str;
    }
  }
}
