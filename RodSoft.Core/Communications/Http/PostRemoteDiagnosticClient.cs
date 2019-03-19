using System;
using System.Collections.Specialized;
using System.Text;

namespace RodSoft.Core.Communications.Http
{
    public class PostRemoteDiagnosticClient<T> : RemoteDiagnosticClient<T> where T : CashedMessage
    {
        private ServiceClient _Client = new ServiceClient();

        public string ServerAddress { get; internal set; }

        MessageSerializatorBase<T> _MessageSerializator = new MessageSerializatorBase<T>();

        public int Timeout
        {
            get { return _Client.Timeout; }
            set { _Client.Timeout = value; }
        }

        public PostRemoteDiagnosticClient(string serverAddress)
            : base()
        {
            this.ServerAddress = serverAddress;
            Start();
        }

        public PostRemoteDiagnosticClient(CommunicationSettings telemetrySettings)
            : base(telemetrySettings)
        {
            if (telemetrySettings != null)
            {
                this.ServerAddress = telemetrySettings.ServerAddress;
                if (telemetrySettings.RequestTimeout > 0)
                    this.Timeout = telemetrySettings.RequestTimeout;
            }
            Start();
        }

        protected override bool TrasmitMessage(T message)
        {
            NameValueCollection values = _MessageSerializator.PrepareCollection(message, null);
            bool isTransmitted = false;
            try
            {
                byte[] response = _Client.UploadValues(ServerAddress, "POST", values);
                //byte[] response = _Client.UploadValues("http://track.t1604.ru/api/track.php", "POST", values);
                string resp = Encoding.Default.GetString(response);
                isTransmitted = resp.StartsWith("200 ") || resp == "200";
            }
            catch (Exception ex)
            {
                return false;
            }
            return isTransmitted;
        }

        protected override bool IsReady()
        {
            return !String.IsNullOrEmpty(ServerAddress);
        }

        protected override void DisposeClient()
        {
            if (_Client != null)
            {
                _Client.Dispose();
                _Client = null;
            }
        }
    }
}

