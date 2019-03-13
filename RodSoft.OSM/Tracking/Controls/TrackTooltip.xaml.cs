using RodSoft.OSM.UI.Controls;
using System;

namespace RodSoft.OSM.Tracking.Controls
{
    /// <summary>
    /// Interaction logic for TrolleyTooltip.xaml
    /// </summary>
    public partial class TrackTooltip : TooltipBase
    {
        public event SelectStartPointDelegate OnSelectStartTrackPoint;

        private TrackPoint _TrackPoint;

        public TrackPoint TrackPoint
        {
            get { return _TrackPoint; }
            set
            {
                _TrackPoint = value;
            }
        }

        public TrackTooltip()
        {
            InitializeComponent();
        }

        public TrackTooltip(TrackPoint currentPoint, Track track)
        {
            InitializeComponent();
            SetValues(currentPoint, track);
        }

        public override void SetValues(VehicleGeoData currentPoint)
        {
            Speed.Text = currentPoint.Speed.ToString();
            Time.Text = currentPoint.Time.ToLongTimeString();
        }

        public virtual void SetValues(TrackPoint currentPoint, Track track)
        {
            SetValues(currentPoint);
            if (track.FirstPosition == null)
                return;
            TimeSpan timeInTrip = currentPoint.Time.Subtract(track.FirstPosition.Time);
            tbxTimeInTrip.Text = string.Format("{0} ч. {1} мин. {2} сек.", timeInTrip.Hours, timeInTrip.Minutes, timeInTrip.Seconds);
            tbxDistance.Text = Math.Round((currentPoint.DistanceFromStart - track.FirstPosition.DistanceFromStart).Value).ToString();
            tbxElevation.Text = currentPoint.Altitude.ToString();
            _TrackPoint = currentPoint;
        }

        private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnSelectStartTrackPoint?.Invoke(this, new SelectStartPointArgs() { TrackPoint = this.TrackPoint });
        }
    }
}
