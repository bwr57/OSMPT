using RodSoft.OSM.Tracking;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;

namespace Demo.WindowsPresentation.Tracking.Telemetry.Serialization
{
    public class GPSMessageSerializator
    {
        public virtual NameValueCollection PrepareCollection(TrackPoint message, NameValueCollection nameValueCollection)
        {
            if (nameValueCollection == null)
                nameValueCollection = new NameValueCollection();
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");
            if (message is TrackPoint)
            {
                nameValueCollection.Add("GPS", "1");
                nameValueCollection.Add("Lat", message.Latitude.ToString("0.######", cultureInfo));
                nameValueCollection.Add("Lng", message.Longitude.ToString("0.######", cultureInfo));
                nameValueCollection.Add("Altitude", message.Elevation.Value.ToString("0.#", cultureInfo));
                nameValueCollection.Add("Speed", message.Speed.ToString("0", cultureInfo));
                nameValueCollection.Add("Bearing", message.Bearing.HasValue ? message.Bearing.Value.ToString("0", cultureInfo) : "0");
            }
            else
                nameValueCollection.Add("GPS", "0");
            return nameValueCollection;
        }
    }
}
