using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using ProtoBuf;
using SimpleImporter;

namespace BusinessLogic.TimeSeries
{
    /// <summary>
    /// Implementation of a continious time series.
    /// </summary>
    public class DenseTimeSeries : ITimeSeries
    {

        private readonly List<double> _mValues;

        private double _mScale = 1;
        private int _mOffset;


        /// <summary>
        /// Constructor used when data are loaded from external source.
        /// </summary>
        public DenseTimeSeries(TimeSeriesDal source)
        {
            _mCore.Properties = source.Properties;
            _mValues = source.Data ?? new List<double>();
        }

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
            return _mValues[tick + _mOffset] * _mScale;
        }

        public double GetAverage()
        {
            return _mValues.Average() * _mScale;
        }

        public void SetScale(double scale)
        {
            _mScale = scale;
        }

        public List<double> GetAllValues()
        {
            return _mValues;
        }

        public IEnumerator<ITimeSeriesItem> GetEnumerator()
        {
            return _mValues.Select((value, index) => new TickTimeSeriesItem(index + _mOffset, value * _mScale)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Offset all data by.
        /// </summary>
        public void SetOffset(int ticks)
        {
            _mOffset = ticks;
        }

        public void AddData(int tick, double value)
        {
            throw new NotImplementedException("Cannot add date to dense time series (only append is supported)");
        }

        #region Delegation

        private readonly BasicTimeSeries _mCore = new BasicTimeSeries();
        public string Name
        {
            get { return (Properties.ContainsKey("Country") ? Properties["Country"] + ", " : "") + _mCore.Name; }
            set { _mCore.Name = value; }
        }

        public Dictionary<string, string> Properties
        {
            get { return _mCore.Properties; }
        }

        #endregion

    }
}
