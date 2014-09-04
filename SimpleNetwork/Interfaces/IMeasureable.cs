using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IMeasureable
    {

        /// <summary>
        /// Start a measurement (all previous measurements are discarded).
        /// </summary>
        void StartMeasurement();

        /// <summary>
        /// Discard all previous measurements.
        /// </summary>
        void Reset();

        /// <summary>
        /// Is a measurement currently running?
        /// </summary>
        bool Measurering { get; }

    }
}
