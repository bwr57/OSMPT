using RodSoft.Core.Communications;
using System;

namespace RodSoft.OSM.Tracking
{
    [Serializable]
    public class GPSDataMessage : MessageBase, IVehicleGeoData
    {
        public double Latitude{ get; set; }
        public double Longitude{ get; set; }
        public double Altitude{ get; set; }
        public double Speed{ get; set; }
        public double Course{ get; set; }

        public GPSDataMessage()
        { }

        public GPSDataMessage(object source)
            : base(source)
        { }
    }
}
