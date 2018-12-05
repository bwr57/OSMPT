using OsmSharp.Tools.Math.Units.Distance;
using RodSoft.OSMPT.PT.Online;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RodSoft.OSM.Tracking
{
    public class TrackPoint : VehicleData
    {
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

        public Meter Elevation
        {
            get; set;
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
