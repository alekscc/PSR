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
        private bool isPaused = false;

        public TimeCounter()
        {

        }
        public void Start()
        {
            timeStart = DateTime.Now;
            interval = TimeSpan.Zero;
            time = 0;
            totalTime = 0;
            isPaused = false;
        }
        public void Stop()
        {
            if (!isPaused)
            {
                timeStop = DateTime.Now;
                interval = timeStop - timeStart;
                totalTime += interval.Ticks * 100;
            }
      
        }
        public void Pause()
        {
            timeStop = DateTime.Now;
            interval = timeStop - timeStart;
            totalTime += interval.Ticks * 100;
            isPaused = true;
        }
        public void Unpause()
        {
            timeStart = DateTime.Now;
            isPaused = false;
        }
        public long GetTime()
        {
            return totalTime;
        }
    }
}
