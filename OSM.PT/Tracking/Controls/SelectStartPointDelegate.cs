using System;
using System.Windows;

namespace RodSoft.OSM.Tracking.Controls
{
    public delegate void SelectStartPointDelegate(object sender, SelectStartPointArgs args);

    public class SelectStartPointArgs : EventArgs
    {
        public Track Track;
        public TrackPoint TrackPoint; 
    }

    public delegate void ClosePanelRequestDelegate(UIElement sender);
}
