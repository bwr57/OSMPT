using Demo.WindowsPresentation.Tracking.Telemetry.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Demo.WindowsPresentation.Tracking.Telemetry
{
    public class CashService
    {
        protected TrackMessageSerializator _TrackMessageSerializator = new TrackMessageSerializator();

        public string CashFolder { get; set; }

        protected Stream _DataStream;

        protected Stream _IndexSteam;

        protected int _Index = 0;

        protected DateTime _StartTime = DateTime.Now;

        protected string _FileName;

        public virtual int Save(TrackMessage trackMessage)
        {
            DateTime time = DateTime.Now;
            if (time.Subtract(_StartTime).TotalSeconds > 1000 || _Index == 1000)
            {
                _DataStream.Close();
                _DataStream.Dispose();
                _DataStream = null;
                _IndexSteam.Close();
                _IndexSteam.Dispose();
                _IndexSteam = null;
            }
            if (_DataStream == null)
            {
                _FileName = String.Format("{0} {1}", time.ToShortDateString().Replace('.', '_'), time.ToShortTimeString().Replace(':', '_'));
                string fileName = String.Format("{0}\\{1}", CashFolder, _FileName); 
                _DataStream = File.Open(fileName + ".dat", FileMode.OpenOrCreate);
                _IndexSteam = File.Open(fileName + ".idx", FileMode.OpenOrCreate);
                _Index = 0;
            }
            trackMessage.Index = _Index++;
            trackMessage.FileName = _FileName;
            _TrackMessageSerializator.SerializeObject(_DataStream, trackMessage);
            _DataStream.Flush();
            return _Index - 1;
        }
    }
}
 