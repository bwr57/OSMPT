using System;

namespace RodSoft.Core.Communications
{
    public class RemoteDeviceDriverBase : IActivatedController, IDisposable
    {
        public string Name { get; set ; }

        private bool _IsActive;

        public bool IsActive
        {
            get { return GetIsActive(); }
            protected set { SetIsActive(value); }
        }
            
        public RemoteDeviceDriverBase()
        { }

        public RemoteDeviceDriverBase(string name)
            :this()
        {
            Name = name;
        }

        public virtual bool GetIsActive()
        {
            return _IsActive;
        }

        protected virtual void SetIsActive(bool isActive)
        {
            _IsActive = isActive;
        }

        public virtual void Dispose()
        { }
    }
}
