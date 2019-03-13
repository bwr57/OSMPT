using RodSoft.OSM.OSMObjects;
using RodSoft.Tools;
using System.Collections.Generic;

namespace RodSoft.OSM.PT
{
    public class OSMPTStopPoint : OSMPoint
    {
        private int _StopType;

        public string StopTypeName { get; set; }

        private IList<OSMPTRouteLite> _Routes = new List<OSMPTRouteLite>();

        public IList<OSMPTRouteLite> Routes
        {
            get { return _Routes; }
        }

        public int StopType
        {
            get { return _StopType; }
            set
            {
                _StopType = value;
                StopTypeName = GetStopTypeName(value);
            }
        }

        public OSMPTStopPoint()
            : base()
        {
        }

        public OSMPTStopPoint(double latitude, double longtitude, string name)
            : base(latitude, longtitude)
        {
            Tags.Add("highway", "bus_stop");
            Tags.Add("name", name);
        }

        public string Name
        {
            get { return GetPropertyValue("name"); }
            set { SetPropertyValue("name", value); }
        }

        public static string GetStopTypeName(int type)
        {

            string typeName = "";
            if (CompareBit.TestBit(type, (int)StopTypes.StopPoint))
            {
                typeName += "Место остановки\n";
            }
            if (CompareBit.TestBit(type, (int)StopTypes.Platform))
            {
                typeName += "Место ожидания\n";
            }
            if (CompareBit.TestBit(type  , (int)StopTypes.BusStop))
            {
                typeName += "Остановка автобуса\n";
            }
            if (CompareBit.TestBit(type, (int)StopTypes.BusStation))
            {
                typeName += "Автостанция\n";
            }
            if (CompareBit.TestBit(type, (int)StopTypes.TramStop))
            {
                typeName += "Остановка трамвая\n";
            }
            if (CompareBit.TestBit(type, (int)StopTypes.SubwayStation))
            {
                typeName += "Станция метро\n";
            }
            else
            {
                if (CompareBit.TestBit(type, (int)StopTypes.RailroadStation))
                {
                    typeName += "Железнодорожная станция\n";
                }
                if (CompareBit.TestBit(type, (int)StopTypes.RailroadHalt))
                {
                    typeName += "Остановочная платформа\n";
                }
            }
            return typeName;
        }

    }
}
