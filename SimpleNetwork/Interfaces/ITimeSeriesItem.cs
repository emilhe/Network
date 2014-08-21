using System;

namespace SimpleNetwork.Interfaces
{
    /// <summary>
    /// Data point abstraction.
    /// </summary>
    public interface ITimeSeriesItem
    {
        DateTime TimeStamp { get; }
        double Value { get; }
    }
}
