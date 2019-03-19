using System;

namespace RodSoft.Core.Communications
{
    [Serializable]
    public class CashedMessage : MessageBase
    {
        public bool WasTransmitted;
        public int Index;
        public string FileName;

        public CashedMessage()
        {        }

        public CashedMessage(object source)
        {
            Assign(source);
        }

    }
}
