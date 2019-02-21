using Demo.WindowsPresentation.Tracking.Telemetry.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Demo.WindowsPresentation.Tracking.Telemetry
{
    public class TrackMessageSender : IDisposable
    {
        private readonly ServiceClient _Client = new ServiceClient();

        private Thread _Thread;

        private bool _IsEnabled = true;

        public bool IsActive { get; set; }

        private IList<TrackMessage> _Messages = new List<TrackMessage>();

        public int Timeout
        {
            get { return _Client.Timeout; }
            set { _Client.Timeout = value; }
        }


        public TrackMessageSender(string address)
        {
            _Thread = new Thread(new ThreadStart(ProcessTransmitting));
            _Thread.Start();
        }

        protected virtual void ProcessTransmitting()
        {
            TrackMessageSerializator trackMessageSerializator = new TrackMessageSerializator();
            while(_IsEnabled)
            {
                int initialCount = 0;
                while (_Messages.Count > 0)
                {
                    TrackMessage trackMessage;
                    lock (_Messages)
                        if (initialCount == _Messages.Count)
                            trackMessage = _Messages[0];
                        else
                        {
                            initialCount = _Messages.Count;
                            trackMessage = _Messages[initialCount- 1];
                        }
                    if(!trackMessage.WasTransmitted)
                    {
                        NameValueCollection values = trackMessageSerializator.PrepareCollection(trackMessage, null);
                        try
                        {
                            byte[] response = _Client.UploadValues("http://localhost:54831/Default.aspx", "POST", values); 
                            string resp = Encoding.Default.GetString(response);
                        }
                        catch
                        {
                            IsActive = false;
                            break;
                        }
                        IsActive = true;
                        lock (_Messages)
                        {
                            _Messages.Remove(trackMessage);
                            initialCount--;
                        }
                    }
                    if(!_IsEnabled)
                    {
                        break;
                    }

                }
                Thread.Sleep(500);
            }
            _Client.Dispose();
        }

        public void SendMessage(TrackMessage trackMessage)
        {
            trackMessage.WasTransmitted = false;
            lock(_Messages)
            {
                _Messages.Add(trackMessage);
            }
        }

        public virtual void Dispose()
        {
            _IsEnabled = false;
        }
    }
}
