using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RodSoft.Core.Communications
{
    public class MessageSerializatorBase<T> where T : CashedMessage
    {

        protected BinaryFormatter _BinaryFormatter = new BinaryFormatter();

      public virtual NameValueCollection PrepareCollection(T message, NameValueCollection nameValueCollection)
        {
            if (message is T)
            {
                if (nameValueCollection == null)
                    nameValueCollection = new NameValueCollection();
                nameValueCollection.Add("Time", message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
            }
            return nameValueCollection;
        }

        public virtual void SerializeObject(Stream stream, T obj)
        {
            _BinaryFormatter.Serialize(stream, obj);
        }

        public virtual T DeserializeObject(Stream stream)
        {
            return (T) _BinaryFormatter.Deserialize(stream);
        }
    }
}
