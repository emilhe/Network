using System;
using BusinessLogic.Interfaces;

namespace BusinessLogic.TimeSeries
{
    /// <summary>
    /// Data point representation
    /// </summary>
    public class BasicTimeSeriesItem : ITimeSeriesItem
    {
        public DateTime TimeStamp { get; private set; }
        public double Value { get; private set; }
        public int Tick { get { return 0; } }

        public BasicTimeSeriesItem(DateTime time, double value)
        {
            Value = value;
            TimeStamp = time;
        }
    }
}
