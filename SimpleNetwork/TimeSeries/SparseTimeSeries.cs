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

        private double _mScale = 1;

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

        public double GetValue(int tick)
        {
            if (!_mValues.ContainsKey(tick)) return double.NaN;
            return _mValues[tick]*_mScale;
        }

        public double GetAverage()
        {
            return _mValues.Values.Average() * _mScale;
        }

        public void SetScale(double scale)
        {
            _mScale = scale;
        }

        public void SetOffset(int ticks)
        {
            throw new NotImplementedException();
        }

        public void AddData(int tick, double value)
        {
            // For now, just overwrite extra data.
            if (_mValues.ContainsKey(tick)) _mValues[tick] = value;
            else _mValues.Add(tick, value);
        }

        public double GetLastValue(int tick)
        {
            // This operation is VERY expensive. Consider using the indexed verion if you call this method often.
            var keys = _mValues.Keys.Where(item => item <= tick);
            return !keys.Any() ? double.NaN : GetValue(keys.Last());
        }

        #region Enumeration

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ITimeSeriesItem> GetEnumerator()
        {
            return
                _mValues.Select(item => new TickTimeSeriesItem(item.Key, item.Value))
                    .Cast<ITimeSeriesItem>()
                    .GetEnumerator();
        }

        #endregion

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

        private readonly TimeSeriesMetaData _mCore = new TimeSeriesMetaData();

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
