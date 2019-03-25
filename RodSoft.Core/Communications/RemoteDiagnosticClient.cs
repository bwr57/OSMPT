using RodSoft.Core.Communications.Http;
using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading;

namespace RodSoft.Core.Communications
{
    public abstract class RemoteDiagnosticClient<T> : IActivated, IDisposable where T : MessageBase
    {

        private Thread _Thread;

        private bool _IsEnabled = true;

        public bool IsActive { get; set; }

        private MessageSerializatorBase<T> _MessageSerializator;
        public MessageSerializatorBase<T> MessageSerializator
        {
            get { return _MessageSerializator; }
            set
            {
                if (CashService != null)
                    CashService.TrackMessageSerializator = value;
                _MessageSerializator = value;
            }
        }

        public CashService<T> CashService { get; set; } 
        public int TransmittingPeriod { get; internal set; } = 1000;

        public RemoteDiagnosticClient()
        {
            CashService = new CashService<T>();
            MessageSerializator = new MessageSerializatorBase<T>();
        }

        public RemoteDiagnosticClient(CommunicationSettings telemetrySettings)
        {
            if (telemetrySettings != null)
            {
                if (telemetrySettings.TransmittingPeriod > 0)
                    this.TransmittingPeriod = telemetrySettings.TransmittingPeriod;
            }
            CashService = new CashService<T>(telemetrySettings);
            MessageSerializator = new MessageSerializatorBase<T>();
        }

        public virtual void Start()
        {
            CashService.LoadCashedData();
            _Thread = new Thread(new ThreadStart(ProcessTransmitting));
            _Thread.Start();
        }

        protected abstract bool TrasmitMessage(CashedMessage<T> message);

        protected abstract bool IsReady();

        protected abstract void DisposeClient();

        protected virtual void ProcessTransmitting()
        {
            while(_IsEnabled)
            {
                if (IsReady())
                {
                    int initialCount = 0;
                    while (CashService.Messages.Count > 0)
                    {
                        CashedMessage<T> trackMessage;
                        lock (CashService.Messages)
                            if (initialCount == CashService.Messages.Count)
                                trackMessage = CashService.Messages[0];
                            else
                            {
                                initialCount = CashService.Messages.Count;
                                trackMessage = CashService.Messages[CashService.Messages.Count - 1];
                            }
                        if (!trackMessage.WasTransmitted)
                        {
                            bool isTransmitted = TrasmitMessage(trackMessage);
                            if(!isTransmitted)
                            {
                                IsActive = false;
                                break;
                            }

                            IsActive = true;
                            if (isTransmitted)
                            {
                                CashService.RegisterSending(trackMessage);
                                initialCount--;
                            }
                        }
                        if (!_IsEnabled)
                        {
                            break;
                        }

                    }
                }
                Thread.Sleep(TransmittingPeriod);
            }
            DisposeClient();
        }

        public virtual void SendMessage(T trackMessage)
        {
//            trackMessage.WasTransmitted = false;
            this.CashService.SaveMessage(trackMessage);
        }


        public virtual void Dispose()
        {
            _IsEnabled = false;
        }
    }
}


//public class TrackMessageSender : IDisposable
//{
//    private readonly ServiceClient _Client = new ServiceClient();

//    private Thread _Thread;

//    private bool _IsEnabled = true;

//    public bool IsActive { get; set; }


//    public int Timeout
//    {
//        get { return _Client.Timeout; }
//        set { _Client.Timeout = value; }
//    }

//    public CashService CashService { get; set; } = new CashService();

//    public TrackMessageSender(string address)
//    {
//        _Thread = new Thread(new ThreadStart(ProcessTransmitting));
//        _Thread.Start();
//    }

//    protected virtual void ProcessTransmitting()
//    {
//        TrackMessageSerializator trackMessageSerializator = new TrackMessageSerializator();
//        while (_IsEnabled)
//        {
//            int initialCount = 0;
//            while (CashService.Messages.Count > 0)
//            {
//                TrackMessage trackMessage;
//                lock (CashService.Messages)
//                    if (initialCount == CashService.Messages.Count)
//                        trackMessage = CashService.Messages[0];
//                    else
//                    {
//                        initialCount = CashService.Messages.Count;
//                        trackMessage = CashService.Messages[initialCount - 1];
//                    }
//                if (!trackMessage.WasTransmitted)
//                {
//                    NameValueCollection values = trackMessageSerializator.PrepareCollection(trackMessage, null);
//                    try
//                    {
//                        //        byte[] response = _Client.UploadValues("http://localhost:54831/Default.aspx", "POST", values); 
//                        byte[] response = _Client.UploadValues("", "POST", values);
//                        string resp = Encoding.Default.GetString(response);
//                    }
//                    catch
//                    {
//                        IsActive = false;
//                        break;
//                    }
//                    IsActive = true;
//                    CashService.RegisterSending(trackMessage);
//                    initialCount--;
//                }
//                if (!_IsEnabled)
//                {
//                    break;
//                }

//            }
//            Thread.Sleep(500);
//        }
//        _Client.Dispose();
//    }

//    public virtual void Dispose()
//    {
//        _IsEnabled = false;
//    }
//}
//}
