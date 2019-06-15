using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace WcfServiceLibrary
{
    class Client
    {
        private ClientData data;
        private Timer timer = new Timer();
        private bool isFree = false;
        private int locRecordVert = -1;
        private int locRecordDist = -1;
        private long totalTime = 0;
        private long dataSyncTime = 0;
        private long comTime = 0;
        private DateTime timeStart;
        private DateTime timeStop;
        private TimeSpan interval;
        private long ctime = 0;
        private bool isDone = false;

        public Client(ClientData data)
        {
            this.data = data;
        }
        public void StartTimeCounting()
        {
            timeStart = DateTime.Now;
            interval = TimeSpan.Zero;
            ctime = 0;

        }
        public void StopTimeCounting()
        {
            timeStop = DateTime.Now;
            interval = timeStop - timeStart;
            comTime = ctime + interval.Ticks * 100;
            //comTime = (interval.Ticks * 100) - totalTime;
        }
        public void PauseTimeCounting()
        {
            timeStop = DateTime.Now;
            interval =  timeStop - timeStart;
            ctime += interval.Ticks * 100;
        }
        public void UnPauseTimeCounting()
        {
            timeStart = DateTime.Now;
        }
        public long CommunicationTime
        {
            get
            {
                return comTime;
            }
        }
        public ClientData Data
        {
            get
            {
                return data;
            }
            set
            {
                value = data;
            }
        }

        public bool IsFree { get => isFree; set => isFree = value; }

        public delegate void delHandler(object source, ElapsedEventArgs e);
        public void SetTimer(delHandler handler, double interval)
        {
            timer.Elapsed += new ElapsedEventHandler(handler);
            timer.Interval = interval;
            timer.Enabled = true;

        }
        public void StopTimer()
        {
            timer.Enabled = false;
        }
        public Timer GetTimer()
        {
            return timer;
        }
        public void SetRecord(int vert, int dist)
        {
            if (locRecordDist > dist || locRecordVert == -1)
            {
                locRecordDist = dist;
                locRecordVert = vert;
            }


        }
        public void ClearRecord()
        {
            locRecordVert = -1;
            locRecordDist = -1;
            totalTime = 0;
            comTime = 0;
            isDone = false;
        }
        public int RecordVertice
        {
            get
            {
                return locRecordVert;
            }
        }
        public int RecordDistance
        {
            get
            {
                return locRecordDist;
            }
        }
        public void AddTotalTime(long val)
        {
            totalTime += val;
        }

        public long TotalTime { get => totalTime; set => totalTime = value; }
        public long DataSyncTime { get => dataSyncTime; set => dataSyncTime = value; }
        public bool IsDone { get => isDone; set => isDone = value; }
    }
}
