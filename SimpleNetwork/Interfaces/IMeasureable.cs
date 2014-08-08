using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DataItems;

namespace SimpleNetwork.Interfaces
{
    public interface IMeasureable
    {
        /// <summary>
        /// Is a measurement currently running?
        /// </summary>
        bool Measurering { get; }

        /// <summary>
        /// Get the time series of the measureable quantity.
        /// </summary>
        /// <returns></returns>
        ITimeSeries TimeSeries { get; }

        /// <summary>
        /// Start a measurement (all previous measurements are discarded).
        /// </summary>
        void StartMeasurement();

        /// <summary>
        /// Discard all previous measurements.
        /// </summary>
        void Reset();
    }
}
