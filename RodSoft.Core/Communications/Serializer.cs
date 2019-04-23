using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RodSoft.Core.Communications
{
    public class Serializer<T> where T : class
    {
        public class DataMemberInfo
        {
            public string Name;
            public MemberInfo MemberInfo;
            public TransmittedAttribute AdditionalProperties;
        }

        protected IList<IList<DataMemberInfo>> _SectionList;

        public string MessageIdentificatorPrefix { get; set; }
        public string GeneralMessageIdentificatorPrefix { get; set; }


        protected BinaryFormatter _BinaryFormatter = new BinaryFormatter();

        public bool IsAutoCreateEmbededObjects { get; set; } = true;

        protected Type _SerializedObjectType = null;

        protected MemberInfo[] _SerializedObjectMembers = null;

        public static object GetMemberValue(object source, MemberInfo member)
        {
            object value = 0;
            value = GetMemberValue(source, member, value);
            if (value is bool)
                value = ((bool)value) ? 1 : 0;
            return value;
        }

        public static object GetMemberValue(object source, MemberInfo member, object value)
        {
            if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).GetValue(source);
            else
            if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).GetValue(source);
            return value;
        }

        protected virtual MemberInfo[] CreateMembersList(Type serializedObjectType)
        {
            if(!serializedObjectType.Equals(_SerializedObjectType))
            {
                _SerializedObjectMembers = serializedObjectType.GetMembers().Where(member => member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property).OrderBy(member => member.Name).ToArray();
                _SerializedObjectType = serializedObjectType;
            }
            return _SerializedObjectMembers;
        }

        public virtual void SerializeObject(Stream stream, T obj)
        {
            _BinaryFormatter.Serialize(stream, obj);
        }

        public virtual T DeserializeObject(Stream stream)
        {
            return (T)_BinaryFormatter.Deserialize(stream);
        }

        public virtual byte[] SerializeBinary(object message)
        {
            IList<byte[]> serializedFields = new List<byte[]>();
            int bytesCount = 0;
            MemberInfo[] fields = CreateMembersList(message.GetType());
            for (int i = 0; i < fields.Length; i++)
            {
                MemberInfo member = fields[i];
                if (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property)
                {
                    byte[] serializedField = SerializeField(GetMemberValue(message, member, null), member);
                    if (serializedField != null)
                    {
                        serializedFields.Add(serializedField);
                        bytesCount += serializedField.Length;
                    }
                }
            }
            byte[] serializedObject = new byte[bytesCount];
            int currentPosition = 0;
            for (int i = 0; i < serializedFields.Count; i++)
            {
                serializedFields[i].CopyTo(serializedObject, currentPosition);
                currentPosition += serializedFields[i].Length;
            }
            return serializedObject;
        }

        public virtual byte[] SerializeField(object fieldValue, MemberInfo memberInfo)
        {
            if (!fieldValue.GetType().IsValueType && fieldValue == null)
                return new byte[1] { 0 };
            if (fieldValue is string)
            {
                byte[] serializedString = System.Text.Encoding.Unicode.GetBytes((string)fieldValue);
                byte[] serializedString1 = new byte[serializedString.Length + 5];
                serializedString1[0] = 1;
                BitConverter.GetBytes(serializedString.Length).CopyTo(serializedString1, 1);
                serializedString.CopyTo(serializedString1, 5);
                return serializedString1;
            }
            if (fieldValue.GetType().IsClass)
            {
                byte[] serializedObject = SerializeBinary(fieldValue);
                byte[] serializedTypeName = System.Text.Encoding.Unicode.GetBytes(fieldValue.GetType().Namespace + "." + fieldValue.GetType().Name);
                byte[] serializedObject1 = new byte[serializedObject.Length + serializedTypeName.Length + 1];
                serializedObject[0] = 1;
                serializedTypeName.CopyTo(serializedObject1, 1);
                serializedObject.CopyTo(serializedObject1, serializedTypeName.Length + 1);
                return serializedObject1;
            }
            if (fieldValue is byte)
                return new byte[1] { (byte)fieldValue };
            if (fieldValue is short)
                return BitConverter.GetBytes((short)fieldValue);
            if (fieldValue is int)
                return BitConverter.GetBytes((int)fieldValue);
            if (fieldValue is uint)
                return BitConverter.GetBytes((uint)fieldValue);
            if (fieldValue is bool)
                return BitConverter.GetBytes((bool)fieldValue);
            if (fieldValue is Single)
                return BitConverter.GetBytes((Single)fieldValue);
            if (fieldValue is double)
                return BitConverter.GetBytes((double)fieldValue);
            if (fieldValue is DateTime)
                return BitConverter.GetBytes(((DateTime)fieldValue).Ticks);
            return null;
        }

        public virtual void WriteFieldValueToBinaryStream(Stream stream, object fieldValue, MemberInfo memberInfo)
        {
            Type serializedObjectType = null;
            if (memberInfo is FieldInfo)
            {
                FieldInfo field = (FieldInfo)memberInfo;
                serializedObjectType = field.FieldType;
            }
            else
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo property = (PropertyInfo)memberInfo;
                serializedObjectType = property.PropertyType;
            }
            if (!serializedObjectType.IsValueType)
            {
                //    if (!serializedObjectType.IsClass || (serializedObjectType.IsClass && IsAutoCreateEmbededObjects))
                //    {
                //        if (fieldValue == null)
                //        {
                //            stream.WriteByte(0);
                //            return;
                //        }
                //        else
                //        {
                //            stream.WriteByte(1);
                //        }
                //    }
                //}
                if (serializedObjectType.FullName == "System.String")
                {
                    WriteString(stream, (string)fieldValue);
                    return;
                    //byte[] serializedString1 = new byte[serializedString.Length + 4];
                    //BitConverter.GetBytes(serializedString.Length).CopyTo(serializedString1, 0);
                    //serializedString.CopyTo(serializedString1, 4);
                    //return serializedString1;
                }
                if (serializedObjectType.IsClass)
                {
                    if (!IsAutoCreateEmbededObjects)
                    {
                        CustomWriteObject(stream, fieldValue);
                        return;
                    }
                    stream.WriteByte((byte)(fieldValue == null ? 0 : 1));
                    WriteToBinaryStream(stream, fieldValue, true);
                    return;
                    //byte[] serializedTypeName = System.Text.Encoding.Unicode.GetBytes(fieldValue.GetType().FullName);
                    //byte[] serializedObject1 = new byte[serializedObject.Length + serializedTypeName.Length + 1];
                    //serializedObject[0] = 1;
                    //serializedTypeName.CopyTo(serializedObject1, 1);
                    //serializedObject.CopyTo(serializedObject1, serializedTypeName.Length + 1);
                    //return serializedObject1;
                }
            }
            if (fieldValue is byte)
            {
                stream.WriteByte((byte)fieldValue);
                return;
            }
            if (fieldValue is short)
            {
                stream.Write(BitConverter.GetBytes((short)fieldValue), 0, 2);
                return;
            }
            if (fieldValue is int)
            {
                stream.Write(BitConverter.GetBytes((int)fieldValue), 0, 4);
                return;
            }
            if (fieldValue is uint)
            {
                stream.Write(BitConverter.GetBytes((uint)fieldValue), 0, 4);
                return;
            }
            if (fieldValue is bool)
            {
                stream.Write(BitConverter.GetBytes((bool)fieldValue), 0, 1);
                return;
            }
            if (fieldValue is Single)
            {
                stream.Write(BitConverter.GetBytes((Single)fieldValue), 0, 4);
                return;
            }
            if (fieldValue is double)
            {
                stream.Write(BitConverter.GetBytes((double)fieldValue), 0, 8);
                return;
            }
            if (fieldValue is DateTime)
            {
                stream.Write(BitConverter.GetBytes(((DateTime)fieldValue).Ticks), 0, 8);
                return;
            }
        }

        public virtual void WriteToBinaryStream(Stream stream, object fieldValue, bool isWriteClassName)
        {
            if (isWriteClassName)
            {
                WriteString(stream, fieldValue.GetType().Assembly.FullName);
                WriteString(stream, fieldValue.GetType().FullName);
            }
            MemberInfo[] fields = CreateMembersList(fieldValue.GetType());
            for (int i = 0; i < fields.Length; i++)
            {
                MemberInfo member = fields[i];
                if (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property)
                {
                    WriteFieldValueToBinaryStream(stream, GetMemberValue(fieldValue, member, null), member);
                }
            }
        }

        public virtual void WriteString(Stream stream, string value)
        {
            if(value == null)
            {
                stream.WriteByte(0);
                return;
            }
            stream.WriteByte(1);
            byte[] serializedString = Encoding.Unicode.GetBytes(value);
            stream.Write(BitConverter.GetBytes(serializedString.Length), 0, 4);
            stream.Write(serializedString, 0, serializedString.Length);
        }

        public static int ReadInt32(Stream stream)
        {
            byte[] serializedObject = new byte[4];
            stream.Read(serializedObject, 0, 4);
            int result = 0;
            result = BitConverter.ToInt32(serializedObject, 0);
            return result;
        }

        public virtual object DeserializeObject(byte[] serializedObject, ref int currentIndex)
        {
            string memberClassAssemblyName = DeserializeString(serializedObject, ref currentIndex);
            string memberClassName = DeserializeString(serializedObject, ref currentIndex);
            Assembly assembly = System.Reflection.Assembly.Load(memberClassAssemblyName);
            object deserializedObject = assembly.CreateInstance(memberClassName);// Activator.CreateInstance(memberClassAssemblyName, memberClassName);
            return DeserializeObject(serializedObject, ref currentIndex, deserializedObject);
        }

        public virtual object DeserializeObject(byte[] serializedObject, ref int currentIndex, object deserializedObject)
        {
            MemberInfo[] fields = CreateMembersList(deserializedObject.GetType());
            for (int i = 0; i < fields.Length; i++)
            {
                MemberInfo member = fields[i];
                if (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property)
                {
                    object deserializedField = DeserializeField(serializedObject, ref currentIndex, member);
                    if (deserializedField != null)
                    {
                        if (member is FieldInfo)
                        {
                            ((FieldInfo)member).SetValue(deserializedObject, deserializedField);
                        }
                        else
                        {
                            ((PropertyInfo)member).SetValue(deserializedObject, deserializedField);
                        }
                    }
                }
            }
            return deserializedObject;
        }

        public virtual object DeserializeField(byte[] serializedObject, ref int currentIndex, MemberInfo memberInfo)
        {
            string memberTypeName = null;
            object deserializedObject = null;
            Type deserializedObjectType = null;
            if (memberInfo is FieldInfo)
            {
                FieldInfo field = (FieldInfo)memberInfo;
                deserializedObjectType = field.FieldType;
            }
            else
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo property = (PropertyInfo)memberInfo;
                deserializedObjectType = property.PropertyType;
            }
            if (deserializedObjectType != null)
            {
//                Debug.WriteLine(memberInfo.Name + " " + currentIndex.ToString());
                memberTypeName = deserializedObjectType.FullName;
                if (deserializedObjectType.IsValueType)
                {
                    if (memberTypeName == "System.Byte")
                        return serializedObject[currentIndex++];
                    if (memberTypeName == "System.Int16")
                    {
                        short result = BitConverter.ToInt16(serializedObject, currentIndex);
                        currentIndex += 2;
                        return result;
                    }
                    if (memberTypeName == "System.Int32")
                    {
                        int result = BitConverter.ToInt32(serializedObject, currentIndex);
                        currentIndex += 4;
                        return result;
                    }
                    if (memberTypeName == "System.UInt32")
                    {
                        uint result = BitConverter.ToUInt32(serializedObject, currentIndex);
                        currentIndex += 4;
                        return result;
                    }
                    if (memberTypeName == "System.Boolean")
                        return DeserializeBoolean(serializedObject, ref currentIndex);
                    if (memberTypeName == "System.Single")
                    {
                        Single result = BitConverter.ToSingle(serializedObject, currentIndex);
                        currentIndex += 4;
                        return result;
                    }
                    if (memberTypeName == "System.Double")
                    {
                        double result = BitConverter.ToDouble(serializedObject, currentIndex);
                        currentIndex += 8;
                        return result;
                    }
                    if (memberTypeName == "System.DateTime")
                    {
                        long ticks = BitConverter.ToInt64(serializedObject, currentIndex);
                        currentIndex += 8;
                        return new DateTime(ticks);
                    }
                }
                else
                {
                    if (deserializedObjectType.FullName == "System.String")
                    {
                        if (serializedObject[currentIndex++] > 0)
                            deserializedObject = DeserializeString(serializedObject, ref currentIndex);
                    }
                    else
                    if (deserializedObjectType.IsClass)
                    {
                        if (!IsAutoCreateEmbededObjects)
                        {
                            return CustomDeserializeObject(serializedObject, ref currentIndex, memberInfo);
                        }
                        if (DeserializeBoolean(serializedObject, ref currentIndex))
                            deserializedObject = DeserializeObject(serializedObject, ref currentIndex);
                    }
                }
                return deserializedObject;
            }
            return null;
        }

        private static string DeserializeString(byte[] serializedObject, ref int currentIndex)
        {
            int stringLenght = BitConverter.ToInt32(serializedObject, currentIndex);
            currentIndex += 4;
            if (stringLenght > 0)
            {
                string deserializedString = System.Text.Encoding.Unicode.GetString(serializedObject, currentIndex, stringLenght);
                currentIndex += stringLenght;
                return deserializedString;
            }
            return null;
        }
        private static bool DeserializeBoolean(byte[] serializedObject, ref int currentIndex)
        {
            return BitConverter.ToBoolean(serializedObject, currentIndex++);
        }

        protected virtual void CustomWriteObject(Stream stream, object embededObject)
        {
            return;
        }


        protected virtual object CustomDeserializeObject(byte[] serializedObject, ref int currentIndex, MemberInfo member)
        {
            return null;
        }

        public virtual T CreateSerializedObjectInstance()
        {
            return (T)typeof(T).Assembly.CreateInstance(typeof(T).FullName);
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
            request = ConcatRequest(request, messageFieldIdentificator + "=" + String.Format(CultureInfo.GetCultureInfo("en-US"), formatString, GetMemberValue(message, member)));
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


    }
}
