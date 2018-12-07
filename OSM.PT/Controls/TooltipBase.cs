using RodSoft.OSMPT.PT.Online;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace RodSoft.OSM.Controls
{
    public abstract class TooltipBase : UserControl
    {
        public abstract void SetValues(VehicleData data);
    }
}
