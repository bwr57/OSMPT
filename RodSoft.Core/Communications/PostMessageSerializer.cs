using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.Text;
using RodSoft.Core.Serialization;

namespace RodSoft.Core.Communications
{
    public class PostMessageSerializer<T> : PostRequestSerializer<T> where T : MessageBase
    {
/*
        public override NameValueCollection PrepareCollection(T message, NameValueCollection nameValueCollection)
        {
            if (message is T)
            {
                if (nameValueCollection == null)
                {
                    nameValueCollection = base.PrepareCollection(message, nameValueCollection);
                    nameValueCollection.Add("Time", message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
                }
            }
            return nameValueCollection;
        }


*/

        public static string SerializeDateTime(DateTime messageTime)
        {
            return String.Format("{0:00}.{1:00}.{2:00} {3:00}.{4:00}.{5:00}", messageTime.Day, messageTime.Month, messageTime.Year, messageTime.Hour, messageTime.Minute, messageTime.Second);
        }

        public override string PrepareRequest(T message, string request)
        {
            if (message is T)
            {
                if (request == null && message is MessageBase)
                {
                    request = String.Format("Time={0}", SerializeDateTime(((MessageBase)message).Time));//.{1}.{2} {3}.{4}.{5}" + messageTime.Day, messageTime.Month, messageTime.Year, messageTime.Hour, messageTime.Minute, messageTime.Second);
                                                                                                        //                request = ConcatRequest(request, "Time=" + message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
                }
            }
            return request;
        }


    }
}
