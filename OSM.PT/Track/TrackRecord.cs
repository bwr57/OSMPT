using RodSoft.OSMPT.PT.Online;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo.WindowsPresentation.Track
{
    public class TrackRecord : VehicleData
    {
        public readonly IDictionary<string, double> ExtendedVehicleData = new Dictionary<string, double>();

        public virtual void SetExtendedVehicleData(IDictionary<string, double> extendedVehicleData)
        {
            if (extendedVehicleData.ContainsKey("speed"))
                this.Speed = extendedVehicleData["speed"];
        }
    }
}
