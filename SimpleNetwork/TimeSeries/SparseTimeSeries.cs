using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using BusinessLogic.Interfaces;
using SimpleImporter;
using Utils;

namespace BusinessLogic.TimeSeries
{
    /// <summary>
    /// Implementation of a non continious time series.
    /// </summary>
    public class SparseTimeSeries : ITimeSeries
    {

        public int Count { get { return _mValues.Count; } }

        private readonly Dictionary<int, double> _mValues;

        public SparseTimeSeries(string name, int capacity = 100)
        {
            _mCore.Properties.Add("Name", name);
            _mValues = new Dictionary<int, double>(capacity);
        }

        public SparseTimeSeries(TimeSeriesDal source)
        {
            _mCore.Properties = source.Properties;
            _mValues = source.DataIndices.Zip(source.Data, (k,v) => new {k,v}).ToDictionary(x => x.k, x => x.v);
        }

        public void AddData(int tick, double value)
        {
            // For now, just overwrite extra data.
            if (_mValues.ContainsKey(tick)) _mValues[tick] = value;
            else _mValues.Add(tick, value);
        }

        public double GetLastValue(int tick)
        {
            var keys = _mValues.Keys.Where(item => item <= tick);
            return !keys.Any() ? double.NaN : GetValue(keys.Last());
        }


        public double GetValue(int tick)
        {
            if (!_mValues.ContainsKey(tick)) return double.NaN;
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

        #region Data extraction

        public List<double> GetAllValues()
        {
            return _mValues.Select(item => item.Value).ToList();
        }

        public List<int> GetAllIndices()
        {
            return _mValues.Select(item => item.Key).ToList();
        }

        #endregion

        #region Delegation

        private readonly BasicTimeSeries _mCore = new BasicTimeSeries();

        public string Name
        {
            get
            {
                var baseString = (Properties.ContainsKey("Country") ? Properties["Country"] + ", " : "") + _mCore.Name;

                if (!DisplayProperties.Any()) return baseString;

                var propertyString =
                    DisplayProperties.Where(property => Properties.ContainsKey(property))
                        .Aggregate(": ", (current, property) => current + (Properties[property] + " "));

                return baseString + propertyString;
            }
            set { _mCore.Name = value; }
        }

        public Dictionary<string, string> Properties
        {
            get { return _mCore.Properties; }
        }

        public List<string> DisplayProperties { get { return _mCore.DisplayProperties; } }

        #endregion

    }
}
