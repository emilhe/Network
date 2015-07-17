using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Statistics
{
    public class MathUtils
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
        public static double Percentile(IEnumerable<double> values, double percentile)
        {
            return FastPercentile(values.OrderBy(item => item).ToList(), percentile);
        }

        /// <summary>
        /// Calculate the percentile given an ordered double array.
        /// </summary>
        private static double FastPercentile(List<double> orderedData, double percentile)
        {
            var idx = (int) Math.Round((orderedData.Count-1)*(percentile/100));
            return orderedData.Count == 0 ? 0 : orderedData[idx];
        }

        public static double[] Linspace(double min, double max, int steps)
        {
            var delta = (max - min)/((double) steps-1);
            var result = new double[steps];
            for (int i = 0; i < steps; i++)
            {
                result[i] = min + delta*i;
            }
            return result;
        }
    }
}
