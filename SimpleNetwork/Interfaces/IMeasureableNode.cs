using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IMeasureableNode : IMeasureable
    {

        /// <summary>
        /// Collect time series.
        /// </summary>
        /// <returns></returns>
        List<ITimeSeries> CollectTimeSeries();

    }
}
