using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RodSoft.Core.Communications
{
    public abstract class RemoteDiagnosticClient<T> : IActivatedController, IDisposable where T : MessageBase
    {

        private Thread _Thread;

        private bool _IsEnabled = true;

        public bool IsActive { get; set; }

        public string Name { get; set; }

        private CommucationLogFile<T> _LogFile = new CommucationLogFile<T>();

        public CommucationLogFile<T> LogFile
        {
            get { return _LogFile; }
            set
            {
                _LogFile = value;
                if (CashService != null)
                    CashService.LogFile = value;
            }
        }

        private PostMessageSerializer<T> _PostMessageSerializer;

        public PostMessageSerializer<T> PostMessageSerializer
        {
            get
            {
                return _PostMessageSerializer;
            }
            set
            {
                _PostMessageSerializer = value;
            }
        }

        private CashedMessageSerializer<T> _CashedMessageSerializer;
        public CashedMessageSerializer<T> CashedMessageSerializer
        {
            get { return _CashedMessageSerializer; }
            set
            {
                if (CashService != null)
                    CashService.TrackMessageSerializator = value;
                _CashedMessageSerializer = value;
            }
        }

        public CashService<T> CashService { get; set; } 
        public int TransmittingPeriod { get; internal set; } = 1000;

        public RemoteDiagnosticClient()
        {
            CashService = new CashService<T>();
            this.CashService.LogFile = LogFile;
            CashedMessageSerializer = new CashedMessageSerializer<T>();
        }

        public RemoteDiagnosticClient(CommunicationSettings telemetrySettings)
        {
            if (telemetrySettings != null)
            {
                if (telemetrySettings.TransmittingPeriod > 0)
                    this.TransmittingPeriod = telemetrySettings.TransmittingPeriod;
                LogFile.CommucationLogFileName = telemetrySettings.CommucationLogFileName;
                LogFile.CommucationLogMode = telemetrySettings.CommucationLogMode;
            }
            CashService = new CashService<T>(telemetrySettings);
            this.CashService.LogFile = LogFile;
            CashedMessageSerializer = new CashedMessageSerializer<T>();

        }

        public virtual void Start()
        {
            CashService.LoadCashedData();
            _Thread = new Thread(new ThreadStart(ProcessTransmitting));
            _Thread.IsBackground = true;
            _Thread.Start();
        }

        protected abstract bool TrasmitMessage(CashedMessage<T> message);

        protected abstract bool IsReady();

        protected virtual void DisposeClient()
        {
            CashService.Dispose();
        }

        protected virtual void ProcessTransmitting()
        {
            while(_IsEnabled)
            {
                if (IsReady())
                {
                    int initialCount = 0;
                    DateTime now = DateTime.Now;
                    lock (CashService.Messages)
                    {
                        IEnumerable<CashedMessage<T>> lastMessages = CashService.Messages.Where(message => message.WasTransmitted && message.Message != null && now.Subtract(message.Message.Time).TotalSeconds < 10);
                        if (lastMessages.Count() == 0)
                        {
                            initialCount = CashService.Messages.Count;
                        }
                        else
                        {
                            DateTime lastMessageDateTime = lastMessages.Max(message => message.Message.Time);
                            try
                            {
                                initialCount = CashService.Messages.IndexOf(lastMessages.Where(message => message.Message.Time == lastMessageDateTime).FirstOrDefault());
                            }
                            catch { }
                        }
                    }
                    while (CashService.Messages.Count > 0)
                    {
                        CashedMessage<T> trackMessage;
                        lock (CashService.Messages)
                        {

                            if (initialCount == CashService.Messages.Count)
                                trackMessage = CashService.Messages[0];
                            else
                            {
                                if (initialCount == 0)
                                    trackMessage = CashService.Messages[CashService.Messages.Count - 1];
                                else
                                    trackMessage = CashService.Messages[initialCount];
                                initialCount = CashService.Messages.Count;
                            }
                        }
                        if (!trackMessage.WasTransmitted)
                        {
                            bool isTransmitted = false;
                            string checkErrorMessage = null;
                            if (CheckTransmittedMessage(trackMessage, ref checkErrorMessage))
                            {
                                isTransmitted = TrasmitMessage(trackMessage);
                                if (!isTransmitted)
                                {
                                    IsActive = false;
                                    break;
                                }

                                IsActive = true;
                            }
                            else
                            {
                                isTransmitted = true;
                                if (LogFile != null && LogFile.CommucationLogMode == CommucationLogMode.Full)
                                    lock (LogFile)
                                        LogFile.WriteRecord(trackMessage, checkErrorMessage);
                            }
                            if (isTransmitted)
                            {
                                initialCount += CashService.RegisterSending(trackMessage);
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

        protected virtual bool CheckTransmittedMessage(T message, ref string checkErrorMessage)
        {
            if (message != null)
                return true;
            checkErrorMessage = "Null message";
            return false;
        }

        protected virtual bool CheckTransmittedMessage(CashedMessage<T> message, ref string checkErrorMessage)
        {
            if (message != null)
                return CheckTransmittedMessage(message.Message, ref checkErrorMessage);
            checkErrorMessage = "Null cash record";
            return false;
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
