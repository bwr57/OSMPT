using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demo.WindowsForms;
using OsmSharp.Osm.Simple;

namespace Rodsoft.OSM.OSMObjects
{

  public enum PlaceTypes
  {
    None
    , City
    , Town
    , Village
    , Hamlet
    , Allotments
    , Suburb
    , Neighbourhood
    , Isolated_dwelling
  }

  public class Place : SimpleWay
  {



    public Place()
      : base()
    {

    }

    public PlaceTypes GetPlaceType()
    {
      string placeTagValue = GetPropertyValue("place");
      if(String.IsNullOrEmpty(placeTagValue))
        return PlaceTypes.None;
        switch (placeTagValue.ToLower())
        {
          case "city": { return PlaceTypes.City; }
          case "town": { return PlaceTypes.Town; }
          case "village": { return PlaceTypes.Village; }
          case "hamlet": { return PlaceTypes.Hamlet; }
          case "allotments": { return PlaceTypes.Allotments; }
          case "suburb": { return PlaceTypes.Suburb; }
          case "neighbourhood": { return PlaceTypes.Neighbourhood; }
          case "isolated_dwelling": { return PlaceTypes.Isolated_dwelling; }
        }
      return PlaceTypes.None;
    }

    public string GetPlaceTypeName()
    {
      PlaceTypes placeType = GetPlaceType();
        switch (placeType)
        {
          case PlaceTypes.None: { return "Неизвестно"; }
          case PlaceTypes.City: { return "Город"; }
          case PlaceTypes.Town: { return "Город"; }
          case PlaceTypes.Village: { return "Село"; }
          case PlaceTypes.Hamlet: { return "Деревня"; }
          case PlaceTypes.Allotments: { return "Садоводческое товарищество";}
          case PlaceTypes.Suburb: { return "Район"; }
          case PlaceTypes.Neighbourhood: { return "Микрорайон"; }
          case PlaceTypes.Isolated_dwelling: { return "Хутор"; }
        }
      return "";
    }

    public PlaceTypes PlaceType
    {
      get { return GetPlaceType(); }
    }

    public override string ToString()
    {
      return string.Format("{0} \"{1}\"", GetPlaceTypeName(), this.Name);
    }
  }

  public class PlaceOSMDriver : WayOSMDriver
  {
    public override object CreateObject()
    {
      return new Place();
    }

  }

}