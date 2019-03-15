using System;

namespace RodSoft.OSM.Tracking.Registrator
{
    public enum GeoPositionRegistratorStatus
    {
        Disabled = 0,
        Active = 1,
        Error = 2,
        AccessDenied = 3,
        Initializing = 4
    }

    public interface IGeoPositionRegistrator : IDisposable
    {
        VehicleGeoData GetCurrentPosition();
        GeoPositionRegistratorStatus GetRegistratorStatus();
        uint Interval { get; set; }
        IVehicleGeoDataAgentFactory VehicleGeoDataAgentFactory { get; set; }
    }
}
