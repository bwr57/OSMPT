using RodSoft.Core.Communications;
using System;

namespace RodSoft.OSM.Tracking
{
    [Serializable]
    public class GPSDataMessage : ActivatedControllerAgent, IVehicleGeoData
    {
        [Transmitted(FormatString = "0.#######", MessageIdentificator ="G1")]
        public double Latitude{ get; set; }
        [Transmitted(FormatString = "0.#######", MessageIdentificator = "G2")]
        public double Longitude{ get; set; }
        [Transmitted(FormatString = "0.#", MessageIdentificator = "G3")]
        public Single Altitude{ get; set; }
        [Transmitted(FormatString = "0", MessageIdentificator = "G4")]
        public short Speed{ get; set; }
        [Transmitted(FormatString = "0", MessageIdentificator = "G5")]
        public short Course{ get; set; }

        public GPSDataMessage()
        { }

        //public GPSDataMessage(object source)
        //    : base(source)
        //{ }
    }
}
