using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Demo.WindowsPresentation.CustomMarkers;
using GMap.NET.WindowsPresentation;
using RodSoft.OSM.Controls;
using RodSoft.OSM.PT.Controls;
using RodSoft.OSMPT.PT.Online;

namespace RodSoft.OSM.PT.Online.Controls
{
    public class PTVehicleVisual : CircleVisual
    {
        private VehicleData _VehicleData;
        public VehicleData VehicleData
        {
            get { return _VehicleData; }
            set
            {
                _VehicleData = value;
                Text = value.Line;
                Angle = value.Bearing;

                if (Popup != null)
                {
                    _Tooltip.SetValues(value);
                }
            }
        }

        protected TrolleyTooltip _Tooltip;
        public PTVehicleVisual(GMapMarker m, Brush background, VehicleData vehicleData) 
            : base(m, background)
        {
            VehicleData = vehicleData;
        }

        protected override TooltipBase CreateTooltip()
        {
            _Tooltip = new TrolleyTooltip();
            return _Tooltip;
        }

        protected override void ProcessMouseEnter()
        {
            if (Popup != null)
            {
                _Tooltip.SetValues(_VehicleData);
            }
            base.ProcessMouseEnter();
        }
    }
}
