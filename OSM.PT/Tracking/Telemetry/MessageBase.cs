using System;
using System.Reflection;

namespace RodSoft.Communications
{
    [Serializable]
    public class MessageBase
    {
        public DateTime Time { get; set; } = DateTime.Now;

        public MessageBase()
        { }

        public MessageBase(object source)
        {
            Copy(source);
        }

        public virtual void Copy(object hdsData)
        {
            foreach (FieldInfo field in this.GetType().GetFields())
            {
                FieldInfo sourceField = hdsData.GetType().GetField(field.Name);
                if (sourceField != null)
                {
                    field.SetValue(this, sourceField.GetValue(hdsData));
                }
                else
                {
                    PropertyInfo sourceProperty = hdsData.GetType().GetProperty(field.Name);
                    if (sourceProperty != null)
                    {
                        field.SetValue(this, sourceProperty.GetValue(hdsData, null));
                    }
                }
            }
        }

    }
}
