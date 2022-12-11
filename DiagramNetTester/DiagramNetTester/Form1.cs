using DiagramNet;
using DiagramNet.Elements;
using StaticExtensions;

namespace DiagramNetTester {
  public partial class Form1 : Form {
    public Form1() {
      InitializeComponent();
    }

    private void ButtonTest_Click(object sender, EventArgs e) {
    
      ElipseElement elipseElementDefault = new();
      elipseElementDefault.Location = new Point(43, 86);
      elipseElementDefault.BorderColor = Color.AliceBlue;
      elipseElementDefault.FillColor1 = Color.BlueViolet;
      elipseElementDefault.Name = "elipseDefault";
      ElipseElement elipseElement1 = new(new Rectangle(86, 86, 86, 86));
      elipseElement1.Name = "elipseElement1";
      ElipseElement elipseElement2 = new(new Point(172, 86), new Size(86, 86));
      elipseElement2.Name = "elipseElement2";
      ElipseElement elipseElement3 = new(86, 86*3, 86, 86);
      elipseElement3.Name = "elipseElement3";

      diagram1.Document.AddElement(elipseElementDefault);
      diagram1.Document.AddElement(elipseElement1 );
      diagram1.Document.AddElement(elipseElement2);
      diagram1.Document.AddElement(elipseElement3);
     
      diagram1.AutoScroll = true;
      diagram1.Refresh();
      Thread.Sleep(1000);

      string sFileName = DllExt.MMCommonsFolder()+ "\\testfilename.json";
      diagram1.Save(sFileName);
      

      diagram1.Document.SelectAllElements();
      diagram1.Document.ClearSelection();
      diagram1.Refresh();
      Thread.Sleep(500);

      diagram1.Open(sFileName);
     


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