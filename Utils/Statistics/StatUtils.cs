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
        public static double CalcCapacity(IEnumerable<double> values)
        {
            var orderedValues = values.OrderBy(item => item).ToList();

            var min = Math.Abs(FastPercentile(orderedValues, 0.5));
            var max = Math.Abs(FastPercentile(orderedValues, 99.5));

            return Math.Max(min, max);
        }

        /// <summary>
        /// The capacity is maximum of the absolute value of the 0.5% and the 99.5% quantiles.
        /// </summary>
        public static double CalcEmpCapacity(List<double> values)
        {
            var max = Math.Max(Math.Abs(values.Min()), Math.Abs(values.Max()));
            return max/3;
        }

        /// <summary>
        /// The capacity is maximum of the absolute value of the 0.5% and the 99.5% quantiles.
        /// </summary>
        public static double CalcFullCapacity(List<double> values)
        {
            return Math.Max(Math.Abs(values.Min()), Math.Abs(values.Max()));
        }

        /// <summary>
        /// Calculate the percentile given an ordered double array.
        /// </summary>
        public static double Percentile(List<double> values, double percentile)
        {
            var orderedValues = values.OrderBy(item => item).ToList();
            var idx = (int)Math.Ceiling((orderedValues.Count - 1) * (percentile / 100));
            return orderedValues[idx];
        }

        /// <summary>
        /// Calculate the percentile given an ordered double array.
        /// </summary>
        private static double FastPercentile(List<double> orderedData, double percentile)
        {
            var idx = (int) Math.Ceiling((orderedData.Count-1)*(percentile/100));
            return orderedData[idx];
        }

    }
}
