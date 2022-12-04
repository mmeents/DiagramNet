using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;

namespace DiagramNet {
  public partial class Designer : UserControl {
    public Designer() {
      InitializeComponent();

      // This change control to not flick
      this.SetStyle(ControlStyles.UserPaint, true);
      this.SetStyle(ControlStyles.ResizeRedraw, true);
      this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      this.SetStyle(ControlStyles.DoubleBuffer, true);

      // Selection Area Properties
      selectionArea.Opacity = 40;
      selectionArea.FillColor1 = SystemColors.Control;
      selectionArea.FillColor2 = Color.Empty;
      selectionArea.BorderColor = SystemColors.Control;

      // Link Line Properties
      //linkLine.BorderColor = Color.FromArgb(127, Color.DarkGray);
      //linkLine.BorderWidth = 4;

      // Label Edit
      labelTextBox.BorderStyle = BorderStyle.FixedSingle;
      labelTextBox.Multiline = true;
      labelTextBox.Hide();
      this.Controls.Add(labelTextBox);

      //EventsHandlers
      RecreateEventsHandlers();
    }



  }
}