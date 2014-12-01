using System;

namespace BusinessLogic.Interfaces
{
    /// <summary>
    /// Data point abstraction.
    /// </summary>
    public interface ITimeSeriesItem
    {
        DateTime TimeStamp { get; }
        double Value { get; }
        int Tick { get; }
    }
}
