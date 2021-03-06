﻿using RodSoft.Core.Communications;
using RodSoft.OSM.Tracking.Telemetry.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RodSoft.OSM.Tracking.Telemetry
{
    public class GPSMessagesCashService : CashService<TrackMessage>
    {
        public GPSMessagesCashService()
            : base()
        {

        }

        public GPSMessagesCashService(CommunicationSettings telemetrySettings)
            : base(telemetrySettings)
        {
        }

    }
    /*
    public class CashService 
    {
        public class CashedDataRef
        {
            public long Ticks;
            public string FileName;
            public IList<int> SendedItems;
            public IList<TrackMessage> TrackMessages;
            public int LoadedItemsCount;
            public bool IsLoaded;
        }

        public DateTime RefDate { get; } = new DateTime(2019, 1, 1);

        protected TrackMessageSerializator _TrackMessageSerializator = new TrackMessageSerializator();

        public string CashFolder { get; set; }

        protected Stream _DataStream;

        protected Stream _IndexSteam;

        protected int _Index = 0;

        protected DateTime _StartTime = DateTime.Now;

        protected string _FileName;

        protected IList<CashedDataRef> _CashedData = new List<CashedDataRef>();

        public IList<TrackMessage> Messages = new List<TrackMessage>();

        protected CashedDataRef _CurrentFile;

        public int MaximumItemsPerCashFile { get; set; } = 1000;

        public int MaximumSecondsPerCashFile { get; set; } = 1000;

        public CashService()
        {

        }

        public CashService(TelemetrySettings telemetrySettings)
        {
            if (telemetrySettings != null)
            {
                CashFolder = telemetrySettings.CashFolder == null ? "" : "Cash";
                if (telemetrySettings.MaximumItemsPerCashFile > 0)
                    MaximumItemsPerCashFile = telemetrySettings.MaximumItemsPerCashFile;
                if (telemetrySettings.MaximumSecondsPerCashFile > 0)
                {
                    MaximumSecondsPerCashFile = telemetrySettings.MaximumSecondsPerCashFile;
                }
            }
        }

        protected virtual IList<int> LoadSendedItemsIndex(string fileName)
        {
            IList<int> sendedItemsIndexes = new List<int>();
            fileName = String.Format("{0}\\{1}.idx", CashFolder, fileName);
            if(File.Exists(fileName))
            {
                byte[] indexDate = File.ReadAllBytes(fileName);
                int i = 0;
                while(i + 1 < indexDate.Length)
                {
                    int index = indexDate[i] * 256 + indexDate[i + 1];
                    sendedItemsIndexes.Add(index);
                    i = i + 2;
                }
                sendedItemsIndexes = sendedItemsIndexes.OrderBy(index => index).ToList();
            }
            return sendedItemsIndexes;
        }

        protected virtual void LoadCashFilesList()
        {
            DirectoryInfo d = new DirectoryInfo(CashFolder);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.dat"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                string fileName = file.Name;
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
                //string strFileDate = fileName.Substring(0, fileName.IndexOf(' '));
                //DateTime fileDate;
                long ticks = 0;
                if(long.TryParse(fileName, out ticks))
                {
                    CashedDataRef cashedDataRef = new CashedDataRef();
                    cashedDataRef.FileName = fileName;
                    cashedDataRef.Ticks = ticks;
                    cashedDataRef.SendedItems = LoadSendedItemsIndex(fileName);
                    _CashedData.Add(cashedDataRef);
                }
            }
            _CashedData = _CashedData.OrderBy(cashedDataRef => cashedDataRef.Ticks).ToList();
        }

        public virtual bool LoadCashedData(CashedDataRef cashedDataRef)
        {
            string fileName = String.Format("{0}\\{1}.dat", CashFolder, cashedDataRef.FileName);
            using (Stream stream = new FileStream(fileName, FileMode.Open))
            {
                int index = 0;
                while(stream.Position < stream.Length)
                {
                    TrackMessage trackMessage = _TrackMessageSerializator.DeserializeObject<TrackMessage>(stream);
                    if(!cashedDataRef.SendedItems.Contains(index++))
                    {
                        lock(Messages)
                            Messages.Insert(cashedDataRef.LoadedItemsCount++, trackMessage);
                    }
                }
                stream.Close();
                stream.Dispose();
            }
            cashedDataRef.IsLoaded = true;
            return cashedDataRef.LoadedItemsCount > 0;
        }

        public virtual bool LoadCashedData()
        {
            LoadCashFilesList();
            while(_CashedData.Count > 0)
            {
                if (!LoadCashedData(_CashedData[0]))
                {
                    DeleteCashFiles(_CashedData[0]);
                }
                else
                    return true;
            }
            return false;
        }

        private void DeleteCashFiles(CashedDataRef cashedDataRef)
        {
            try
            {
                string fileName = String.Format("{0}\\{1}.dat", CashFolder, cashedDataRef.FileName);
                if (File.Exists(fileName))
                    File.Delete(fileName);
                fileName = String.Format("{0}\\{1}.idx", CashFolder, cashedDataRef.FileName);
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            catch (Exception ex)
            {

            }
            _CashedData.Remove(cashedDataRef);
        }

        public void SaveMessage(TrackMessage trackMessage)
        {
            Save(trackMessage);
            lock (Messages)
            {
                Messages.Add(trackMessage);
            }
        }

        public virtual int Save(TrackMessage trackMessage)
        {
            DateTime time = DateTime.Now;
            if (_DataStream == null)
            {
                _StartTime = DateTime.Now;
                //                _FileName = String.Format("{0} {1}", time.ToShortDateString ().Replace('.', '_'), time.ToShortTimeString().Replace(':', '_'));
                _CurrentFile = new CashedDataRef();
                _CurrentFile.Ticks = time.Subtract(RefDate).Ticks;
                _CurrentFile.IsLoaded = true;
                _FileName = _CurrentFile.Ticks.ToString();
                _CurrentFile.FileName = _FileName;
                string fileName = String.Format("{0}\\{1}", CashFolder, _FileName);
                _DataStream = File.Open(fileName + ".dat", FileMode.OpenOrCreate);
                _IndexSteam = File.Open(fileName + ".idx", FileMode.OpenOrCreate);
                _CurrentFile.SendedItems = new List<int>();
                _Index = 0;
                _CashedData.Add(_CurrentFile);

            }
            _CurrentFile.LoadedItemsCount++;

            trackMessage.Index = _Index++;
            trackMessage.FileName = _CurrentFile.FileName;
            _TrackMessageSerializator.SerializeObject(_DataStream, trackMessage);
            _DataStream.Flush();
            if (time.Subtract(_StartTime).TotalSeconds > MaximumSecondsPerCashFile || _Index >= MaximumItemsPerCashFile)
            {
                _DataStream.Close();
                _DataStream.Dispose();
                _DataStream = null;
                _IndexSteam.Close();
                _IndexSteam.Dispose();
                _IndexSteam = null;
                _CurrentFile = null;
            }
            return _Index - 1;
        }

        public virtual void RegisterSending(TrackMessage trackMessage)//string fileName, short index
        {
            if(trackMessage.FileName == _FileName && _IndexSteam != null)
            {
                _IndexSteam.WriteByte((byte)((trackMessage.Index & 0xFF00) >> 8));
                _IndexSteam.WriteByte((byte)(trackMessage.Index & 0xFF));
                _IndexSteam.Flush();
                _CurrentFile.SendedItems.Add(trackMessage.Index);
            }
            else
            {
                string fileName = String.Format("{0}\\{1}", CashFolder, trackMessage.FileName);
                using (Stream indexSteam = File.Open(fileName + ".idx", FileMode.Open))
                {
                    indexSteam.WriteByte((byte)((trackMessage.Index & 0xFF00) >> 8));
                    indexSteam.WriteByte((byte)(trackMessage.Index & 0xFF));
                    indexSteam.Close();
                    indexSteam.Dispose();
                }
            }
            lock (Messages)
            {
                Messages.Remove(trackMessage);
            }
            CashedDataRef cashedDataRef = _CashedData.Where(cashFile => cashFile.FileName == trackMessage.FileName).FirstOrDefault();
            if (cashedDataRef != null && --cashedDataRef.LoadedItemsCount == 0 && cashedDataRef != _CurrentFile)
            {
                if (_CashedData[0] == cashedDataRef && _CashedData.Count > 1 && !_CashedData[1].IsLoaded)
                    LoadCashedData(_CashedData[1]);
                DeleteCashFiles(cashedDataRef);
            }
        }
    } 
    */
}
 