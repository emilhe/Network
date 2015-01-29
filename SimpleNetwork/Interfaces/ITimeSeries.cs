using System;
using System.Collections.Generic;

namespace BusinessLogic.Interfaces
{
    /// <summary>
    /// Time series abstraction.
    /// </summary>
    public interface ITimeSeries : IEnumerable<ITimeSeriesItem>
    {
        /// <summary>
        /// Name/description of the time series.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Number of entries.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Get the value for a specific time stamp.
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        double GetValue(int tick);

        /// <summary>
        /// Calcualate average (expensive operation).
        /// </summary>
        /// <returns> average </returns>
        double GetAverage();

        /// <summary>
        /// Get all values from the time series (no matter the time stamp).
        /// </summary>
        /// <returns></returns>
        List<double> GetAllValues();

        /// <summary>
        /// Properties; can be all kind of stuff.
        /// </summary>
        Dictionary<string, string> Properties { get; }

        /// <summary>
        /// Which properties (besides country) should be included in the name?
        /// </summary>
        List<string> DisplayProperties { get; }

        /// <summary>
        /// Scale all data by.
        /// </summary>
        void SetScale(double scale);

        /// <summary>
        /// Offset all data by.
        /// </summary>
        void SetOffset(int ticks);

    }

}
