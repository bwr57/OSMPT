using Microsoft.Win32;
using RodSoft.Core.Configuration;

namespace RodSoft.Core.UI.Speech
{
    public class SpeechSynthesizerSettingsRegistryOperation : RegistryOperationBase<SpeechSynthesizerSettings>
    {
        /// <summary>
        /// Ключ реестра для хранения пути сохранения журналов измерений
        /// </summary>
        public const string SPEECH_SYNTHESIZER_SETTING_FOLDER_NAME = "SpeechSynthesizerSettings";

        /// <summary>
        /// Конструктор объекта операции чтения и сохранения путей сохранения протоколов испытаний и  журналов измерений
        /// </summary>
        /// <param name="registryFolderName">Папка реестра для хранения путей сохранения протоколов испытаний и  журналов измерений</param>
        public SpeechSynthesizerSettingsRegistryOperation(string registryFolderName)
            : base(registryFolderName)
        { }

        public override SpeechSynthesizerSettings LoadSettings(SpeechSynthesizerSettings settings)
        {
            string registryFolderName = GetFullRegistryFolderName();
            if (settings == null)
            {
                settings = new SpeechSynthesizerSettings();
            }
            settings.SpeechWarningMode = (SpeechWarningModes)Registry.GetValue(registryFolderName, SPEECH_SYNTHESIZER_SETTING_FOLDER_NAME, settings.SpeechWarningMode);
            return settings;
        }

        public override void SaveSettings(SpeechSynthesizerSettings settings)
        {
            string registryFolderName = GetFullRegistryFolderName();
            Registry.SetValue(registryFolderName, SPEECH_SYNTHESIZER_SETTING_FOLDER_NAME, settings.SpeechWarningMode, RegistryValueKind.DWord);
        }
    }
}
