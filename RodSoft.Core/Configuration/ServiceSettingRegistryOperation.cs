namespace RodSoft.Core.Configuration
{
    public class ServiceSettingRegistryOperation : RegistryOperationBase<ServiceSettings>
    {
        private const string ENABLED_KEY = "Enabled";

        public ServiceSettingRegistryOperation(string registryFolderName)
            : base(registryFolderName)
        { }

        public override ServiceSettings LoadSettings(ServiceSettings settings)
        {
            if (settings == null)
                settings = base.LoadSettings(settings);
            try
            {
                settings.Enabled = LoadBoolean(GetFullRegistryFolderName(), ENABLED_KEY);
            }
            catch { }
            return settings;
        }

        public override void SaveSettings(ServiceSettings settings)
        {
            base.SaveSettings(settings);
            string registryFolderFullName = GetFullRegistryFolderName();
            try
            {
                SaveBoolean(registryFolderFullName, ENABLED_KEY, settings.Enabled);
            }
            catch { }
        }
    }
}
