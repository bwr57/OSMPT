using OsmSharp.Tools.Math.Geo;
using RodSoft.Core.Communications;
using System;

namespace RodSoft.OSM.Tracking
{
    [Serializable]
    public class VehicleGeoData : GeoCoordinate, IVehicleGeoData
    {
        public DateTime Time{ get; set; }
        [Transmitted(PropertyIndex =4, FormatString ="0")]
        public double Speed{ get; set; }
        [Transmitted(PropertyIndex = 5, FormatString = "0")]
        public double Course{ get; set; }
        [Transmitted(PropertyIndex = 3, FormatString = "0.#")]
        public double Altitude{ get; set; }

        /// <summary>
        /// Creates a geo coordinate.
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        public VehicleGeoData(double latitude, double longitude)
            : base(latitude, longitude)
        {

        }


    }
}
