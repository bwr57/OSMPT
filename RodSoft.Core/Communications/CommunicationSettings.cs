using RodSoft.Core.Configuration;

namespace RodSoft.Core.Communications
{
    public class CommunicationSettings : ServiceSettings
    {
        public string VehicleNumber;
        public int ServiceType;
        public string ServerAddress;
        public int TransmittingPeriod;
        public int RequestTimeout;
        public string CashFolder;
        public int MaximumItemsPerCashFile;
        public int MaximumSecondsPerCashFile;
        public int CommucationLogMode;
        public string CommucationLogFileName;
    }
}
