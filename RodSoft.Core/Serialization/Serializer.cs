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

namespace RodSoft.Core.Serialization
{
    public abstract class Serializer<T> where T : class
    {
        protected class MemberInfoEx
        {
            public MemberInfo Member;
         //   public Trans
        }
  //      protected Dictionary<string, >
        public Type SerializedObjectClass
        {
            get { return _SerializedObjectType; }
            set { _SerializedObjectType = value; }
        }

        public bool IsAutoCreateEmbededObjects { get; set; } = true;

        protected Type _SerializedObjectType;

//        protected MemberInfo[] _SerializedObjectMembers = null;

        public virtual byte[] Serialize(object obj)
        {
            if (obj == null)
                return new byte[0];
            byte[] serializedObject = new byte[0];
            using (Stream stream = new MemoryStream())
            {
                Serialize(stream, obj);
                stream.Position = 0;
                serializedObject = new byte[stream.Length];
                stream.Read(serializedObject, 0, serializedObject.Length);
                stream.Dispose();
            }
            return serializedObject;
        }

        public abstract void Serialize(Stream stream, object obj);
/*
 *      public virtual void Serialize(Stream stream, object obj)
        {
            if(stream != null)
            {
                byte[] serializedObject = Serialize(obj);
                if (serializedObject != null && serializedObject.Length > 0)
                    stream.Write(serializedObject, 0, serializedObject.Length);
            }
        }

    */

        public abstract T Deserialize(byte[] serializedData);

        public virtual T Deserialize(Stream stream)
        {
            if (stream == null)
                return null;
            byte[] serializedObject = new byte[stream.Length - stream.Position];
            stream.Read(serializedObject, 0, serializedObject.Length);
            return Deserialize(serializedObject);
        }

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

        public static void SetMemberValue(object target, MemberInfo member, object value)
        {
            try
            {
                if (member.MemberType == MemberTypes.Field)
                    ((FieldInfo)member).SetValue(target, value);
                else
                if (member.MemberType == MemberTypes.Property)
                    ((PropertyInfo)member).SetValue(target, value);
            }
            catch { }
        }

        public static Type GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo)
            {
                FieldInfo field = (FieldInfo)memberInfo;
                return field.FieldType;
            }
            else
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo property = (PropertyInfo)memberInfo;
                return property.PropertyType;
            }
            return null;
        }

        protected static IDictionary<Type, MemberInfo[]> _Members = new Dictionary<Type, MemberInfo[]>();
        protected virtual MemberInfo[] CreateMembersList(Type serializedObjectType)
        {
            MemberInfo[] members = null;
            if (_Members.ContainsKey(serializedObjectType))
            {
                members = _Members[serializedObjectType];
            }
            else
            {
                members = serializedObjectType.GetMembers().Where(member => member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property).OrderBy(member => member.Name).ToArray();
                _Members.Add(serializedObjectType, members);
            }
            //if (!serializedObjectType.Equals(_SerializedObjectType))
            //{
            //    _SerializedObjectType = serializedObjectType;
            //}
            return members;
        }

        public virtual object CreateSerializedObjectInstance(Type type)
        {
            return type.Assembly.CreateInstance(type.FullName);
        }

        public virtual T CreateSerializedObjectInstance()
        {
            if(_SerializedObjectType != null)
                return (T) CreateSerializedObjectInstance(_SerializedObjectType);

            return (T)CreateSerializedObjectInstance(typeof(T));
        }

    }
}
