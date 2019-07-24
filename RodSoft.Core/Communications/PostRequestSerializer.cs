using RodSoft.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RodSoft.Core.Communications
{
    public class DataMemberInfo
    {
        public string Name;
        public MemberInfo MemberInfo;
        public TransmittedAttribute AdditionalProperties;
    }
    public class PostRequestSerializer<T> : Serializer<T> where T : class
    {
        protected static IDictionary<Type, IList<IList<DataMemberInfo>>> _SectionList = new Dictionary<Type, IList<IList<DataMemberInfo>>>();

        public string MessageIdentificatorPrefix { get; set; }
        public string GeneralMessageIdentificatorPrefix { get; set; }

        protected static IDictionary<Type, IList<DataMemberInfo>> MembersData = new Dictionary<Type, IList<DataMemberInfo>>();
        protected static IDictionary<Type, IDictionary<MemberInfo, string>> MemberIdentifiersInMessage = new Dictionary<Type, IDictionary<MemberInfo, string>>();

        public TransmittedAttribute GetTransmittedAttribute(MemberInfo memberInfo)
        {
            object[] attributes = memberInfo.GetCustomAttributes(true);
            for(int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is TransmittedAttribute)
                    return (TransmittedAttribute)attributes[i];
            }
            return null;
        }

        protected override MemberInfo[] CreateMembersList(Type messageType)
        {
            MemberInfo[] members = null;
            IList<IList<DataMemberInfo>> sectionList = null;
            lock (_SectionList)
            {
                if (_SectionList.ContainsKey(messageType))
                {
                    return _Members[messageType];
                }
                sectionList = new List<IList<DataMemberInfo>>();
                _SectionList.Add(messageType, sectionList);
            }
                IList<DataMemberInfo> messageMembers = new List<DataMemberInfo>();
                IDictionary<MemberInfo, string> MemberIdentifiers = new Dictionary<MemberInfo, string>();
            members = base.CreateMembersList(messageType);
                //                    SortedDictionary<int, MemberInfo> messageMembers = new SortedDictionary<int, MemberInfo>();
                foreach (MemberInfo member in members)
                {
                    TransmittedAttribute assignableAttribute = GetTransmittedAttribute(member); // (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true);
                    if (assignableAttribute != null)
                    {
                        DataMemberInfo dataMemberInfo = new DataMemberInfo() { Name = member.Name, MemberInfo = member, AdditionalProperties = assignableAttribute };
                        messageMembers.Add(dataMemberInfo);
                    }
                MemberIdentifiers.Add(member, FormIdentifier(messageType, member, assignableAttribute));
                    //                    messageMembers.Add(member);
                    //                           messageMembers.Add(assignableAttribute.SectionIndex, member);
                }
            messageMembers = messageMembers.OrderBy(memberInfo => memberInfo.AdditionalProperties.SectionIndex).ToList();
            lock(MembersData)
                MembersData.Add(messageType, messageMembers);
                int sectionIndex = 0;
                IList<DataMemberInfo> sectionMembers = new List<DataMemberInfo>();
                //                    SortedDictionary<int, MemberInfo> sectionMembers = new SortedDictionary<int, MemberInfo>();
                foreach (DataMemberInfo member in messageMembers)
                {
                    //                TransmittedAttribute assignableAttribute = (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true);
                    if (member.AdditionalProperties.SectionIndex != sectionIndex)
                    {
                        sectionMembers = sectionMembers.OrderBy(memberInfo => memberInfo.AdditionalProperties.PropertyIndex).ToList();
                        sectionList.Add(sectionMembers);
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
                sectionList.Add(sectionMembers);
            lock(MemberIdentifiersInMessage)
                MemberIdentifiersInMessage.Add(messageType, MemberIdentifiers);
            
            return members;
        }
/*
        public virtual NameValueCollection PrepareCollection(T message, NameValueCollection nameValueCollection)
        {
            if (message is T)
            {
                if (nameValueCollection == null)
                {
                    nameValueCollection = new NameValueCollection();
                }
            }
            return nameValueCollection;
        }

        public virtual NameValueCollection AutoPrepareCollection(T message, NameValueCollection nameValueCollection, bool isAnyMember)
        {
            nameValueCollection = PrepareCollection(message, nameValueCollection);
            //if (message is T)
            //{
            //    if (nameValueCollection == null)
            //        nameValueCollection = new NameValueCollection();
            //    nameValueCollection.Add("Time", message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
            //}
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
            nameValueCollection.Add(messageFieldIdentificator, String.Format(formatString, GetMemberValue(message, member)));
        }

        private void AddValueToCollection(T message, NameValueCollection nameValueCollection, MemberInfo member)
        {
            AddValueToCollection(message, nameValueCollection, member, (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true));
        }
*/


        public virtual string PrepareRequest(T message, string request)
        {
            if (message is T)
            {
                if (request == null)
                    request = "";
            }
            return request;
        }

        public virtual string AutoPrepareRequest(T message, string request, bool isAnyMember)
        {
            request = PrepareRequest(message, request);
            //if (message is T)
            //{
            //    if (request == null)
            //        request = "Time=" + message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.');
            //    //                request = ConcatRequest(request, "Time=" + message.Time.ToShortDateString() + " " + message.Time.ToLongTimeString().Replace(':', '.'));
            //}
            if (message != null)
            {
                Type messageType = message.GetType();
                MemberInfo[] members = CreateMembersList(messageType);
                if (isAnyMember)
                {
                    foreach (MemberInfo member in members)
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
//                    if (_SectionList == null)
                    IList<IList<DataMemberInfo>> sectionList = _SectionList[messageType];
                    foreach (IList<DataMemberInfo> sectionMembers in sectionList)
                        foreach (DataMemberInfo memberInfo in sectionMembers)
                        {
                            request = AddToRequest(message, request, memberInfo.MemberInfo, memberInfo.AdditionalProperties);
                        }
                }
            }
            return request;
        }

        public virtual string FormIdentifier(Type objectType, MemberInfo member, TransmittedAttribute transmittedAttribute)
        {
            string messageFieldIdentificator = String.IsNullOrEmpty(MessageIdentificatorPrefix) ? member.Name : MessageIdentificatorPrefix + member.Name;
            if (transmittedAttribute != null)
            {
                if (!String.IsNullOrEmpty(transmittedAttribute.MessageIdentificator))
                    messageFieldIdentificator = transmittedAttribute.MessageIdentificator;
                if (transmittedAttribute.WriteType)
                    messageFieldIdentificator = String.IsNullOrEmpty(GeneralMessageIdentificatorPrefix) ? objectType.Name + messageFieldIdentificator : GeneralMessageIdentificatorPrefix + messageFieldIdentificator;
                else
                {
                    if (!String.IsNullOrEmpty(MessageIdentificatorPrefix))
                    {
                        messageFieldIdentificator = MessageIdentificatorPrefix + messageFieldIdentificator;
                    }
                }
            }
            return messageFieldIdentificator;
        }

        private string AddToRequest(T message, string request, MemberInfo member, TransmittedAttribute assignableAttribute)
        {
            Type objectType = message.GetType();
            if (MemberIdentifiersInMessage.ContainsKey(objectType))
                CreateMembersList(objectType);
            IDictionary<MemberInfo, string> memberIdentifiers = MemberIdentifiersInMessage[objectType];
            string memberIdentifier = memberIdentifiers[member];
            string formatString = "{0}";
            if (assignableAttribute != null && assignableAttribute.FormatString != null)
                    formatString = "{0:" + assignableAttribute.FormatString + "}";
            //string messageFieldIdentificator = String.IsNullOrEmpty(MessageIdentificatorPrefix) ? member.Name : MessageIdentificatorPrefix + member.Name;
            //if (assignableAttribute != null)
            //{
            //    if (assignableAttribute.FormatString != null)
            //        formatString = "{0:" + assignableAttribute.FormatString + "}";
            //    if (!String.IsNullOrEmpty(assignableAttribute.MessageIdentificator))
            //        messageFieldIdentificator = assignableAttribute.MessageIdentificator;
            //    if (assignableAttribute.WriteType)
            //        messageFieldIdentificator = String.IsNullOrEmpty(GeneralMessageIdentificatorPrefix) ? message.GetType().Name + messageFieldIdentificator : GeneralMessageIdentificatorPrefix + messageFieldIdentificator;
            //    else
            //    {
            //        if (!String.IsNullOrEmpty(MessageIdentificatorPrefix))
            //        {
            //            messageFieldIdentificator = MessageIdentificatorPrefix + messageFieldIdentificator;
            //        }
            //    }
            //}
            request = ConcatRequest(request, memberIdentifier + "=" + String.Format(CultureInfo.GetCultureInfo("en-US"), formatString, GetMemberValue(message, member)));
            return request;
        }

        private string AddToRequest(T message, string request, MemberInfo member)
        {
            return AddToRequest(message, request, member, (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true));
        }


        public static string ConcatRequest(string request, string newPart)
        {
            return (String.IsNullOrEmpty(request) ? "" : request + "&") + newPart;
        }

        public override byte[] Serialize(object obj)
        {
            if (obj is T)
            {
                string serializedObject = PrepareRequest((T)obj, null);
                return Encoding.UTF8.GetBytes(serializedObject);
            }
            return new byte[0];
        }

        public override void Serialize(Stream stream, object obj)
        {
            if (stream != null && obj != null)
            {
                byte[] serializedObject = Serialize(obj);
                stream.Write(serializedObject, 0, serializedObject.Length);
            }
        }

        public virtual void FillMemberValues(object deserializedObject, IDictionary<string, string> recievedData)
        {
            if (deserializedObject != null)
            {
                Type objectType = deserializedObject.GetType();
                MemberInfo[] members = CreateMembersList(objectType);
                IDictionary<MemberInfo, string> memberIdentifiers = MemberIdentifiersInMessage[objectType];
                IList<DataMemberInfo> membersData = MembersData[objectType];
                for (int i = 0; i < members.Length; i++)
                {
                    MemberInfo member = members[i];
                    //if (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property)
                    //{

                    //TransmittedAttribute assignableAttribute = memberData.AdditionalProperties; // (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute));
                    //string messageFieldIdentificator = String.IsNullOrEmpty(MessageIdentificatorPrefix) ? memberData.Name : MessageIdentificatorPrefix + memberData.Name;
                    //if (assignableAttribute != null)
                    //{
                    //    if (!String.IsNullOrEmpty(assignableAttribute.MessageIdentificator))
                    //        messageFieldIdentificator = assignableAttribute.MessageIdentificator;
                    //    if (assignableAttribute.WriteType)
                    //        messageFieldIdentificator = String.IsNullOrEmpty(GeneralMessageIdentificatorPrefix) ? deserializedObject.GetType().Name + messageFieldIdentificator : GeneralMessageIdentificatorPrefix + messageFieldIdentificator;
                    //    else
                    //    {
                    //        if (!String.IsNullOrEmpty(MessageIdentificatorPrefix))
                    //        {
                    //            messageFieldIdentificator = MessageIdentificatorPrefix + messageFieldIdentificator;
                    //        }
                    //    }
                    //}
                    Type memberType = GetMemberType(member);
                    string memberIdentifier = memberIdentifiers[member];
                    string memberTypeName = memberType.FullName;
                    if (memberType.IsClass && memberTypeName != "System.String")
                    {
                        object embededObject = null;
                        if (!IsAutoCreateEmbededObjects)
                            embededObject = CustomDeserializeObject(deserializedObject, member, recievedData);
                        else
                        {
                            if (recievedData.ContainsKey(memberIdentifier) && recievedData[memberIdentifier] == "1")
                            {
                                embededObject = CreateSerializedObjectInstance(memberType);
                            }
                        }
                        if (embededObject != null)
                        {
                            FillMemberValues(embededObject, recievedData);
                            SetMemberValue(deserializedObject, member, embededObject);
                        }
                        continue;
                    }
                    if (recievedData.ContainsKey(memberIdentifier))
                    {
                        string valueString = recievedData[memberIdentifier];
                        bool wasParsed = false;
                        if (memberType.IsValueType)
                        {
                            if (memberTypeName == "System.Byte")
                            {
                                byte value;
                                if (Byte.TryParse(valueString, out value))
                                    SetMemberValue(deserializedObject, member, value);
                                continue;
                            }
                            if (memberTypeName == "System.Int16")
                            {
                                Int16 value;
                                if (short.TryParse(valueString, out value))
                                    SetMemberValue(deserializedObject, member, value);
                                continue;
                            }
                            if (memberTypeName == "System.Int32")
                            {
                                int value;
                                if (int.TryParse(valueString, out value))
                                    SetMemberValue(deserializedObject, member, value);
                                continue;
                            }
                            if (memberTypeName == "System.UInt32")
                            {
                                uint value;
                                if (uint.TryParse(valueString, out value))
                                    SetMemberValue(deserializedObject, member, value);
                                continue;
                            }
                            if (memberTypeName == "System.Boolean")
                            {
                                byte value;
                                if (Byte.TryParse(valueString, out value))
                                    SetMemberValue(deserializedObject, member, value != 0);
                                continue;
                            }
                            if (memberTypeName == "System.Single")
                            {
                                Single value;
                                if (Single.TryParse(valueString, out value))
                                    SetMemberValue(deserializedObject, member, value);
                                continue;
                            }
                            if (memberTypeName == "System.Double")
                            {
                                double value;
                                if (double.TryParse(valueString, out value))
                                    SetMemberValue(deserializedObject, member, value);
                                continue;
                            }
                            if (memberTypeName == "System.DateTime")
                            {
                                DateTime value;
                                string[] parts = valueString.Split(' ');
                                if (parts.Length == 2)
                                {
                                    valueString = parts[0] + " " + parts[1].Replace('.', ':');
                                    if (DateTime.TryParse(valueString, CultureInfo.GetCultureInfo("ru", "RU") , DateTimeStyles.AdjustToUniversal, out value))
                                        SetMemberValue(deserializedObject, member, value);
                                }
                                continue;
                            }
                        }
                        else
                        {
                            if (memberType.FullName == "System.String")
                            {
                                SetMemberValue(deserializedObject, member, valueString);
                            }
                            //else
                            //if (memberType.IsClass)
                            //{
                            //    object embededObject = null;
                            //    if (!IsAutoCreateEmbededObjects)
                            //    {
                            //        embededObject = CustomDeserializeObject(deserializedObject, member, recievedData);
                            //    }
                            //    else
                            //    {
                            //        embededObject = CreateSerializedObjectInstance(memberType);
                            //        FillMemberValues(embededObject, recievedData);
                            //    }
                            //    if (embededObject != null)
                            //        SetMemberValue(deserializedObject, member, embededObject);
                            //}
                        }

                    }
                }
            }
        }

        protected virtual object CustomDeserializeObject(object deserializedObject, MemberInfo member, IDictionary<string, string> recievedData)
        {
            if (deserializedObject != null)
            {
                Type objectType = deserializedObject.GetType();
                IDictionary<MemberInfo, string> memberIdentifiers = MemberIdentifiersInMessage[objectType];
                if (memberIdentifiers.ContainsKey(member))
                {
                    string memberIdentifier = memberIdentifiers[member];
                    if (memberIdentifier != null && recievedData.ContainsKey(memberIdentifier))
                    {
                        int value = 0;
                        if (int.TryParse(recievedData[memberIdentifier], out value) && value > 0)
                        {
                            return CreateSerializedObjectInstance(GetMemberType(member));
                        }
                    }
                }
            }
            return null;
        }

        public override T Deserialize(byte[] serializedData)
        {
            IDictionary<string, string> recievedData = new Dictionary<string, string>();
            string req = Encoding.Default.GetString(serializedData);
            string[] keyValuePairs = req.Split('&');
            for(int i = 0; i < keyValuePairs.Length; i++)
            {
                string[] keyValuePair = keyValuePairs[i].Split('=');
                if (keyValuePair.Length == 2)
                {
                    recievedData.Add(keyValuePair[0], keyValuePair[1]);
                }
            }
            T deserializedObject = CreateSerializedObjectInstance();
            FillMemberValues(deserializedObject, recievedData);
            return deserializedObject;
        }
    }
}
