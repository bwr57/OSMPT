using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demo.WindowsPresentation.CustomMarkers;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace Demo.WindowsPresentation.Source
{
    public class BusMarker : GMapMarker
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

        public BusMarker(PointLatLng pos)
            : base(pos)
        { }


        public virtual int GetSizeByZoom(double zoom)
        {
            if (zoom > 15)
                return 22;
            if (zoom > 13)
                return 16;
            if (zoom > 11)
                return 12;
            if (zoom > 9)
                return 6;
            return (4);
        }
        public virtual void ResizeByZoom(double zoom)
        {
            CircleVisual v = (CircleVisual)Shape;
            v.Size = GetSizeByZoom(zoom);
        }
    }
}
