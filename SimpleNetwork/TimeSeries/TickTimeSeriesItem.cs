using System;
using DataItems;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.TimeSeries
{
    /// <summary>
    /// Data point representation
    /// </summary>
    public class TickTimeSeriesItem : ITimeSeriesItem
    {
        public DateTime TimeStamp { get { return TimeManager.Instance().GetTime(_mTick); } }
        public double Value { get; private set; }

        private readonly int _mTick;

        public TickTimeSeriesItem(int tick, double value)
        {
            Value = value;
            _mTick = tick;
        }
    }
}
