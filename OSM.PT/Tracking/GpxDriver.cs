using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace RodSoft.OSM.Tracking
{
    public class GpxDriver
    {
        public string _XML;
        public GpxDriver(string fileName)
        {
            if (File.Exists(fileName))
            {
                _XML = File.ReadAllText(fileName);
            }
        }

        public virtual Track Parse()
        {
            if (_XML == null)
                return null;
            Track track = new Track();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(_XML);
            XmlNode root = xml.DocumentElement;

            // Add the namespace.  
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("xsi", "http://www.topografix.com/GPX/1/1/gpx.xsd");
            //    / trk / trkseg
            if (root.LastChild != null)
            {


                XmlNodeList segmentsXmlNodes = root.LastChild.ChildNodes;
                foreach (XmlNode segmentsXmlNode in segmentsXmlNodes)
                {

                    IList<TrackPoint> trackSegment = track.AddSegment();
                    XmlNodeList pointsXmlNodes = segmentsXmlNode.ChildNodes;
                    foreach (XmlNode pointXmlNode in pointsXmlNodes)
                    {
                        double latitude, longitude;
                        if (!Double.TryParse(pointXmlNode.Attributes["lat"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"), out latitude))
                            continue;
                        if (!Double.TryParse(pointXmlNode.Attributes["lon"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"), out longitude))
                            continue;
                        TrackPoint tr = new TrackPoint(latitude, longitude);
                        if (pointXmlNode.ChildNodes != null)
                        {
                            XmlNodeList trDataNodes = pointXmlNode.ChildNodes;
                            foreach (XmlNode trDataNode in trDataNodes)
                            {
                                switch (trDataNode.Name)
                                {
                                    case "ele":
                                        {
                                            Double elevation = 0;
                                            if (Double.TryParse(trDataNode.LastChild.Value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"), out elevation))
                                            {
                                                tr.Elevation = elevation;
                                                break;
                                            }
                                            tr.Elevation = null;
                                            break;
                                        }
                                    case "time":
                                        {
                                            DateTime time;
                                            if (DateTime.TryParse(trDataNode.LastChild.Value, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AssumeUniversal, out time))
                                            {
                                                tr.Time = time;
                                                break;
                                            }
                                            tr.Time = DateTime.MinValue;
                                            break;
                                        }
                                    case "extensions":
                                        {
                                            IDictionary<string, double> extendedVehicleData = ParseExtendedVehicleData(trDataNode);
                                            if (extendedVehicleData != null)
                                            {
                                                tr.SetExtendedVehicleData(extendedVehicleData);
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        track.AddTrackPoint(tr);
                    }
                }
            }
            return track;
        }

        protected virtual IDictionary<string, double> ParseExtendedVehicleData(XmlNode extendedVehicleDataXmlNode)
        {
            if (extendedVehicleDataXmlNode == null)
            {
                return null;
            }
            XmlNodeList extendedVehicleDataNodes = extendedVehicleDataXmlNode.ChildNodes;
            if (extendedVehicleDataNodes == null)
                return null;
            IDictionary<string, double> extendedVehicleData = new Dictionary<string, double>();
            foreach (XmlNode extendedVehicleDataNode in extendedVehicleDataNodes)
            {
                Double result = 0;
                try
                {
                    if (Double.TryParse(extendedVehicleDataNode.LastChild.Value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"), out result))
                        extendedVehicleData.Add(extendedVehicleDataNode.Name.ToLower(), result);
                }
                catch (Exception ex)
                { }
            }

            return extendedVehicleData;
        }
    }
}
