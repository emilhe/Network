using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace BusinessLogic.Interfaces
{
    public interface IMeasureable
    {

        /// <summary>
        /// Are we measureing?
        /// </summary>
        bool Measuring { get; }

        /// <summary>
        /// Start measuring.
        /// </summary>
        void Start(int ticks);

        /// <summary>
        /// Stop measuring and discard all previous measurements.
        /// </summary>
        void Clear();

        /// <summary>
        /// Signal that the measureable should sample!
        /// </summary>
        void Sample(int tick);

        /// <summary>
        /// Get the measurement time series.
        /// </summary>
        /// <returns></returns>
        List<ITimeSeries> CollectTimeSeries();

    }
}
