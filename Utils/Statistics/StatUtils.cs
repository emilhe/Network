using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Statistics
{
    public class StatUtils
    {

        /// <summary>
        /// The capacity is maximum of the absolute value of the 0.5% and the 99.5% quantiles.
        /// </summary>
        public static double CalcCapacity(List<double> values)
        {
            var orderedValues = values.OrderBy(item => item).ToList();

            var min = Math.Abs(Percentile(orderedValues, 0.5));
            var max = Math.Abs(Percentile(orderedValues, 99.5));

            return Math.Max(min, max);
        }

        /// <summary>
        /// Calculate the percentile given an ordered double array.
        /// </summary>
        public static double Percentile(List<double> orderedData, double quantile)
        {
            var idx = (int) Math.Ceiling((orderedData.Count-1)*(quantile/100));
            return orderedData[idx];
        }

    }
}
