﻿namespace RodSoft.OSM.Tracking
{
    public class TelemetrySettings
    {
        public bool Enabled;
        public string ServerAddress;
        public int TransmittingPeriod;
        public int RequestTimeout;
        public string CashFolder;
        public int MaximumItemsPerCashFile;
        public int MaximumSecondsPerCashFile;
    }
}
