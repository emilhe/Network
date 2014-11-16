using System;

namespace BusinessLogic
{
    public class TimeManager
    {

        #region Fields

        public DateTime StartTime { get; set; }
        public int Interval { get; set; }

        #endregion

        #region Singleton

        private static readonly TimeManager _mInstance = new TimeManager();

        private TimeManager(){}

        public static TimeManager Instance()
        {
            return _mInstance;
        }

        #endregion

        public DateTime GetTime(int tick)
        {
            return StartTime.AddMinutes(tick * _mInstance.Interval);
        }

    }
}
