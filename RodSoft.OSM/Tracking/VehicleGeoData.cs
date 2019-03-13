﻿using OsmSharp.Tools.Math.Geo;
using System;

namespace RodSoft.OSM.Tracking
{
    [Serializable]
    public class VehicleGeoData : GeoCoordinate, IVehicleGeoData
    {
        public DateTime Time{ get; set; }
        public double Speed{ get; set; }
        public double Course{ get; set; }
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