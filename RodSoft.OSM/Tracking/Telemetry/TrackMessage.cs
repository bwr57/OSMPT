using RodSoft.Core.Communications;
using System;

namespace RodSoft.OSM.Tracking.Telemetry
{
    [Serializable]
    public class TrackMessage : CashedMessage
    {
        public DateTime Time;
        public string Vehicle;
        public GPSDataMessage TrackPoint;
        public bool WasTransmitted;
        public int Index;
        public string FileName;
   }
}
