using RodSoft.OSM.Tracking;
using System;

namespace RodSoft.OSM.PT.Online
{
    [Serializable]
    public class VehicleData : VehicleGeoData
    {
        public OSMOTRouteTypes RouteType;
        public int Id;

        //public double Latitude;
        //public double Longitude;
        public string Line;
        public string Operator;
        public string Type;
        public string Number;
        public string LastStop;
        public string TrackType;
        public string AreaName;
        public string StreetName;
        public byte Red;
        public byte Green;
        public byte Blue;


        //public VehicleData(double[] values)
        //    : base(values)
        //{  }

        /// <summary>
        /// Creates a geo coordinate.
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        public VehicleData(double latitude, double longitude)
            : base(latitude, longitude)
        {

        }


    }
}
