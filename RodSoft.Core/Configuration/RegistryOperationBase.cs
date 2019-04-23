using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodSoft.Core.Configuration
{
    public class RegistryOperationBase<TSettings> : RegistryOperation where TSettings : class
    {
        /// <summary>
        /// Конструктор объекта операции чтения и сохранения настроек в реестре
        /// </summary>
        /// <param name="registryFolderName">Папка реестра для хранения настроек</param>
        public RegistryOperationBase(string registryFolderName)
            : base(registryFolderName)
        { }


        public override object Create()
        {
            return Activator.CreateInstance(typeof(TSettings));
        }

        public virtual TSettings CreateSettings()
        {
            return  Create() as TSettings;
        }

        /// <summary>
        /// Чтение настроек из реестра
        /// </summary>
        /// <param name="settings">Настройки</param>
        /// <returns>Настройки, измененные за счет считывания данных из реестра</returns>
        public virtual TSettings LoadSettings(TSettings settings)
        {
            if(settings == null)
            {
                return CreateSettings();
            }
            return settings;
        }

        /// <summary>
        /// Сохранение настроек в реестре
        /// </summary>
        /// <param name="settings">Настройки</param>
        public virtual void SaveSettings(TSettings settings)
        { }

    }
}
