namespace RodSoft.Core.Communications
{
    public interface IActivated
    {
        bool IsActive { get; }
    }

    public class ActivationStatusAgent : MessageBase, IActivated
    {
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
