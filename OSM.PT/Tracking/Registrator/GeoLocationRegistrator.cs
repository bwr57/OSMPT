using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RodSoft.OSM.Tracking;
using LocationApiLib;

namespace Demo.WindowsPresentation.Tracking.Registrator
{
    public class GeoLocationRegistrator : IGeoPositionRegistrator, IDisposable
    {

        protected LatLongReportFactory _RegistratorService;

        public uint Interval
        {
            get { return _RegistratorService.ReportInterval; }
            set { _RegistratorService.ReportInterval = Interval; }
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

        public GeoLocationRegistrator()
        {
            _RegistratorService = new LatLongReportFactory();
            _RegistratorService.ListenForReports();
        }

        public TrackPoint GetCurrentPosition()
        {
            DispLatLongReport report = _RegistratorService.LatLongReport;
            TrackPoint trackPoint = new TrackPoint(report.Latitude, report.Longitude);
            trackPoint.Time = report.Timestamp;
            return trackPoint;
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
