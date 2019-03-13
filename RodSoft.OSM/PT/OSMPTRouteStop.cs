using RodSoft.OSM.OSMObjects;
using System.Collections.Generic;

namespace RodSoft.OSM.PT
{
    public class OSMPTRouteStop : OSMPoint
    {
        private bool _IsForward = true;

        public bool IsForward
        {
            get { return _IsForward; }
            set { _IsForward = value; }
        }

        public bool IsEntryOnly { get; set; }
        public bool IsExitOnly { get; set; }



        private Dictionary<long, OSMPTStopPoint> _StopPoints = new Dictionary<long, OSMPTStopPoint>();
        public Dictionary<long, OSMPTStopPoint> StopPoints
        {
            get { return _StopPoints; }
        }
    }
}

