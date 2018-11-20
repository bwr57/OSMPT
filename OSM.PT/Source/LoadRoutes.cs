using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml;

using Demo.WindowsPresentation.Cash;
using GMap.NET;

namespace Demo.WindowsPresentation.Source
{
    public class LoadRoutesService
    {
        private static bool _IsRequested = false;
        public static HttpWebResponse PostMethod(string postedData, string postUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);
            request.Method = "POST";
            request.Credentials = CredentialCache.DefaultCredentials;

            UTF8Encoding encoding = new UTF8Encoding();
            var bytes = encoding.GetBytes(postedData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bytes.Length;

            using (var newStream = request.GetRequestStream())
            {
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();
            }
            return (HttpWebResponse)request.GetResponse();
        }

        public static bool LoadRoutes(CashHelper cash, RectLatLng viewRect)
        {
            bool result = LoadRoutes(cash, viewRect.Lat, viewRect.Lng);
            result |= LoadRoutes(cash, viewRect.Lat, viewRect.Lng + viewRect.WidthLng);
            result |= LoadRoutes(cash, viewRect.Lat - viewRect.HeightLat, viewRect.Lng );
            result |= LoadRoutes(cash, viewRect.Lat - viewRect.HeightLat, viewRect.Lng + viewRect.WidthLng);
            return result;
        }

        public static bool LoadRoutes(CashHelper cash, double latitude, double longtitude)
        {
 //       	MessageBox.Show(_IsRequested.ToString());
            if (_IsRequested)
                return false;
            _IsRequested = true;
            if (cash.TestSquare(latitude, longtitude))
            {
                _IsRequested = false;
        //        MessageBox.Show(_IsRequested.ToString());
                return false;
            }
            double w = Math.Truncate(longtitude);
            double n = Math.Ceiling(latitude);
            double e = Math.Ceiling(longtitude);
            double s = Math.Truncate(latitude);
            if (w == e)
                e = w + 1;
            if (n == s)
                n = s + 1;
            string url = "http://www.overpass-api.de/api/interpreter/";
            string requestCode = String.Format(@"<!--
This is an example Overpass query.
Try it out by pressing the Run button above!
You can find more examples with the Load tool.
-->
<union>
<query type=""node"">
  <has-kv k=""highway"" v=""bus_stop""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>

</query>
<query type=""node"">
  <has-kv k=""railway"" v=""tram_stop""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>

</query>
<query type=""node"">
  <has-kv k=""railway"" v=""station""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>

</query>

<query type=""node"">
  <has-kv k=""railway"" v=""halt""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>

</query>

<query type=""node"">
  <has-kv k=""public_transport"" v=""platform""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>

</query>

<query type=""node"">
  <has-kv k=""public_transport"" v=""stop_position""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>

</query>

<query type=""way"">
  <has-kv k=""boundary"" v=""administrative""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>

</query>
<query type=""way"">
  <has-kv k=""place"" v=""city""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>
</query>
<query type=""way"">
  <has-kv k=""place"" v=""town""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>
</query>
<query type=""way"">
  <has-kv k=""place"" v=""village""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>
</query>
<query type=""way"">
  <has-kv k=""place"" v=""hamlet""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>
</query>
<query type=""relation"">
  <has-kv k=""type"" v=""route""/>
  <has-kv k=""route"" v=""trolleybus""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>

</query>
<query type=""relation"">
  <has-kv k=""type"" v=""route""/>
  <has-kv k=""route"" v=""bus""/>
  
  <bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>

</query>
<query type=""relation"">
  <has-kv k=""type"" v=""route""/>
  <has-kv k=""route"" v=""tram""/>
  
  <bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>
</query>
<query type=""relation"">
  <has-kv k=""type"" v=""route""/>
  <has-kv k=""route"" v=""train""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>
</query>

<query type=""relation"">
  <has-kv k=""type"" v=""route""/>
  <has-kv k=""route"" v=""light_rail""/>
  
<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>
</query>

<query type=""relation"">
  <has-kv k=""type"" v=""route""/>
  <has-kv k=""route"" v=""subway""/>
	<bbox-query w=""{0}"" n=""{1}"" e=""{2}""  s=""{3}""/>
</query>

</union>
<!-- added by auto repair -->

<!-- added by auto repair -->
<union>
  <item/>
  <recurse type=""down""/>
</union>
<!-- end of auto repair -->
<print/>", w, n, e, s);
//            MessageBox.Show(requestCode);
            HttpWebResponse response = PostMethod(requestCode, url);
//            MessageBox.Show(response == null ? "Хрен" : "Что-то есть");
            if (response != null)
            {
                var strreader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                XmlDocument doc = new XmlDocument();
                doc.Load(strreader);
                doc.Save("ot.osm");
                XmlNodeList nodes = doc.SelectNodes("osm/node");
                int i =0;
                foreach (XmlNode node in nodes)
                {
                    cash.SaveNode(node);
                    i++;
                }
                nodes = doc.SelectNodes("osm/way");
                foreach (XmlNode node in nodes)
                    cash.SaveWay(node);
                 
                nodes = doc.SelectNodes("osm/relation");
                foreach (XmlNode node in nodes)
                    cash.SaveRoute(node);
                cash.RegisterSquare(w, n, e, s);
                cash.TransferToDisk();
            }
            _IsRequested = false;
            return true;
    
        }
    }
}
