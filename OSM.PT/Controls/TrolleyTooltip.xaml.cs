using System.Windows.Controls;
using GMap.NET;
using Demo.WindowsForms;

namespace Demo.WindowsPresentation.Controls
{
   /// <summary>
   /// Interaction logic for TrolleyTooltip.xaml
   /// </summary>
   public partial class TrolleyTooltip : UserControl
   {
      public TrolleyTooltip()
      {
         InitializeComponent();
      }

      public void SetValues(VehicleData vl)
      {
         LineNum.Text = vl.Line;
         Operator.Text = vl.Operator;
         labelMarshrutType.Text = vl.Type;
         Number.Text = vl.Number;
//         TrackType.Text = vl.TrackType;
//         TimeGps.Text = vl.Time;
//         Area.Text = vl.AreaName;
         Speed.Text = vl.Speed;
         Time.Text = vl.Time;
      }
   }
}
