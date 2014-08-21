using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleNetwork.Interfaces;
using SimpleNetwork.TimeSeries;

namespace DataItems.TimeSeries
{
    /// <summary>
    /// Implementation of a non continious time series.
    /// </summary>
    public class SparseTimeSeries : SimpleNetwork.Interfaces.ITimeSeries
    {
        public string Name { get; set; }
        private readonly Dictionary<int, double> _mValues;

        public SparseTimeSeries(string name, int capacity = 100)
        {
            Name = name;
            _mValues = new Dictionary<int, double>(capacity);
        }

        public void AddData(int tick, double value)
        {
            // For now, just overwrite extra data.
            if (_mValues.ContainsKey(tick)) _mValues[tick] = value;
            else _mValues.Add(tick, value);
        }

        public double GetValue(int tick)
        {
            return _mValues[tick];
        }

        public double GetAverage()
        {
            return _mValues.Values.Average();
        }

        public void SetScale(double scale)
        {
            foreach (var pair in _mValues) _mValues[pair.Key] = pair.Value * scale;
        }

        public IEnumerator<ITimeSeriesItem> GetEnumerator()
        {
            return
                _mValues.Select(item => new TickTimeSeriesItem(item.Key, item.Value))
                    .Cast<ITimeSeriesItem>()
                    .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AppendData(double value)
        {
            throw new NotImplementedException("Cannot append data to sparse time series.");
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
