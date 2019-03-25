using System;

namespace RodSoft.Core.Communications
{
    [Serializable]
    public class CashedMessage<TMessage> where TMessage : MessageBase
    {
        public bool WasTransmitted;
        public int Index;
        public string FileName;
        public TMessage Message;

        public CashedMessage()
        {        }

        public CashedMessage(TMessage message)
        {
            Message = message;
        }

    }
}
