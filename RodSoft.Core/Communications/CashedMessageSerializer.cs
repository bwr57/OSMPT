using RodSoft.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodSoft.Core.Communications
{
    public class CashedMessageSerializer<T> : BinarySerializer<CashedMessage<T>> where T : MessageBase
    {
        private BinarySerializer<T> _BinaryMessageSerializer = new BinarySerializer<T>();
        public BinarySerializer<T> BinaryMessageSerializer
        {
            get { return _BinaryMessageSerializer; }
            set
            {
                if(value == null)
                {
                    IsAutoCreateEmbededObjects = true;
                    _BinaryMessageSerializer = new BinarySerializer<T>();
                }
                else
                {
                    IsAutoCreateEmbededObjects = false;
                    _BinaryMessageSerializer = value;
                }
            }
        }

        public PostMessageSerializer<T> PostMessageSerializer { get; set; }

        public CashedMessageSerializer()
            : base()
        {
        }

        public CashedMessageSerializer(BinarySerializer<T> messageSerializer)
            : this()
        {
            BinaryMessageSerializer = messageSerializer;
        }

        public override void Serialize(Stream stream, object fieldValue, bool isWriteClassName)
        {
            base.Serialize(stream, fieldValue, isWriteClassName);
            if (!IsAutoCreateEmbededObjects && BinaryMessageSerializer != null)
            {
                CashedMessage<T> cashedMessage = fieldValue as CashedMessage<T>;
                stream.WriteByte((byte)(cashedMessage == null ? 0 : 1));
                BinaryMessageSerializer.Serialize(stream, cashedMessage.Message);
            }
        }

        public override object DeserializeObject(byte[] serializedObject, ref int currentIndex, object deserializedObject)
        {
            object deserializedMessage = base.DeserializeObject(serializedObject, ref currentIndex, deserializedObject);
            CashedMessage<T> deserializedCashedMessage = deserializedMessage as CashedMessage<T>;
            if (deserializedCashedMessage != null && deserializedCashedMessage.FileName != null)
                if (!IsAutoCreateEmbededObjects && BinaryMessageSerializer != null)
                {
                    if (serializedObject[currentIndex++] == 1)
                    {
                        deserializedCashedMessage.Message = BinaryMessageSerializer.CreateSerializedObjectInstance();
                        BinaryMessageSerializer.DeserializeObject(serializedObject, ref currentIndex, deserializedCashedMessage.Message);
                    }
                }
            return deserializedMessage;
        }

        public override CashedMessage<T> CreateSerializedObjectInstance()
        {
            return new CashedMessage<T>();
        }
    }
}
