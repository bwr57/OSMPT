using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodSoft.Core.Communications
{
    public class CashedMessageSerializer<T> : Serializer<CashedMessage<T>> where T : MessageBase
    {
        private MessageSerializatorBase<T> _MessageSerializer = new MessageSerializatorBase<T>();
        public MessageSerializatorBase<T> MessageSerializer
        {
            get { return _MessageSerializer; }
            set
            {
                if(value == null)
                {
                    IsAutoCreateEmbededObjects = true;
                    _MessageSerializer = new MessageSerializatorBase<T>();
                }
                else
                {
                    IsAutoCreateEmbededObjects = false;
                    _MessageSerializer = value;
                }
            }
        }

        public CashedMessageSerializer()
            : base()
        {
        }

        public CashedMessageSerializer(MessageSerializatorBase<T> messageSerializer)
            : this()
        {
            MessageSerializer = messageSerializer;
        }

        public override void WriteToBinaryStream(Stream stream, object fieldValue, bool isWriteClassName)
        {
            base.WriteToBinaryStream(stream, fieldValue, isWriteClassName);
            if (!IsAutoCreateEmbededObjects && MessageSerializer != null)
            {
                CashedMessage<T> cashedMessage = fieldValue as CashedMessage<T>;
                stream.WriteByte((byte)(cashedMessage == null ? 0 : 1));
                MessageSerializer.WriteToBinaryStream(stream, cashedMessage.Message, false);
            }
        }

        public override object DeserializeObject(byte[] serializedObject, ref int currentIndex, object deserializedObject)
        {
            object deserializedMessage = base.DeserializeObject(serializedObject, ref currentIndex, deserializedObject);
            CashedMessage<T> deserializedCashedMessage = deserializedMessage as CashedMessage<T>;
            if (deserializedCashedMessage != null)
                if (!IsAutoCreateEmbededObjects && MessageSerializer != null)
                {
                    if (serializedObject[currentIndex++] == 1)
                    {
                        deserializedCashedMessage.Message = MessageSerializer.CreateSerializedObjectInstance();
                        MessageSerializer.DeserializeObject(serializedObject, ref currentIndex, deserializedCashedMessage.Message);
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
