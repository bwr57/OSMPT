using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RodSoft.OSM.Tracking.Controls
{
    /// <summary>
    /// Логика взаимодействия для TrackPartInfoUserControl.xaml
    /// </summary>
    public partial class TrackPartInfoUserControl : UserControl
    {
        public event ClosePanelRequestDelegate FixRequest;
        public event ClosePanelRequestDelegate CloseRequest;
        public event ClosePanelRequestDelegate SaveRequest;

        public TrackPartInfoUserControl()
        {
            InitializeComponent();
        }
        public virtual void SetValues(TrackPoint currentPoint, Track track)
        {
            if (track.FirstPosition == null)
                return;
            TimeSpan timeInTrip = currentPoint.Time.Subtract(track.FirstPosition.Time);
            tbxTimeInTrip.Text = string.Format("{0} ч. {1} мин. {2} сек.", timeInTrip.Hours, timeInTrip.Minutes, timeInTrip.Seconds);
            tbxDistance.Text = (currentPoint.DistanceFromStart - track.FirstPosition.DistanceFromStart).ToString();
            tbxElevation.Text = currentPoint.Altitude.ToString();
            Speed.Text = currentPoint.Speed.ToString();
            Time.Text = currentPoint.Time.ToLongTimeString();
        }

        private void LabelClose_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseRequest?.Invoke(this);
        }

        private void LabelFix_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FixRequest?.Invoke(this);
        }

        private void LabelSave_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveRequest?.Invoke(this);
        }
    }
}
