using System;
using System.Reflection;

namespace RodSoft.Core.Communications
{
    public class TransmittedAttribute : Attribute
    {
        public int SectionIndex { get; set; }
        public int PropertyIndex { get; set; }
        public string FormatString { get; set; }
        public string MessageIdentificator { get; set; }
        public bool WriteType { get; set; }
    }

    [Serializable]
    public class MessageBase
    {
        public DateTime Time { get; set; } = DateTime.Now;

        public MessageBase()
        { }

        public MessageBase(object source)
        {
            Assign(source);
        }

        public static void CopyMemberValues(object source, object target, bool isAnyMember)
        {
            foreach (FieldInfo field in source.GetType().GetFields())
            {
                if (isAnyMember || field.GetCustomAttribute(typeof(TransmittedAttribute), true) != null)
                {
                    AssignMemberValue(target, field.Name, field.GetValue(source), isAnyMember);
                }
            }
            foreach (PropertyInfo property in source.GetType().GetProperties())
            {
                if (isAnyMember || property.GetCustomAttribute(typeof(TransmittedAttribute), true) != null)
                {
                    AssignMemberValue(target, property.Name, property.GetValue(source), isAnyMember);
                }
            }
        }

        public static void CopyMemberValues(object source, object target)
        {
            CopyMemberValues(source, target, true);
        }

        public static void AssignMemberValue(object target, string memberName, object value, bool isAnyMember)
        {
            FieldInfo targetField = target.GetType().GetField(memberName);
            if (targetField != null)
            {
                if (isAnyMember || targetField.GetCustomAttribute(typeof(TransmittedAttribute), true) != null)
                {
                    targetField.SetValue(target, value);
                }
            }
            else
            {
                PropertyInfo targetProperty = target.GetType().GetProperty(memberName);
                if (targetProperty != null && (isAnyMember || targetProperty.GetCustomAttribute(typeof(TransmittedAttribute), true) != null))
                {
                    targetProperty.SetValue(target, value);
                }
            }
        }

        public static void AssignMemberValue(object target, string memberName, object value)
        {
            AssignMemberValue(target, memberName, value, true);
        }

        public virtual void Assign(object source)
        {
            CopyMemberValues(source, this);
        }

        public virtual void AssignTo(object target)
        {
            CopyMemberValues(this, target);
        }



    }
}


