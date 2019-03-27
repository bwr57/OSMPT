using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;

namespace RodSoft.Core.Communications
{
    public class MessageSerializatorBase<T>  where T : MessageBase
    {
        public class DataMemberInfo
        {
            public string Name;
            public MemberInfo MemberInfo;
            public TransmittedAttribute AdditionalProperties;
        }

        protected IList<IList<DataMemberInfo>> _SectionList;

        protected BinaryFormatter _BinaryFormatter = new BinaryFormatter();

        public string MessageIdentificatorPrefix { get; set; }
        public string GeneralMessageIdentificatorPrefix { get; set; }

        public virtual NameValueCollection PrepareCollection(T message, NameValueCollection nameValueCollection)
        {
            if (message is T)
            {
                if (nameValueCollection == null)
                    nameValueCollection = new NameValueCollection();
                nameValueCollection.Add("Time", message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
            }
            return nameValueCollection;
        }

        public virtual NameValueCollection AutoPrepareCollection(T message, NameValueCollection nameValueCollection, bool isAnyMember)
        {
            if (message is T)
            {
                if (nameValueCollection == null)
                    nameValueCollection = new NameValueCollection();
                nameValueCollection.Add("Time", message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
            }
            if (message != null)
            {
                Type messageType = message.GetType();
                if (isAnyMember)
                {
                    foreach (MemberInfo member in messageType.GetMembers())
                    {
                        AddValueToCollection(message, nameValueCollection, member);
                    }


                    //foreach (FieldInfo member in messageType.GetFields())
                    //{
                    //    nameValueCollection.Add(member.Name, String.Format("{0}", member.GetValue(message)));
                    //}
                    //foreach (PropertyInfo member in messageType.GetProperties())
                    //{
                    //    nameValueCollection.Add(member.Name, String.Format("{0}", member.GetValue(message)));
                    //}
                }
                else
                {
                    if (_SectionList == null)
                        FormMembersList(message);
                    foreach (IList<DataMemberInfo> sectionMembers in _SectionList)
                        foreach (DataMemberInfo memberInfo in sectionMembers)
                        {
                            AddValueToCollection(message, nameValueCollection, memberInfo.MemberInfo);
                        }
                    //sectionMembers = messageMembers.OrderBy(memberInfo => ((TransmittedAttribute)memberInfo.GetCustomAttribute(typeof(TransmittedAttribute), true)).PropertyIndex).ToList();
                    //foreach (MemberInfo sectionMemberInfo in sectionMembers)
                    //{
                    //    AddValueToCollection(message, nameValueCollection, sectionMemberInfo);
                    //}
                }
            }
            return nameValueCollection;
        }

        public virtual void FormMembersList(T message)
        {
            Type messageType = message.GetType();
            _SectionList = new List<IList<DataMemberInfo>>();
            IList<DataMemberInfo> messageMembers = new List<DataMemberInfo>();
            //                    SortedDictionary<int, MemberInfo> messageMembers = new SortedDictionary<int, MemberInfo>();
            foreach (MemberInfo member in messageType.GetMembers())
            {
                TransmittedAttribute assignableAttribute = (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true);
                if (assignableAttribute != null)
                {
                    DataMemberInfo dataMemberInfo = new DataMemberInfo() { Name = member.Name, MemberInfo = member, AdditionalProperties = assignableAttribute };
                    messageMembers.Add(dataMemberInfo);
                }
                //                    messageMembers.Add(member);
                //                           messageMembers.Add(assignableAttribute.SectionIndex, member);
            }
            messageMembers = messageMembers.OrderBy(memberInfo => memberInfo.AdditionalProperties.SectionIndex).ToList();
            int sectionIndex = 0;
            IList<DataMemberInfo> sectionMembers = new List<DataMemberInfo>();
            //                    SortedDictionary<int, MemberInfo> sectionMembers = new SortedDictionary<int, MemberInfo>();
            foreach (DataMemberInfo member in messageMembers)
            {
                //                TransmittedAttribute assignableAttribute = (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true);
                if (member.AdditionalProperties.SectionIndex != sectionIndex)
                {
                    sectionMembers = sectionMembers.OrderBy(memberInfo => memberInfo.AdditionalProperties.PropertyIndex).ToList();
                    _SectionList.Add(sectionMembers);
                    sectionMembers = new List<DataMemberInfo>();
                    //foreach (MemberInfo sectionMemberInfo in sectionMembers)
                    //{
                    //    AddValueToCollection(message, nameValueCollection, sectionMemberInfo);
                    //}
                    //sectionMembers.Clear();
                    sectionIndex = member.AdditionalProperties.SectionIndex;
                }
                sectionMembers.Add(member);
            }
            sectionMembers = sectionMembers.OrderBy(memberInfo => memberInfo.AdditionalProperties.PropertyIndex).ToList();
            _SectionList.Add(sectionMembers);
        }

        public static string ConcatRequest(string request, string newPart)
        {
            return (String.IsNullOrEmpty(request) ? "" : request + "&") + newPart;
        }

        public virtual string PrepareRequest(T message, string request)
        {
            if (message is T)
            {
                if (request == null)
                    request = "Time=" + message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.');
                //                request = ConcatRequest(request, "Time=" + message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
            }
            return request;
        }

        public virtual string AutoPrepareRequest(T message, string request, bool isAnyMember)
        {
            if (message is T)
            {
                if (request == null)
                    request = "Time=" + message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.');
                //                request = ConcatRequest(request, "Time=" + message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
            }
            if (message != null)
            {
                Type messageType = message.GetType();
                if (isAnyMember)
                {
                    foreach (MemberInfo member in messageType.GetMembers())
                    {
                        request = AddToRequest(message, request, member);
                    }


                    //foreach (FieldInfo member in messageType.GetFields())
                    //{
                    //    nameValueCollection.Add(member.Name, String.Format("{0}", member.GetValue(message)));
                    //}
                    //foreach (PropertyInfo member in messageType.GetProperties())
                    //{
                    //    nameValueCollection.Add(member.Name, String.Format("{0}", member.GetValue(message)));
                    //}
                }
                else
                {
                    //IList<MemberInfo> messageMembers = new List<MemberInfo>();
                    ////                    SortedDictionary<int, MemberInfo> messageMembers = new SortedDictionary<int, MemberInfo>();
                    //foreach (MemberInfo member in messageType.GetMembers())
                    //{
                    //    TransmittedAttribute assignableAttribute = (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true);
                    //    if (assignableAttribute != null)
                    //        messageMembers.Add(member);
                    //    //                           messageMembers.Add(assignableAttribute.SectionIndex, member);
                    //}
                    //messageMembers = messageMembers.OrderBy(memberInfo => ((TransmittedAttribute)memberInfo.GetCustomAttribute(typeof(TransmittedAttribute), true)).SectionIndex).ToList();
                    //int sectionIndex = 0;
                    //IList<MemberInfo> sectionMembers = new List<MemberInfo>();
                    ////                    SortedDictionary<int, MemberInfo> sectionMembers = new SortedDictionary<int, MemberInfo>();
                    //foreach (MemberInfo member in messageMembers)
                    //{
                    //    TransmittedAttribute assignableAttribute = (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true);
                    //    if (assignableAttribute.SectionIndex != sectionIndex)
                    //    {
                    //        sectionMembers = sectionMembers.OrderBy(memberInfo => ((TransmittedAttribute)memberInfo.GetCustomAttribute(typeof(TransmittedAttribute), true)).PropertyIndex).ToList();
                    //        foreach (MemberInfo sectionMemberInfo in sectionMembers)
                    //        {
                    //            request = AddToRequest(message, request, sectionMemberInfo);
                    //        }
                    //        sectionMembers.Clear();
                    //        sectionIndex = assignableAttribute.SectionIndex;
                    //    }
                    //    sectionMembers.Add(member);
                    //}
                    //sectionMembers = sectionMembers.OrderBy(memberInfo => ((TransmittedAttribute)memberInfo.GetCustomAttribute(typeof(TransmittedAttribute), true)).PropertyIndex).ToList();
                    //foreach (MemberInfo sectionMemberInfo in sectionMembers)
                    //{
                    //    request = AddToRequest(message, request, sectionMemberInfo);
                    //}
                    if (_SectionList == null)
                        FormMembersList(message);
                    foreach (IList<DataMemberInfo> sectionMembers in _SectionList)
                        foreach (DataMemberInfo memberInfo in sectionMembers)
                        {
                            request = AddToRequest(message, request, memberInfo.MemberInfo, memberInfo.AdditionalProperties);
                        }
                }
            }
            return request;
        }

        public virtual void SerializeObject(Stream stream, CashedMessage<T> obj)
        {
            _BinaryFormatter.Serialize(stream, obj);
        }

        public virtual CashedMessage<T> DeserializeObject(Stream stream)
        {
            return (CashedMessage<T>)_BinaryFormatter.Deserialize(stream);
        }

        private void AddValueToCollection(T message, NameValueCollection nameValueCollection, MemberInfo member, TransmittedAttribute assignableAttribute)
        {
            string formatString = "{0}";
            string messageFieldIdentificator = member.Name;
            if (assignableAttribute != null)
            {
                if (assignableAttribute.FormatString != null)
                    formatString = "{0:" + assignableAttribute.FormatString + "}";
                if (!String.IsNullOrEmpty(assignableAttribute.MessageIdentificator))
                    messageFieldIdentificator = assignableAttribute.MessageIdentificator;
            }
            if (member.MemberType == MemberTypes.Field)
                nameValueCollection.Add(messageFieldIdentificator, String.Format(formatString, ((FieldInfo)member).GetValue(message)));
            else
            if (member.MemberType == MemberTypes.Property)
                nameValueCollection.Add(messageFieldIdentificator, String.Format(CultureInfo.GetCultureInfo("en-US"), formatString, ((PropertyInfo)member).GetValue(message)));
        }

        private void AddValueToCollection(T message, NameValueCollection nameValueCollection, MemberInfo member)
        {
            AddValueToCollection(message, nameValueCollection, member, (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true));
        }


        private string AddToRequest(T message, string request, MemberInfo member, TransmittedAttribute assignableAttribute)
        {
            string formatString = "{0}";
            string messageFieldIdentificator = String.IsNullOrEmpty(MessageIdentificatorPrefix) ? member.Name : MessageIdentificatorPrefix + member.Name;
            if (assignableAttribute != null)
            {
                if (assignableAttribute.FormatString != null)
                    formatString = "{0:" + assignableAttribute.FormatString + "}";
                if (!String.IsNullOrEmpty(assignableAttribute.MessageIdentificator))
                    messageFieldIdentificator = assignableAttribute.MessageIdentificator;
                if (assignableAttribute.WriteType)
                    messageFieldIdentificator = String.IsNullOrEmpty(GeneralMessageIdentificatorPrefix) ? message.GetType().Name + messageFieldIdentificator : GeneralMessageIdentificatorPrefix + messageFieldIdentificator;
                else
                {
                    if (!String.IsNullOrEmpty(MessageIdentificatorPrefix))
                    {
                        messageFieldIdentificator = MessageIdentificatorPrefix + messageFieldIdentificator;
                    }
                }
            }
            if (member.MemberType == MemberTypes.Field)
                request = ConcatRequest(request, messageFieldIdentificator + "=" + String.Format(CultureInfo.GetCultureInfo("en-US"), formatString, ((FieldInfo)member).GetValue(message)));
            else
            if (member.MemberType == MemberTypes.Property)
                request = ConcatRequest(request, messageFieldIdentificator + "=" + String.Format(CultureInfo.GetCultureInfo("en-US"), formatString, ((PropertyInfo)member).GetValue(message)));
            return request;
        }

        private string AddToRequest(T message, string request, MemberInfo member)
        {
            return AddToRequest(message, request, member, (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true));
        }
    }
}
