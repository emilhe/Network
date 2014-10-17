using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using NUnit.Framework;
using SimpleImporter;
using Utils.Statistics;


namespace BusinessLogic.TimeSeries
{
    public static class Mapping
    {

        /// <summary>
        /// Create a data bin table with X number of bins. Default X is 16.
        /// </summary>
        /// <param name="input"> the time series data </param>
        /// <param name="binCount"> the (optionary) bin count </param>
        /// <returns> a data bin table </returns>
        public static DataBinTable ToDataBinTable(this ITimeSeries input, int binCount = 16)
        {
            var tsValues = input.GetAllValues();
            double[] midPoints;
            double[] values;

            // No values are present; requires special care.
            if (!tsValues.Any())
            {
                midPoints = new double[] {0};
                values = new double[] {0};
                return new DataBinTable(midPoints, values);
            }

            var min = Math.Floor(tsValues.Min());
            var max = Math.Ceiling(tsValues.Max());
            var binSize = (max - min)/(binCount - 1);

            // Only one value is present; requires special care.
            if (binSize == 0)
            {
                midPoints = new[] {tsValues[0]};
                values = new double[] {tsValues.Count};
            }
            // Calculate midpoints & bins.
            else
            {
                midPoints = new double[binCount];
                for (int i = 0; i < binCount; i++) midPoints[i] = min + (0.5 + i)*binSize;
                values = new double[binCount];
                foreach (var value in tsValues) values[(int) Math.Floor((value - min)/binSize)]++;
            }

            return new DataBinTable(midPoints, values);
        }

        /// <summary>
        /// Create a data bin table with specefic bins specified by the bins variable.
        /// </summary>
        /// <param name="input"> the time series data </param>
        /// <param name="midPoints"> the bin midPoints (assumed to be ordered and equally spaced) </param>
        /// <returns> a data bin table </returns>
        public static DataBinTable ToDataBinTable(this ITimeSeries input, double[] midPoints)
        {
            var tsValues = input.GetAllValues();
            var binSize = midPoints[1] - midPoints[0];
            var min = midPoints[0] - binSize/2;

            var values = new double[midPoints.Length];
            foreach (var value in tsValues)
            {
                var idx = (int) Math.Floor((value - min)/binSize);
                if (idx < 0) values[0]++;
                else if (idx < values.Length) values[(int) Math.Floor((value - min)/binSize)]++;
                else values[midPoints.Length - 1]++;
            }

            return new DataBinTable(midPoints, values);
        }


        public static TimeSeriesDal ToTimeSeriesDal(this ITimeSeries input)
        {
            var sparse = input as SparseTimeSeries;
            if (sparse != null)
            {
                return new TimeSeriesDal
                {
                    Data = sparse.GetAllValues(),
                    DataIndices = sparse.GetAllIndices(),
                    Properties = sparse.Properties
                };
            }

            var dense = input as DenseTimeSeries;
            if (dense != null)
            {
                return new TimeSeriesDal
                {
                    Data = dense.GetAllValues(),
                    DataIndices = null,
                    Properties = dense.Properties
                };
            }

            return null;
        }

        public static ITimeSeries ToTimeSeries(this TimeSeriesDal input)
        {
            return input.DataIndices == null ? (ITimeSeries) new DenseTimeSeries(input) : new SparseTimeSeries(input);
        }

    }
}
