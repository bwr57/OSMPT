using System;
using System.Threading;

namespace RodSoft.Core.Communications
{
    public abstract class ActiveRemoteDeviceDriverBase : RemoteDeviceDriverBase
    {
        public DateTime LastRegistrationTime;

        public double MaximumWaitTime { get; set; } = 3000;

        public int Period { get; set; } = 50;

        public bool IsWorking { get; protected set; } = true;


        public ActiveRemoteDeviceDriverBase()
            : base()
        {
            Start();
        }

        public ActiveRemoteDeviceDriverBase(string deviceName)
            : base(deviceName)
        {
            Start();
        }

        public void Start()
        {
            PrepareWorkingCircle();
            IsWorking = true;
            Thread workThread = new Thread(new ThreadStart(this.WorkingCircle));
            workThread.Start();
        }

        public virtual void Stop()
        {
            IsWorking = false;
        }

        protected virtual void PrepareWorkingCircle()
        {  }

        protected virtual void DisposeWorkingCircle()
        {  }

        protected virtual void WorkingCircle()
        {
            while (IsWorking)
            {
                Process();
                if(Period > 0)
                    Thread.Sleep(Period);
            }
            DisposeWorkingCircle();
        }

        protected abstract void Process();

        public override bool GetIsActive()
        {
            return DateTime.Now.Subtract(LastRegistrationTime).TotalMilliseconds <= MaximumWaitTime; 
        }

        public override void Dispose()
        {
            Stop();
            base.Dispose();
        }
    }


}
