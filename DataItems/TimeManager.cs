﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataItems
{
    public class TimeManager
    {

        #region Fields

        public DateTime StartTime { get; set; }
        public int Interval { get; set; }

        #endregion

        #region Singleton

        private static TimeManager _mInstance;

        private TimeManager(){}

        public static TimeManager Instance()
        {
            return _mInstance ?? (_mInstance = new TimeManager());
        }

        #endregion

        public DateTime GetTime(int tick)
        {
            return StartTime.AddMinutes(tick*_mInstance.Interval);
        }
    }
}