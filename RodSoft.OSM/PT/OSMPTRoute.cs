using RodSoft.OSM.OSMObjects;
using System.Collections.Generic;

namespace RodSoft.OSM.PT
{
    public class OSMPTRoute : OSMPTRouteLite
    {
        private List<OSMPTRouteStop> _Stops = new List<OSMPTRouteStop>();
        public List<OSMPTRouteStop> Stops
        {
            get { return _Stops; }
        }
        private List<OSMWay> _Ways = new List<OSMWay>();

        public List<OSMWay> Ways
        {
            get { return _Ways; }
        }

    }
}
