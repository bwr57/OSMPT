using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace RodSoft.Core.Communications
{
    public class MessageSerializatorBase<T> where T : CashedMessage
    {

        protected BinaryFormatter _BinaryFormatter = new BinaryFormatter();

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
                    IList<MemberInfo> messageMembers = new List<MemberInfo>();
//                    SortedDictionary<int, MemberInfo> messageMembers = new SortedDictionary<int, MemberInfo>();
                    foreach (MemberInfo member in messageType.GetMembers())
                    {
                        TransmittedAttribute assignableAttribute = (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true);
                        if (assignableAttribute != null)
                            messageMembers.Add(member);
 //                           messageMembers.Add(assignableAttribute.SectionIndex, member);
                    }
                    messageMembers = messageMembers.OrderBy(memberInfo => ((TransmittedAttribute)memberInfo.GetCustomAttribute(typeof(TransmittedAttribute), true)).SectionIndex).ToList();
                    int sectionIndex = 0;
                    IList<MemberInfo> sectionMembers = new List<MemberInfo>();
//                    SortedDictionary<int, MemberInfo> sectionMembers = new SortedDictionary<int, MemberInfo>();
                    foreach (MemberInfo member in messageMembers)
                    {
                        TransmittedAttribute assignableAttribute = (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true);
                        if (assignableAttribute.SectionIndex != sectionIndex)
                        {
                            sectionMembers = messageMembers.OrderBy(memberInfo => ((TransmittedAttribute)memberInfo.GetCustomAttribute(typeof(TransmittedAttribute), true)).PropertyIndex).ToList();
                            foreach (MemberInfo sectionMemberInfo in sectionMembers)
                            {
                                AddValueToCollection(message, nameValueCollection, sectionMemberInfo);
                            }
                            sectionMembers.Clear();
                            sectionIndex = assignableAttribute.SectionIndex;
                        }
                        sectionMembers.Add(member);
                    }
                    sectionMembers = messageMembers.OrderBy(memberInfo => ((TransmittedAttribute)memberInfo.GetCustomAttribute(typeof(TransmittedAttribute), true)).PropertyIndex).ToList();
                    foreach (MemberInfo sectionMemberInfo in sectionMembers)
                    {
                        AddValueToCollection(message, nameValueCollection, sectionMemberInfo);
                    }
                }
            }
            return nameValueCollection;
        }

        public virtual void SerializeObject(Stream stream, T obj)
        {
            _BinaryFormatter.Serialize(stream, obj);
        }

        public virtual T DeserializeObject(Stream stream)
        {
            return (T)_BinaryFormatter.Deserialize(stream);
        }

        private static void AddValueToCollection(T message, NameValueCollection nameValueCollection, MemberInfo member)
        {
            string formatString = "{0}";
            TransmittedAttribute assignableAttribute = (TransmittedAttribute)member.GetCustomAttribute(typeof(TransmittedAttribute), true);
            if (assignableAttribute != null && assignableAttribute.FormatString != null)
                formatString = "{0:" + assignableAttribute.FormatString + "}";
            if (member.MemberType == MemberTypes.Field)
                nameValueCollection.Add(member.Name, String.Format(formatString, ((FieldInfo)member).GetValue(message)));
            else
            if (member.MemberType == MemberTypes.Property)
                nameValueCollection.Add(member.Name, String.Format(formatString, ((PropertyInfo)member).GetValue(message)));
        }
    }
}
