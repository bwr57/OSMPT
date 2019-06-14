using RodSoft.Core.Communications;
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

    public interface IGeoPositionRegistrator : IActivatedController, IDisposable
    {
        IVehicleGeoDataAgentFactory VehicleGeoDataAgentFactory { get; set; }
        VehicleGeoData GetCurrentPosition();
        GeoPositionRegistratorStatus GetRegistratorStatus();
        int Period { get; set; }
    }

}
