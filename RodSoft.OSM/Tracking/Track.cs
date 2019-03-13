using OsmSharp.Tools.Math.Units.Distance;
using System.Collections.Generic;

namespace RodSoft.OSM.Tracking
{
    public class Track
    {
        public readonly IList<IList<TrackPoint>> Segments = new List<IList<TrackPoint>>();

        private TrackPoint _FirstPosition;
        private IList<TrackPoint> _CurrentSegment;

        private TrackPoint _AddedPosition;
        private IList<TrackPoint> _AddedSegment;

        public Meter TotalDistance
        {
            get; protected set;
        }

        public TrackPoint FirstPosition
        {
            get { return _FirstPosition; }
            set
            {
                foreach (IList<TrackPoint> segment in this.Segments)
                {
                    if (segment.Contains(value))
                    {
                        _FirstPosition = value;
                        _CurrentSegment = segment;
                        break;
                    }
                }
            }
        }

        public virtual IList<TrackPoint> AddSegment()
        {
            _AddedSegment = new List<TrackPoint>();
            Segments.Add(_AddedSegment);
            return _AddedSegment;
        }

        public virtual void AddTrackPoint(TrackPoint trackPoint)
        {
            if (trackPoint == null)
                return;
            if (_AddedSegment == null)
                AddSegment();
            if(_FirstPosition == null)
            {
                _CurrentSegment = _AddedSegment;
                _FirstPosition = trackPoint;
                TotalDistance = 0;
            }
            else
            {
                TotalDistance += trackPoint.DistanceReal(_AddedPosition);
            }
            trackPoint.DistanceFromStart = TotalDistance;
            _AddedPosition = trackPoint;
            _AddedSegment.Add(trackPoint);
        }

    }
}
