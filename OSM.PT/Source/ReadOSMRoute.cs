using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using GMap.NET;

namespace Demo.WindowsForms
{
    public abstract class OSMDriverBase
    {
        public static XmlDocument LoadOSMData(string type, string id)
        {
            string url = String.Format("http://www.openstreetmap.org/api/0.6/{0}/{1}", type, id);
            string xml = string.Empty;
            XmlDocument doc = new XmlDocument();
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.Accept = "*/*";
                request.KeepAlive = true;
                request.Timeout = 30000;
                //                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                //              request.CachePolicy = noCachePolicy;


                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader read = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            doc.Load(read);
                            xml = read.ReadToEnd();
                        }
                    }
#if PocketPC
               request.Abort();
#endif
                    response.Close();
                }
            }
            return doc;
        }

        public abstract object CreateObject();

    }

    public class OSMPoint 
    {
        public Int64 ID;
        public bool IsWay;
        public string Type;
        public string Name;

        public PointLatLng Point;

        public double Latitude
        {
            get { return this.Point.Lat; }
            set { this.Point.Lat = value; }
        }

        public double Longtitude
        {
            get { return this.Point.Lng; }
            set { this.Point.Lng = value; }
        }

        public IDictionary<string, string> Tags = new Dictionary<string, string>();

        public OSMPoint()
        {
        }

        
        public OSMPoint(double latitude, double longtitude)
        {
            Latitude = latitude;
            Longtitude = longtitude;
        }

        public string GetPropertyValue(string name)
        {
            if (Tags.ContainsKey(name))
                return Tags[name];
            return null;
        }
        public void SetPropertyValue(string name, string value)
        {
            if (Tags.ContainsKey(name))
                Tags[name] = value;
            else
                Tags.Add(name, value);
        }

        
    }

    public class PointOSMDriver : OSMDriverBase
    {
        public PointOSMDriver()
        {        }

        public PointOSMDriver(XmlNode node)
        {
            Parse(node);
        }

        public virtual OSMPoint Parse(XmlNode node)
        {
            return Parsing(null, node);
        }

        public OSMPoint Parsing(OSMPoint osmPoint, XmlNode xml)
        {
            if (osmPoint == null)
            {
                osmPoint = (OSMPoint)CreateObject();
            }
            osmPoint.Latitude  = Convert.ToDouble(xml.Attributes["lat"].Value, new CultureInfo("en-US"));
            osmPoint.Longtitude  = Double.Parse(xml.Attributes["lon"].Value, new CultureInfo("en-US"));
            XmlNodeList tagNodes = xml.SelectNodes("tag");
            if (tagNodes != null)
            {
                foreach (XmlNode tagNode in tagNodes)
                {
                    osmPoint.Tags.Add(tagNode.Attributes["k"].Value, tagNode.Attributes["v"].Value);
                }
            }
            return osmPoint;
        }

        public OSMPoint LoadPoint(string id)
        {
            XmlDocument doc = LoadOSMData("node", id);
            if (doc != null)
            {
                XmlNode node = doc.SelectSingleNode("osm/node");
                if(node != null)
                    return Parse(node);
            }
            return null;
        }

        public override object CreateObject()
        {
            return new OSMPoint();
        }

    }

    public enum StopTypes
    {
        NoneStop = 0,
        StopPoint = 1,
        Platform = 2,
        BusStop = 4,
        BusStation = 8,
        TramStop = 16,
        RailroadStation = 32,
        RailroadHalt = 64,
		    SubwayStation = 128
    }

    public class Tools
    {
        public static bool TestBit(int data, int bit)
        {
            return (data & bit) == bit;
        }


    }

    public class OSMStopPoint : OSMPoint
    {
        private int _StopType;

        public string StopTypeName { get; set; }

        private IList<OSMOTRouteLite> _Routes = new List<OSMOTRouteLite>();

        public IList<OSMOTRouteLite> Routes
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

        public OSMStopPoint()
            : base()
        {
        }

        public OSMStopPoint(double latitude, double longtitude, string name)
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
            if (Tools.TestBit(type, (int)StopTypes.StopPoint))
            {
                typeName += "Место остановки\n";
            }
            if (Tools.TestBit(type, (int)StopTypes.Platform))
            {
                typeName += "Место ожидания\n";
            }
            if (Tools.TestBit(type, (int)StopTypes.BusStop))
            {
                typeName += "Остановка автобуса\n";
            }
            if (Tools.TestBit(type, (int)StopTypes.BusStation))
            {
                typeName += "Автостанция\n";
            }
            if (Tools.TestBit(type, (int)StopTypes.TramStop))
            {
                typeName += "Остановка трамвая\n";
            }
            if (Tools.TestBit(type, (int)StopTypes.SubwayStation))
            {
                typeName += "Станция метро\n";
            }
            else
            {
            if (Tools.TestBit(type, (int)StopTypes.RailroadStation))
            {
                typeName += "Железнодорожная станция\n";
            }
            if (Tools.TestBit(type, (int)StopTypes.RailroadHalt))
            {
                typeName += "Остановочная платформа\n";
            }
            }
            return typeName;
        }

    }

    public class StopPoinOSMtDriver : PointOSMDriver
    {
        public override object CreateObject()
        {
            return new OSMStopPoint();
        }
    }

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

    public class WayOSMDriver : OSMDriverBase
    {
        public OSMWay Parse(OSMWay way, XmlNode node)
        {
            if (way == null)
            {
                way = (OSMWay)CreateObject();
            }
            XmlNodeList nodes = node.SelectNodes("nd");
            foreach (XmlNode wayPointNode in nodes)
            {
                PointOSMDriver pointDriver = new PointOSMDriver();
                OSMPoint point = pointDriver.LoadPoint(wayPointNode.Attributes["ref"].Value);
                if(point != null)
                    way.Points.Add(point);
            }
            return way;
        }
        public OSMWay LoadPoint(string id)
        {
            OSMWay way = new OSMWay();
            XmlDocument doc = LoadOSMData("way", id);
            if (doc != null)
            {
                PointOSMDriver stopPointDriver = new PointOSMDriver();
                XmlNodeList nodes = doc.SelectNodes("osm/way/nd");
                
                if (nodes != null)
                    foreach (XmlNode node in nodes)
                    {
                        OSMPoint point = (OSMPoint)stopPointDriver.LoadPoint(node.Attributes["ref"].Value);
                        if (point != null)
                        {
                            way.Points.Add(point);
                        }
                    }
            }
            return way;
        }

        public override object CreateObject()
        {
            return new OSMWay();
        }

    }

    public enum OSMOTRouteTypes
    {
        Way = 0
        , Tramway = 1
        , Trolleybus = 2
        , Bus = 4
        , SharedTaxi = 8
        , Train = 16
        , Light_rail = 32
        , Subway = 64
        , Ferry = 128
        , Local = 256
        , Regional = 512
        , LongDistance = 1024
        , HighSpeed = 2048
    }
    public static class OSMOTHelper
    {
//        public static IDictionary<uint, string>
        public static string GetRouteTypeName(OSMOTRouteTypes osmOTRouteType)
        {
            if (osmOTRouteType == OSMOTRouteTypes.Tramway)
                return "Трамвай";
            return "";

        }
    }
    public class OSMOTRouteStop : OSMPoint
    {
    	private bool _IsForward = true;
    	
    	public bool IsForward
    	{
    		get { return _IsForward; }
    		set { _IsForward = value; }
    	}
    	
    	public bool IsEntryOnly { get; set; }
    	public bool IsExitOnly { get; set; }
    	
    	
    	
        private Dictionary<long, OSMStopPoint> _StopPoints =  new Dictionary<long, OSMStopPoint>();
        public Dictionary<long, OSMStopPoint> StopPoints
        {
            get { return _StopPoints; }
        }
    }

    public class OSMOTRouteLite
    {
        public Int64 id;
        public string Name;
        public string Ref;
        public OSMOTRouteTypes RouteType;
        public string Operator;
        public string Network;
        public string From;
        public string To;
    }

    public class OSMOTRoute : OSMOTRouteLite
    {

        private List<OSMOTRouteStop> _Stops =  new List<OSMOTRouteStop>();
        public List<OSMOTRouteStop> Stops
        {
            get { return _Stops; }
        }
        private List<OSMWay> _Ways = new List<OSMWay>();

        public List<OSMWay> Ways
        {
            get { return _Ways; }
        }

    }


    public class ReadOSMRoute : OSMDriverBase
    {

        public override object CreateObject()
        {
            return new OSMOTRoute();
        }


        public static OSMOTRoute ReadRoute()
        {
            XmlDocument doc = LoadOSMData("relation", "376588");
            XmlNodeList members = doc.SelectNodes("osm/relation/member");
            OSMOTRoute route = new OSMOTRoute();
            List<XmlNode> stopNodes = new List<XmlNode>();
            List<XmlNode> wayNodes = new List<XmlNode>();
            StopPoinOSMtDriver stopPointDriver = new StopPoinOSMtDriver();
            WayOSMDriver wayPointDriver = new WayOSMDriver();
            foreach (XmlNode member in members)
            {
                try
                {
                    if (member.Attributes["type"].Value == "node")
                    {
                        OSMStopPoint stopPoint = (OSMStopPoint)stopPointDriver.LoadPoint(member.Attributes["ref"].Value);
                        if(stopPoint != null)
                        {
                        	OSMOTRouteStop routeStop = new OSMOTRouteStop();
                        	routeStop.Name = stopPoint.Name;
                        	routeStop.StopPoints.Add(stopPoint.ID, stopPoint);
                            route.Stops.Add(routeStop);
                        }
                    }
                    else
                    {
                        if (member.Attributes["type"].Value == "way")
                        {
                            OSMWay way = (OSMWay)wayPointDriver.LoadPoint(member.Attributes["ref"].Value);
                            if(way != null)
                                route.Ways.Add(way);
                        }
                    }
                }
                catch
                {
                }
            }
            return route;
        }

    }
}
