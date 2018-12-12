using System;
using System.Collections.Generic;

namespace OsmSharp.Osm.Xml.v0_6.JSON
{
    [Serializable]
    public class Osm3s
    {
        public DateTime Timestamp_osm_base;
        public string Copyright;
    }

    [Serializable]
    public class OsmObject
    {
        public string Type;
        public long Id;
        public IDictionary<string, string> Tags;
        public double Lat;
        public double Lon;
        public IList<long> Nodes;
    }

    [Serializable]
    public class JsonMessage
    {
        public string Version;
        public string Generator;
        public Osm3s Osm3s;
        public IList<OsmObject> Elements;
    }
}
