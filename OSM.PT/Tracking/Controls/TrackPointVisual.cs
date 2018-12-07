using Demo.WindowsPresentation.CustomMarkers;
using GMap.NET.WindowsPresentation;
using RodSoft.OSM.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RodSoft.OSM.Tracking.Controls
{
    public class TrackPointVisual : CircleVisual
    {
        public event SelectStartPointDelegate OnSelectStartTrackPoint;
        public event SelectStartPointDelegate OnSelectLastTrackPoint;

        private TrackPoint _TrackPoint;

        public TrackPoint TrackPoint
        {
            get { return _TrackPoint; }
            set
            {
                _TrackPoint = value;
            }
        }

        public Track Track
        {
            get; protected set;
        }

        protected TrackTooltip TrackTooltip
        {
            get { return (TrackTooltip) Tooltip; }
        }

        public TrackPointVisual(GMapMarker m, Brush background, TrackPoint trackPoint, Track track) 
            : base(m, background)
        {
            if(TrackTooltip != null)
            {
                TrackTooltip.SetValues(trackPoint, track);
                TrackTooltip.OnSelectStartTrackPoint += new SelectStartPointDelegate(this.Tooltip_SelectStartPoint);
            }
            Track = track;
            TrackPoint = trackPoint;
        }

        protected override TooltipBase CreateTooltip()
        {
            return new TrackTooltip();
        }

        protected virtual void Tooltip_SelectStartPoint(object sender, SelectStartPointArgs args)
        {
            OnSelectStartTrackPoint?.Invoke(this, new SelectStartPointArgs() { Track = this.Track, TrackPoint = this.TrackPoint });
        }

        protected override void ProcessMouseEnter()
        {
            TrackTooltip.SetValues(TrackPoint, Track);
            base.ProcessMouseEnter();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                OnSelectStartTrackPoint?.Invoke(this, new SelectStartPointArgs() { Track = this.Track, TrackPoint = this.TrackPoint });
//                this.Track.FirstPosition = this.TrackPoint;
                TrackTooltip.SetValues(TrackPoint, Track);
            }
        }

        private bool _IsFirst = false;

        public virtual void SetAsFirst(bool isFirst)
        {
            if (isFirst)
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                UpdateVisual(true);
                TrackTooltip.SetValues(TrackPoint, Track);
            }
            else
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                UpdateVisual(true);
                TrackTooltip.SetValues(TrackPoint, Track);
            }
        }

        public virtual void SetAsLast(bool isLast)
        {
            if (isLast)
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                UpdateVisual(true);
            }
            else
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                UpdateVisual(true);
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (!_IsFirst)
            {
                OnSelectLastTrackPoint?.Invoke(this, new SelectStartPointArgs() { Track = this.Track, TrackPoint = this.TrackPoint });
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
             base.OnMouseLeave(e);
        }
    }
}
