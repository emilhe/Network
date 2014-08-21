using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataItems.TimeSeries;
using ProtoBuf;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.TimeSeries
{
    /// <summary>
    /// Implementation of a continious time series.
    /// </summary>
    public class DenseTimeSeries : ITimeSeries
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        private readonly List<double> _mValues;

        private double _mScale = 1;

        public DenseTimeSeries(string name, int capacity = 100)
        {
            Name = name;
            _mValues = new List<double>(capacity);
        }

        public void AppendData(double value)
        {
            _mValues.Add(value);
        }

        public double GetValue(int tick)
        {
            return _mValues[tick] * _mScale;
        }

        public double GetAverage()
        {
            return _mValues.Average() * _mScale;
        }

        public void SetScale(double scale)
        {
            _mScale = scale;
        }

        public IEnumerator<ITimeSeriesItem> GetEnumerator()
        {
            return _mValues.Select((value, index) => new TickTimeSeriesItem(index, value * _mScale)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddData(int tick, double value)
        {
            throw new NotImplementedException("Cannot add date to dense time series (only append is supported)");
        }

        #region Serialization

        public DenseTimeSeries(string name, List<double> values)
        {
            Name = name;
            _mValues = values;
        }

        public List<double> GetAllValues()
        {
            return _mValues;
        }

        #endregion

        public object Clone()
        {
            var deepCopy = new List<double>(_mValues.Count);
            _mValues.ForEach(deepCopy.Add);
            return new DenseTimeSeries(Name, deepCopy);
        }
    }
}
