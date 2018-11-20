using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Osm.Simple;

namespace Rodsoft.OSM.OSMObjects
{
  public class OSMArea : SimpleWay
  {
    private List<SimpleNode> _NodeList = new List<SimpleNode>();

    public  List<SimpleNode> NodeList
    {
      get { return _NodeList; }
    }

    public void CalculateMinMax()
    {
      MinLatitude = _NodeList.Min(n => n.Latitude);
      MaxLatitude = _NodeList.Max(n => n.Latitude);
      MinLongitude = _NodeList.Min(n => n.Longitude);
      MaxLongitude = _NodeList.Max(n => n.Longitude);
    }

    public bool IsPointInside(double latitude, double longitude)
    {
      if(_NodeList.Count < 3)
        return false;
      foreach (SimpleNode node in _NodeList)
      {
        if (node.Latitude.HasValue && node.Longitude.HasValue
           && Math.Abs(node.Longitude.Value - longitude) <= 0.0000001
           && Math.Abs(node.Latitude.Value - latitude) <= 0.0000001)
        {
          return true;
        }
      }
      SimpleNode[] temp = new SimpleNode[_NodeList.Count + 1];
      _NodeList.CopyTo(temp);
      temp[temp.Length - 1] = _NodeList[0];
      int crosses = 0;
      for (int i = 0; i < temp.Length - 2; i++)
      {
        if(temp[i].Longitude.HasValue && temp[i+1].Longitude.HasValue && temp[i].Latitude.HasValue && temp[i+1].Latitude.HasValue)
        {
          double sign = (temp[i].Latitude.Value - latitude) * (temp[i + 1].Latitude.Value - latitude);
          if(Math.Abs(sign) <= 0.00000000000001)
          {
            if((temp[i].Longitude.Value - longitude) * (temp[i].Longitude.Value - longitude) <= 0)
              return true;
            if(temp[i].Longitude.Value < longitude && temp[i + 1].Longitude.Value < longitude)
            {
              crosses++;
            }
            continue;
          }
          if(sign> 0)
            continue;
          double crossLongitude = temp[i].Longitude.Value + (temp[i].Latitude.Value - latitude) * (temp[i + 1].Longitude.Value - temp[i].Longitude.Value) / (temp[i].Latitude.Value - temp[i+1].Latitude.Value);
          if (crossLongitude < longitude)
          {
            crosses++;
          }
        }
        return false;
      }
      return (crosses % 2) > 0;
    }

    public double? MinLatitude
    {
      get; set;
    }
    public double? MaxLatitude
    {
      get;
      set;
    }
    public double? MinLongitude
    {
      get;
      set;
    }
    public double? MaxLongitude
    {
      get;
      set;
    }
  }
}
