using RodSoft.OSM.Tracking;
using System.Windows.Controls;

namespace RodSoft.OSM.UI.Controls
{
    public abstract class TooltipBase : UserControl
    {
        public abstract void SetValues(VehicleGeoData data);
    }
}
