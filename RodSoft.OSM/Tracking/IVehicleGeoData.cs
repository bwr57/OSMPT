using System;

namespace RodSoft.OSM.Tracking
{
    public interface IVehicleGeoData
    {
        DateTime Time { get; set; }
        double Latitude { get; set; }
        double Longitude { get; set; }
        double Speed { get; set; }
        double Course{ get; set; }
        double Altitude{ get; set; }
    }
}
