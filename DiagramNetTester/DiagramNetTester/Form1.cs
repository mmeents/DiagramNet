using DiagramNet;
using DiagramNet.Elements;
using StaticExtensions;

namespace DiagramNetTester {
  public partial class Form1 : Form {
    public Form1() {
      InitializeComponent();
    }

    private void ButtonTest_Click(object sender, EventArgs e) {
      Document emptyDocument = Desiger1.Document;
    
      ElipseElement elipseElementDefault = new();
      elipseElementDefault.Location = new Point(43, 86);
      elipseElementDefault.BorderColor = Color.AliceBlue;
      elipseElementDefault.FillColor1 = Color.BlueViolet;
      ElipseElement elipseElement1 = new ElipseElement(new Rectangle(86, 86, 86, 86));
      ElipseElement elipseElement2 = new ElipseElement(new Point(172, 86), new Size(86, 86));
      ElipseElement elipseElement3 = new ElipseElement(86, 86*3, 86, 86);
      
      Desiger1.Document.AddElement(elipseElementDefault);
      Desiger1.Document.AddElement(elipseElement1 );
      Desiger1.Document.AddElement(elipseElement2);
      Desiger1.Document.AddElement(elipseElement3);

      //textBox1.Text = emptyDocument.AsJsonString();
      Desiger1.AutoScroll = true;
      Desiger1.BackColor = Color.LightGray;
      Desiger1.Refresh();
      /*
      { "Elements":[
          { "Location":"43, 86","Size":"100, 100","FillColor1":"White","FillColor2":"DodgerBlue","Label":"DiagramNet.Elements.LabelElement","Background":null,"Name":null,"Visible":true,"BorderColor":"AliceBlue","BorderWidth":1,"Opacity":100},
          { "Location":"86, 86","Size":"86, 86","FillColor1":"White","FillColor2":"DodgerBlue","Label":"DiagramNet.Elements.LabelElement","Background":null,"Name":null,"Visible":true,"BorderColor":"Black","BorderWidth":1,"Opacity":100},
          { "Location":"172, 86","Size":"86, 86","FillColor1":"White","FillColor2":"DodgerBlue","Label":"DiagramNet.Elements.LabelElement","Background":null,"Name":null,"Visible":true,"BorderColor":"Black","BorderWidth":1,"Opacity":100},
          { "Location":"86, 258","Size":"86, 86","FillColor1":"White","FillColor2":"DodgerBlue","Label":"DiagramNet.Elements.LabelElement","Background":null,"Name":null,"Visible":true,"BorderColor":"Black","BorderWidth":1,"Opacity":100}],
        "SelectedElements":[],"SelectedNodes":[],"Location":"43, 86","Size":"215, 258","SmoothingMode":2,"PixelOffsetMode":0,"CompositingQuality":4,"Action":0,"Zoom":1.0,"ElementType":1,"LinkType":1,"GridSize":"50, 50"}

      */
    }
  }
}