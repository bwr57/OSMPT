using System;

namespace RodSoft.OSM.Tracking
{
    public interface IVehicleGeoData
    {
        DateTime Time { get; set; }
        double Latitude { get; set; }
        double Longitude { get; set; }
        short Speed { get; set; }
        short Course{ get; set; }
        Single Altitude{ get; set; }
    }
}
