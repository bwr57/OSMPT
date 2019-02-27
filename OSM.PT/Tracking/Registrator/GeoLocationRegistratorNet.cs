using RodSoft.OSM.Tracking;
using System;
using System.Device.Location;

namespace Demo.WindowsPresentation.Tracking.Registrator
{
    public class GeoLocationRegistratorNet : IGeoPositionRegistrator, IDisposable
    {

        protected GeoCoordinateWatcher _RegistratorService;

        public uint Interval
        {
            get;
            set;
        }

        public GeoPositionRegistratorStatus GetRegistratorStatus()
        {
            GeoPositionStatus status = _RegistratorService.Status;
            if (status == GeoPositionStatus.Ready)
                return GeoPositionRegistratorStatus.Active;
            if (status == GeoPositionStatus.Disabled)
                return GeoPositionRegistratorStatus.Disabled;
            if (status == GeoPositionStatus.Initializing)
                return GeoPositionRegistratorStatus.Initializing;
            return GeoPositionRegistratorStatus.Error;
        }

        public GeoLocationRegistratorNet()
        {
            _RegistratorService = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            _RegistratorService.Start();
        }

        public TrackPoint GetCurrentPosition()
        {
            GeoPosition<GeoCoordinate> position = _RegistratorService.Position;
            TrackPoint trackPoint = new TrackPoint(position.Location.Latitude, position.Location.Longitude)
            { Course = position.Location.Course, Speed = position.Location.Speed, Altitude = position.Location.Altitude, Time = position.Timestamp.DateTime };
            trackPoint.Time = position.Timestamp.DateTime;
            return trackPoint;
        }

        public void Dispose()
        {
            if (_RegistratorService != null)
            {
                _RegistratorService.Dispose();
                _RegistratorService = null;
            }
        }
    }
}
