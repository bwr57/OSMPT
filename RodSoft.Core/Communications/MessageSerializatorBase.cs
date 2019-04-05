using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.Text;

namespace RodSoft.Core.Communications
{
    public class MessageSerializatorBase<T> : Serializer<T> where T : MessageBase
    {

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




        public override string PrepareRequest(T message, string request)
        {
            if (message is T)
            {
                if (request == null)
                    request = "Time=" + message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.');
                //                request = ConcatRequest(request, "Time=" + message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
            }
            return request;
        }


    }
}
