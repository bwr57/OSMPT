using System.Collections.Generic;

namespace RodSoft.OSM.OSMObjects
{
    public class OSMWay
    {
        private IList<OSMPoint> _Points = new List<OSMPoint>();

        public IList<OSMPoint> Points
        {
            get { return _Points; }
        }

        public OSMWay()
        {
        }
    }

}
