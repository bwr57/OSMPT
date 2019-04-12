﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace RodSoft.Core.Communications.Http
{
    public class PostRemoteDiagnosticClient<T> : RemoteDiagnosticClient<T> where T : MessageBase
    {
        private ServiceClient _Client = new ServiceClient();

        public string ServerAddress { get; internal set; }


        public int Timeout
        {
            get { return _Client.Timeout; }
            set { _Client.Timeout = value; }
        }

        public PostRemoteDiagnosticClient(string serverAddress)
            : base()
        {
            this.ServerAddress = serverAddress;
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
         }

        protected override bool TrasmitMessage(CashedMessage<T> message)
        {
            if (_Client == null)
                return false;
//            NameValueCollection values = MessageSerializator.PrepareCollection(message, null);
            string requestTest = CashedMessageSerializer.MessageSerializer.PrepareRequest(message.Message, null);
            bool isTransmitted = false;
            try
            {
                WebRequest request = WebRequest.Create(ServerAddress);
                request.Method = "POST"; // для отправки используется метод Post
                                         // данные для отправки
                // преобразуем данные в массив байтов
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(requestTest);
                // устанавливаем тип содержимого - параметр ContentType
                request.ContentType = "application/x-www-form-urlencoded";
                // Устанавливаем заголовок Content-Length запроса - свойство ContentLength
                request.ContentLength = byteArray.Length;
                request.Timeout = 500;
                //записываем данные в поток запроса
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }


                WebResponse response =  request.GetResponse();
                string resp = "0";
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        resp = reader.ReadToEnd();
                    }
                }                //byte[] response = _Client.UploadValues(ServerAddress, "POST", values);
                                 //byte[] response = _Client.UploadValues("http://track.t1604.ru/api/track.php", "POST", values);
                                 //        string resp = Encoding.Default.GetString(response);http://localhost:54831/Default.aspx
                isTransmitted = resp.StartsWith("200 ") || resp == "200" || resp.StartsWith("200\n");
                if(LogFile != null && ((LogFile.CommucationLogMode > 1) || (!isTransmitted && LogFile.CommucationLogMode > 0)))
                    LogFile.WriteRecord(message, resp);
            }
            catch (Exception ex)
            {
                if (LogFile != null && LogFile.CommucationLogMode > 0)
                    LogFile.WriteRecord(message, ex.Message);
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
            base.DisposeClient();
        }
    }
}

