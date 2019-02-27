using Demo.WindowsPresentation.Tracking.Registrator;
using RodSoft.OSM.Tracking;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Demo.WindowsPresentation.Tracking.Telemetry
{
    [Serializable]
    public class TrackMessage 
    {
        public DateTime Time;
        public string Vehicle;
        public GPSDataMessage TrackPoint;
        public bool WasTransmitted;
        public int Index;
        public string FileName;
   }
}
