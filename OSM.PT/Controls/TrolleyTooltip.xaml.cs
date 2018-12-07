using System.Windows.Controls;
using GMap.NET;
using Demo.WindowsForms;
using RodSoft.OSMPT.PT.Online;
using RodSoft.OSM.Controls;

namespace RodSoft.OSM.PT.Controls
{
    /// <summary>
    /// Interaction logic for TrolleyTooltip.xaml
    /// </summary>
    public partial class TrolleyTooltip : TooltipBase
    {
        public TrolleyTooltip()
        {
            InitializeComponent();
        }

        public override void SetValues(VehicleData vl)
        {
            if (vl != null)
            {
//                LineNum.Text = vl.Line;
                Operator.Text = vl.Operator;
                labelMarshrutType.Text = OSMOTHelper.GetRouteTypeName(vl.RouteType) + " " + vl.Line;
                Number.Text = vl.Number;
                //         TrackType.Text = vl.TrackType;
                //         TimeGps.Text = vl.Time;
                //         Area.Text = vl.AreaName;
                Speed.Text = vl.Speed.ToString();
                Time.Text = vl.Time.ToLongTimeString();
            }
        }

    }
}
