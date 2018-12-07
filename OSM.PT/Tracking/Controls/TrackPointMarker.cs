using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demo.WindowsPresentation.CustomMarkers;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace RodSoft.OSM.Tracking.Controls
{
    public class TrackPointMarker : GMapMarker
    {
        private int _ID;

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        private DateTime _TimeStamp = DateTime.Now;

        public DateTime TimeStamp
        {
            get { return _TimeStamp; }
            set { _TimeStamp = value; }
        }

        public TrackPointMarker(PointLatLng pos)
            : base(pos)
        { }


        public virtual int GetSizeByZoom(double zoom)
        {
            if (zoom > 15)
                return 8;
            if (zoom > 13)
                return 6;
            if (zoom > 11)
                return 4;
            if (zoom > 9)
                return 2;
            return (2);
        }
        public virtual void ResizeByZoom(double zoom)
        {
            CircleVisual v = (CircleVisual)Shape;
            v.Size = GetSizeByZoom(zoom);
        }
    }
}
