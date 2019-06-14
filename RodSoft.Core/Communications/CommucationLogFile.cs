using System;
using System.IO;

namespace RodSoft.Core.Communications
{
    public class CommucationLogFile<T> where T: MessageBase
    {
        public int CommucationLogMode;
        public string CommucationLogFileName = "communication log.txt";

        public CommucationLogFile()
        { }

        public CommucationLogFile(int communicationLogMode, string communicationLogFileName)
        {
            CommucationLogMode = communicationLogMode;
            CommucationLogFileName = communicationLogFileName;
        }

        public void WriteRecord(CashedMessage<T> cashedMessage, string response)
        {
            try
            {
                if (CommucationLogMode > 0 && !String.IsNullOrEmpty(CommucationLogFileName))
                    File.AppendAllLines(CommucationLogFileName, new string[] { String.Format("{0} {1}\t{2}\t{3}\t{4}\t{5}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), cashedMessage.FileName, cashedMessage.Index, cashedMessage.Message.Time.ToString(), response) });
            }
            catch
            { }
        }

        public void WriteDamagedFileInfo(string recordString)
        {
            try
            {
                if (CommucationLogMode > 0 && !String.IsNullOrEmpty(CommucationLogFileName))
                    File.AppendAllLines(CommucationLogFileName, new string[] { String.Format("{0} {1}. {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), recordString) });
            }
            catch
            { }
        }

    }
}
