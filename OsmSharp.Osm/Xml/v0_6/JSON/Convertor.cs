using OsmSharp.Osm.Factory;
using OsmSharp.Osm.Sources;
using OsmSharp.Tools.Math.Geo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmSharp.Osm.Xml.v0_6.JSON
{
    public static class Convertor
    {
        public static Node ConvertNodeFrom(OsmObject osmObj)
        {
            // create a new node an immidiately set the id.
            Node new_obj = OsmBaseFactory.CreateNode((int)osmObj.Id);

            new_obj.Coordinate = new GeoCoordinate((double)osmObj.Lat, (double)osmObj.Lon);

            // set the tags.
            if (osmObj.Tags != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in osmObj.Tags)
                {
                    new_obj.Tags.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return new_obj;
        }

        public static Way ConvertWayFrom(OsmObject osmObj, INodeSource node_source)
        {
            // create a new node an immidiately set the id.
            Way new_obj = OsmBaseFactory.CreateWay((int)osmObj.Id);


            // set the tags.
            if (osmObj.Nodes != null)
            {
                foreach (long nodeId in osmObj.Nodes)
                {
                    Node child_node = node_source.GetNode(nodeId);
                    if (child_node != null)
                    {
                        new_obj.Nodes.Add(child_node);
                    }
                    else
                    { // way cannot be converted; node was not found!
                        return null;
                    }
                }
            }

            // set the tags.
            if (osmObj.Tags != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in osmObj.Tags)
                {
                    new_obj.Tags.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return new_obj;
        }
    }
}
