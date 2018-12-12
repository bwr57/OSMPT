using Newtonsoft.Json;
using OsmSharp.Osm.Data;
using OsmSharp.Osm.Data.Raw.XML.OsmSource;
using OsmSharp.Osm.Xml.v0_6.JSON;
using OsmSharp.Tools.Math.Geo;
using RodSoft.OSMPT.PT.Online;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RodSoft.OSM.Source
{
    public class LoadRoadsJosm
    {
            public static HttpWebResponse GetMethod(string postedData, string postUrl)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl  + postedData);
                request.Method = "GET";
                request.Credentials = CredentialCache.DefaultCredentials;

                return (HttpWebResponse)request.GetResponse();
            }
            public static IDataSource LoadRoutes(GeoCoordinateBox area)
        {
  /*          //       	MessageBox.Show(_IsRequested.ToString());
            //if (_IsRequested)
            //    return false;
            //_IsRequested = true;
            //if (cash.TestSquare(latitude, longtitude))
            //{
            //    _IsRequested = false;
            //    //        MessageBox.Show(_IsRequested.ToString());
            //    return false;
            //}
            //double w = Math.Truncate(longtitude);
            //double n = Math.Ceiling(latitude);
            //double e = Math.Ceiling(longtitude);
            //double s = Math.Truncate(latitude);
            //if (w == e)
            //    e = w + 1;
            //if (n == s)
            //    n = s + 1;
            //            string url = "http://www.overpass-api.de/api/interpreter/";
            string url = "https://www.overpass-api.de/api/interpreter?data=";
            string requestCode = String.Format("[out: json] [timeout:25];way[\"highway\"] ({0},{1},{2},{3});(._;>;);out; ", area.MinLat.ToString(CultureInfo.GetCultureInfo("en-US")), area.MinLon.ToString(CultureInfo.GetCultureInfo("en-US")), area.MaxLat.ToString(CultureInfo.GetCultureInfo("en-US")), area.MaxLon.ToString(CultureInfo.GetCultureInfo("en-US")));
            //            MessageBox.Show(requestCode);
            HttpWebResponse response = GetMethod(requestCode, url);
            string json = null;
            //            MessageBox.Show(response == null ? "Хрен" : "Что-то есть");
            if (response != null)
            {
                StreamReader strreader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                json = strreader.ReadToEnd();
            }
            if (json == null)
                return null;
            File.AppendAllText("roads.txt", json);*/
            string json = File.ReadAllText("roads.txt");
            if (json != null)
            {
                JsonMessage jsonMessage = JsonConvert.DeserializeObject<JsonMessage>(json);
                OsmDataSource osmDataSource = new OsmDataSource(area, jsonMessage);
                return osmDataSource;
            }
            return null;
        }
    }
}
