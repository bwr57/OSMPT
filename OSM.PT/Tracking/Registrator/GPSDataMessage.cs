using RodSoft.Communications;
using System;

namespace Demo.WindowsPresentation.Tracking.Registrator
{
    [Serializable]
    public class GPSDataMessage : MessageBase
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double Speed;
        public double Course;

        public GPSDataMessage()
        { }

        public GPSDataMessage(object source)
            : base(source)
        { }
    }
}
