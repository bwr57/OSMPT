using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo.WindowsPresentation.Tracking
{
    public class TelemetrySettings
    {
        public string ServerAddress;
        public int TransmittingPeriod;
        public int RequestTimeout;
        public string CashFolder;
        public int MaximumItemsPerCashFile;
        public int MaximumSecondsPerCashFile;
    }
}
