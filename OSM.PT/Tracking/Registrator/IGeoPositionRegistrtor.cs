using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RodSoft.OSM.Tracking;

namespace Demo.WindowsPresentation.Tracking.Registrator
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
        TrackPoint GetCurrentPosition();
        GeoPositionRegistratorStatus GetRegistratorStatus();
        uint Interval { get; set; }

    }
}
