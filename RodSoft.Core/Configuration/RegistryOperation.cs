using Microsoft.Win32;
using System;
using System.Reflection;

namespace Rodsoft.Core.RegistryOperations
{
    /// <summary>
    /// Базовый класс для сохранения и чтения настроек в реестре
    /// </summary>
    /// <typeparam name="TSettings">Класс настроек</typeparam>
    public class RegistryOperation
    {
        /// <summary>
        /// Раздел реестра для хранения настроек (личный раздел настроек пользователя)
        /// </summary>
        public const string REGISTRY_ROOT = "HKEY_CURRENT_USER\\SOFTWARE";

        /// <summary>
        /// Папка реестра для хранения настроек
        /// </summary>
        public string RegistryFolderName { get; set; }

        /// <summary>
        /// Получение полного пути к папке настроек
        /// </summary>
        /// <returns>Полный путь к папке настроек</returns>
        public virtual string GetFullRegistryFolderName()
        {
            return REGISTRY_ROOT + "\\" + RegistryFolderName;
        }

        /// <summary>
        /// Конструктор объекта операции чтения и сохранения настроек в реестре
        /// </summary>
        /// <param name="registryFolderName">Папка реестра для хранения настроек</param>
        public RegistryOperation(string registryFolderName)
        {
            this.RegistryFolderName = registryFolderName;
        }

        /// <summary>
        /// Метод чтения 16-разрядного целого значения, сохраненного в реестре
        /// </summary>
        /// <param name="registryFolderName">Наименование папки реестра, хранящей настройки</param>
        /// <param name="valueName">Название параметра реестра</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Считанное из реестра значение или значение по умолчанию, если параметр отсутствует в реестре</returns>
        public static ushort LoadUShortRegistryValue(string registryFolderName, string valueName, int defaultValue)
        {
            object value = Registry.GetValue(registryFolderName, valueName, defaultValue);
            if (value == null)
                return (ushort)defaultValue;
            return Convert.ToUInt16(value);
        }

        /// <summary>
        /// Метод чтения 16-разрядного целого значения, сохраненного в реестре
        /// </summary>
        /// <param name="registryFolderName">Наименование папки реестра, хранящей настройки</param>
        /// <param name="valueName">Название параметра реестра</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Считанное из реестра значение или значение по умолчанию, если параметр отсутствует в реестре</returns>
        public static byte LoadByteRegistryValue(string registryFolderName, string valueName, int defaultValue)
        {
            object value = Registry.GetValue(registryFolderName, valueName, defaultValue);
            if (value == null)
                return (byte)defaultValue;
            return Convert.ToByte(value);
        }


        public virtual void SaveSettingsAuto(string registryFolderName, object settings)
        {
            Type settingsClass = settings.GetType();
            foreach (FieldInfo field in settingsClass.GetFields())
            {
                switch (field.FieldType.Name)
                {
                    case "String":
                        {
                            Registry.SetValue(registryFolderName, field.Name, field.GetValue(settings), RegistryValueKind.String);
                            break;
                        }
                    case "Int32":
                        {
                            Registry.SetValue(registryFolderName, field.Name, field.GetValue(settings), RegistryValueKind.DWord);
                            break;
                        }
                    case "Double":
                        {
                            Registry.SetValue(registryFolderName, field.Name, field.GetValue(settings).ToString(), RegistryValueKind.String);
                            break;
                        }
                    case "Boolean":
                        {
                            bool value = (bool)field.GetValue(settings);
                            Registry.SetValue(registryFolderName, field.Name, value ? 1 : 0, RegistryValueKind.DWord);
                            break;
                        }
                    default:
                        {
                            if (field.FieldType.IsClass)
                            {
                                SaveSettingsAuto(registryFolderName + "\\" + field.Name, field.GetValue(settings));
                            }
                            break;
                        }
                }
            }
            foreach (PropertyInfo property in settingsClass.GetProperties())
            {
                switch (property.PropertyType.Name)
                {
                    case "String":
                        {
                            Registry.SetValue(registryFolderName, property.Name, property.GetValue(settings, null), RegistryValueKind.String);
                            break;
                        }
                    case "Int32":
                        {
                            Registry.SetValue(registryFolderName, property.Name, property.GetValue(settings, null), RegistryValueKind.DWord);
                            break;
                        }
                    case "Double":
                        {
                            Registry.SetValue(registryFolderName, property.Name, property.GetValue(settings, null).ToString(), RegistryValueKind.String);
                            break;
                        }
                    case "Boolean":
                        {
                            bool value = (bool)property.GetValue(settings);
                            Registry.SetValue(registryFolderName, property.Name, value ? 1 : 0, RegistryValueKind.DWord);
                            break;
                        }
                    default:
                        {
                            if (property.PropertyType.IsClass)
                            {
                                SaveSettingsAuto(registryFolderName + "\\" + property.Name, property.GetValue(settings, null));
                            }
                            break;
                        }
                }
            }
        }
        

        public virtual void SaveSettingsAuto(object settings)
        {
            string registryFolderName = GetFullRegistryFolderName();
            SaveSettingsAuto(registryFolderName, settings);
        }

        public virtual void LoadSettingsAuto(string registryFolderName, object settings)
        {
            Type settingsClass = settings.GetType();
            foreach (FieldInfo field in settingsClass.GetFields())
            {
                switch (field.FieldType.Name)
                {
                    case "String":
                        {
                            field.SetValue(settings, Registry.GetValue(registryFolderName, field.Name, null));
                            break;
                        }
                    case "Int32":
                        {
                            field.SetValue(settings, Registry.GetValue(registryFolderName, field.Name, 0));
                            break;
                        }
                    case "Double":
                        {
                            double parameterValue = 0;
                            if (Double.TryParse((string)Registry.GetValue(registryFolderName, field.Name, "0"), out parameterValue))
                                field.SetValue(settings, parameterValue);
                            break;
                        }
                    case "Boolean":
                        {
                            bool value = ((int)Registry.GetValue(registryFolderName, field.Name, 0) > 0);
                            field.SetValue(settings, value);
                            break;
                        }
                    default:
                        {
                            if (field.FieldType.IsClass)
                            {
                                object subSettings = Activator.CreateInstance(field.FieldType);
                                LoadSettingsAuto(registryFolderName + "\\" + field.Name, subSettings);
                                field.SetValue(settings, subSettings);
                            }
                            break;
                        }

                }
             }
            foreach (PropertyInfo property in settingsClass.GetProperties())
            {
                switch (property.PropertyType.Name)
                {
                    case "String":
                        {
                            property.SetValue(settings, Registry.GetValue(registryFolderName, property.Name, null), null);
                            break;
                        }
                    case "Int32":
                        {
                            property.SetValue(settings, Registry.GetValue(registryFolderName, property.Name, 0), null);
                            break;
                        }
                    case "Double":
                        {
                            double parameterValue = 0;
                            if (Double.TryParse((string)Registry.GetValue(registryFolderName, property.Name, "0"), out parameterValue))
                                property.SetValue(settings, parameterValue, null);
                            break;
                        }
                    case "Boolean":
                        {
                            bool value = ((int)Registry.GetValue(registryFolderName, property.Name, 0) > 0);
                            property.SetValue(settings, value, null);
                            break;
                        }
                    default:
                        {
                            if (property.PropertyType.IsClass)
                            {
                                object subSettings = Activator.CreateInstance(property.PropertyType);
                                LoadSettingsAuto(registryFolderName + "\\" + property.Name, subSettings);
                                property.SetValue(settings, subSettings, null);
                            }
                            break;
                        }
                }
             }

        }

        public virtual void LoadSettingsAuto(object settings)
        {
            string registryFolderName = GetFullRegistryFolderName();
            LoadSettingsAuto(registryFolderName, settings);
        }
    }
}