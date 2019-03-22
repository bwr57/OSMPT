using RodSoft.Core.Communications;
using System;

namespace RodSoft.OSM.Tracking
{
    [Serializable]
    public class GPSDataMessage : CashedMessage, IVehicleGeoData
    {
        [Transmitted(FormatString = "0.#######", MessageIdentificator ="G1")]
        public double Latitude{ get; set; }
        [Transmitted(FormatString = "0.#######", MessageIdentificator = "G2")]
        public double Longitude{ get; set; }
        [Transmitted(FormatString = "0.#", MessageIdentificator = "G3")]
        public double Altitude{ get; set; }
        [Transmitted(FormatString = "0", MessageIdentificator = "G4")]
        public double Speed{ get; set; }
        [Transmitted(FormatString = "0", MessageIdentificator = "G5")]
        public double Course{ get; set; }

        public GPSDataMessage()
        { }

        //public GPSDataMessage(object source)
        //    : base(source)
        //{ }
    }
}
