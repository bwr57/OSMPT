using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RodSoft.Core.Serialization
{
    public class StandardBinarySerializer<T> : Serializer<T> where T : class
    {
        protected BinaryFormatter _BinaryFormatter = new BinaryFormatter();

        public override void Serialize(Stream stream, object obj)
        {
            _BinaryFormatter.Serialize(stream, obj);
        }

        public override T Deserialize(Stream stream)
        {
            return (T)_BinaryFormatter.Deserialize(stream);
        }

        public override T Deserialize(byte[] serializedObject)
        {
            T obj = null;
            if(serializedObject != null && serializedObject.Length > 0)
            {
                using (Stream stream = new MemoryStream())
                {
                    stream.Write(serializedObject, 0, serializedObject.Length);
                    obj = Deserialize(stream);
                    stream.Dispose();
                }
            }
            return obj;
        }

    }
}
