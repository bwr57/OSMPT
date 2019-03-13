using System;
using System.Device.Location;

namespace RodSoft.OSM.Tracking.Registrator
{
    public class GeoLocationRegistratorNet : IGeoPositionRegistrator, IDisposable
    {

        protected GeoCoordinateWatcher _RegistratorService;

        public IVehicleGeoDataAgentFactory VehicleGeoDataAgentFactory;

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

        public VehicleGeoData GetCurrentPosition()
        {
            GeoPosition<GeoCoordinate> position = _RegistratorService.Position;
            VehicleGeoData vehicleGeoData = VehicleGeoDataAgentFactory == null ? new VehicleGeoData(position.Location.Latitude, position.Location.Longitude) : VehicleGeoDataAgentFactory.CreateVehicleGeoDataAgent(position.Location.Latitude, position.Location.Longitude);
            vehicleGeoData.Course = position.Location.Course;
            vehicleGeoData.Speed = position.Location.Speed;
            vehicleGeoData.Altitude = position.Location.Altitude;
            vehicleGeoData.Time = position.Timestamp.DateTime;
            return vehicleGeoData;
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
