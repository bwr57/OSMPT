using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RodSoft.OSM.Tracking.Telemetry.Serialization
{
    public class TrackMessageSerializator
    {
        protected GPSMessageSerializator gpsMessageSerializator = new GPSMessageSerializator();

        protected BinaryFormatter _BinaryFormatter = new BinaryFormatter();

        public virtual NameValueCollection PrepareCollection(TrackMessage message, NameValueCollection nameValueCollection)
        {
            if (message is TrackMessage)
            {
                if (nameValueCollection == null)
                    nameValueCollection = new NameValueCollection();
                nameValueCollection.Add("Time", message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
                nameValueCollection.Add("Vehicle", message.Vehicle);
                gpsMessageSerializator.PrepareCollection(message.TrackPoint, nameValueCollection);
            }
            return nameValueCollection;
        }

        public void SerializeObject<TrackMessage>(Stream stream, TrackMessage obj)
        {
            _BinaryFormatter.Serialize(stream, obj);
        }

        public TrackMessage DeserializeObject<TrackMessage>(Stream stream)
        {
            return (TrackMessage) _BinaryFormatter.Deserialize(stream);
        }
    }
}
