using System;

namespace RodSoft.Core.Communications
{
    public interface IActivated
    {
        bool IsActive { get; }
    }

    [Serializable]
    public class ActivationStatusAgent : MessageBase, IActivated
    {
        [Transmitted(PropertyIndex = -1, MessageIdentificator = "0", WriteType= true)]
        public bool IsActive { get; set; }
        public ActivationStatusAgent()
        { }

        public ActivationStatusAgent(object source)
            : base(source)
        { }

    }

    public interface IActivatedController : IActivated
    {
        string Name { get; set; }
    }

    [Serializable]
    public class ActivatedControllerAgent : ActivationStatusAgent, IActivatedController
    {
        public string Name { get; set; }

        public ActivatedControllerAgent()
        { }

        public ActivatedControllerAgent(object source)
            : base(source)
        { }

    }

}
