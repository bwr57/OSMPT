using RodSoft.Core.Communications;
using System;

namespace RodSoft.OSM.Tracking.Registrator
{
    public class GeoLocationLogger : IActivatedController, IDisposable
    {
        public string Name { get; set; } = "GPS";

        public readonly Track Track = new Track();

        private bool _IsActive;

        public bool IsActive
        {
            get { return _IsActive; }
            protected set { _IsActive = value; } 
        }

        private bool _IsTracking;

        public bool IsTracking
        {
            get { return _IsTracking; }
            set
            {
                if (value && !_IsTracking)
                    Track.AddSegment();
                _IsTracking = value;
            }
        }

        private int _TrackPointID = 0;

        public TrackPoint LastLocation { get; protected set; }

        private IGeoPositionRegistrator _GeoPositionRegistrator = null;
        public IGeoPositionRegistrator GeoPositionRegistrator
        {
            get { return _GeoPositionRegistrator; }
            set
            {
                if (_GeoPositionRegistrator != null)
                {
                    lock (_GeoPositionRegistrator) { }
                }
                _GeoPositionRegistrator = value;
            }
        }

        public GeoLocationLogger(bool isTracking)
        {
            this.GeoPositionRegistrator = new GeoLocationRegistratorNet();
            this.GeoPositionRegistrator.VehicleGeoDataAgentFactory = new TrackPointFactory();
            this.IsTracking = isTracking;
        }

        public GeoLocationLogger(IGeoPositionRegistrator geoLocationRegistrator, bool isTracking)
        {
            this.GeoPositionRegistrator = geoLocationRegistrator;
            this.GeoPositionRegistrator.VehicleGeoDataAgentFactory = new TrackPointFactory();
            this.IsTracking = isTracking;
        }

        public virtual void RegisterGeoLocation()
        {
            if (_GeoPositionRegistrator != null)
            {
                lock (_GeoPositionRegistrator)
                {
                    LastLocation = _GeoPositionRegistrator.GetCurrentPosition() as TrackPoint;

                    LastLocation.Id = _TrackPointID;
                    if (!this.GeoPositionRegistrator.IsActive || double.IsNaN(LastLocation.Latitude))
                    {
                        IsActive = false;
                    }
                    else
                    {
                        if (IsTracking)
                        {
                            Track.AddTrackPoint(LastLocation);
                            _TrackPointID++;
                        }
                        IsActive = true;
                    }
                }
            }

        }

        public void Dispose()
        {
            if (GeoPositionRegistrator != null)
                GeoPositionRegistrator.Dispose();
            GeoPositionRegistrator = null;
        }
    }
}
