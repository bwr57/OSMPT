using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace RodSoft.OSM.OSMObjects
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
}
