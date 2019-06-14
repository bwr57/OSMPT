namespace RodSoft.Core.UI.Speech
{
    public enum SpeechWarningModes
    {
        Disabled = 0,
        Errors = 1,
        ErrorsAndWarnings = 2,
        All = 3
    }

    public class SpeechSynthesizerSettings
    {
        public SpeechWarningModes SpeechWarningMode;
    }
}
