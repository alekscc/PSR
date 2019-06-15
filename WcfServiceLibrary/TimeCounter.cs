using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfServiceLibrary
{
    class TimeCounter
    {
        private DateTime timeStart;
        private DateTime timeStop;
        private TimeSpan interval;
        private long time;
        private long totalTime;

        public TimeCounter()
        {

        }
        public void Start()
        {
            timeStart = DateTime.Now;
            interval = TimeSpan.Zero;
            time = 0;
            totalTime = 0;
        }
        public void Stop()
        {
            timeStop = DateTime.Now;
            interval = timeStop - timeStart;
            totalTime = time + interval.Ticks * 100;
        }
        public void Pause()
        {
            timeStop = DateTime.Now;
            interval = timeStop - timeStart;
            time += interval.Ticks * 100;
        }
        public void Unpause()
        {
            timeStart = DateTime.Now;
        }
        public long GetTime()
        {
            return totalTime;
        }
    }
}
