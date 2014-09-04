using System;
using BusinessLogic.Interfaces;

namespace BusinessLogic.TimeSeries
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
