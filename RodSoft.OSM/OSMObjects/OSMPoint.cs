using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodSoft.OSM.OSMObjects
{
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
}
