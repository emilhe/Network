using System;
using System.Collections.Generic;

namespace SimpleNetwork.Interfaces
{
    /// <summary>
    /// Time series abstraction.
    /// </summary>
    public interface ITimeSeries : IEnumerable<ITimeSeriesItem>, ICloneable
    {
        /// <summary>
        /// Name/description of the time series.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Get the value for a specific time stamp.
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        double GetValue(int tick);

        /// <summary>
        /// Add a data point to the time series.
        /// </summary>
        /// <param name="tick"> tick </param>
        /// <param name="value"> data point value </param>
        void AddData(int tick, double value);

        /// <summary>
        /// Append a data point to the time series.
        /// </summary>
        /// <param name="value"> data point value </param>
        void AppendData(double value);

        /// <summary>
        /// Calcualate average (expensive operation).
        /// </summary>
        /// <returns></returns>
        double GetAverage();

        /// <summary>
        /// Scale all data by.
        /// </summary>
        /// <returns></returns>
        void SetScale(double scale);
    }

}
