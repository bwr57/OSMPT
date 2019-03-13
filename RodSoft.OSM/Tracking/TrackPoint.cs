using OsmSharp.Tools.Math.Units.Distance;
using System;
using System.Collections.Generic;

namespace RodSoft.OSM.Tracking
{
    [Serializable]
    public class TrackPoint : VehicleGeoData
    {
        public int Id;

        public Meter DistanceFromStart
        {
            get; set;
        }

        public readonly IDictionary<string, double> ExtendedVehicleData = new Dictionary<string, double>();

        public virtual void SetExtendedVehicleData(IDictionary<string, double> extendedVehicleData)
        {
            if (extendedVehicleData.ContainsKey("speed"))
                this.Speed = extendedVehicleData["speed"] * 3.6;
        }


        /// <summary>
        /// Creates a geo coordinate.
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        public TrackPoint(double latitude, double longitude)
            : base(latitude, longitude)
        {

        }

    }
}
