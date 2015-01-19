using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace SimpleImporter
{
    /// <summary>
    /// The proto cache can be used for loading read-only elements ONLY. 
    /// By loading throught the proto cache, memory can be saved since only one copy of the data will be loaded.
    /// </summary>
    public class ProtoCache
    {

        // For now, just flush all at once :).
        public static void Flush()
        {
            TimeSeries.Clear();
        }

        #region Time series

        private static readonly Dictionary<string, TimeSeriesDal> TimeSeries = new Dictionary<string, TimeSeriesDal>();

        /// <summary>
        /// Method for loading time series from import folder (typically data imports).
        /// </summary>
        /// <param name="key"> file name </param>
        /// <returns> the time series </returns>
        public static TimeSeriesDal LoadTimeSeriesFromImport(string key)
        {
            if(!TimeSeries.ContainsKey(key)) TimeSeries[key] = ProtoStore.LoadTimeSeriesFromImport(key);
            return TimeSeries[key];
        }

        #endregion

    }
}
