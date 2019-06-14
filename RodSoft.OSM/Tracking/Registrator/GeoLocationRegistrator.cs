using LocationApiLib;
using System;

namespace RodSoft.OSM.Tracking.Registrator
{
    public class GeoLocationRegistrator : IGeoPositionRegistrator, IDisposable
    {

        protected LatLongReportFactory _RegistratorService;

        public IVehicleGeoDataAgentFactory VehicleGeoDataAgentFactory { get; set; }

        public int Period
        {
            get { return (int)_RegistratorService.ReportInterval; }
            set { _RegistratorService.ReportInterval = (uint)Period; }
        }

        public string Name { get; set; }

        public bool IsActive
        {
            get { return GetActiveStatus(); }
        }

        public GeoPositionRegistratorStatus GetRegistratorStatus()
        {
            uint status = _RegistratorService.status;
            if (status == 0)
                return GeoPositionRegistratorStatus.Disabled;
            if (status == 4)
                return GeoPositionRegistratorStatus.Active;
            return (GeoPositionRegistratorStatus)(status + 1);
        }

        public virtual bool GetActiveStatus()
        {
            return GetRegistratorStatus() == GeoPositionRegistratorStatus.Active;
        }

        public GeoLocationRegistrator()
        {
            _RegistratorService = new LatLongReportFactory();
            _RegistratorService.ListenForReports();
        }

        public VehicleGeoData GetCurrentPosition()
        {
            DispLatLongReport report = _RegistratorService.LatLongReport;

            VehicleGeoData vehicleGeoData = VehicleGeoDataAgentFactory == null ? new VehicleGeoData(report.Latitude, report.Longitude) : VehicleGeoDataAgentFactory.CreateVehicleGeoDataAgent(report.Latitude, report.Longitude);
            vehicleGeoData.Time = report.Timestamp;
            vehicleGeoData.Altitude = Convert.ToSingle(report.Altitude);
            return vehicleGeoData;
        }

        public void Dispose()
        {
            if(_RegistratorService != null)
            {
                _RegistratorService = null;
            }
        }
    }
}
