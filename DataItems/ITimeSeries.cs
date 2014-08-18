using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace DataItems
{
    /// <summary>
    /// Time series abstraction.
    /// </summary>
    public interface ITimeSeries : IEnumerable<ITimeSeriesItem>, ICloneable
    {
        /// <summary>
        /// Name/description of the time series.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Get the value for a specific time stamp.
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        double GetValue(int tick);

        /// <summary>
        /// Add a data point to the time series.
        /// </summary>
        /// <param name="tick"> tick </param>
        /// <param name="value"> data point value </param>
        void AddData(int tick, double value);

        /// <summary>
        /// Append a data point to the time series.
        /// </summary>
        /// <param name="value"> data point value </param>
        void AppendData(double value);

        /// <summary>
        /// Calcualate average (expensive operation).
        /// </summary>
        /// <returns></returns>
        double GetAverage();

        /// <summary>
        /// Scale all data by.
        /// </summary>
        /// <returns></returns>
        void SetScale(double scale);
    }

    /// <summary>
    /// Data point abstraction.
    /// </summary>
    public interface ITimeSeriesItem
    {
        DateTime TimeStamp { get; }
        double Value { get; }
    }

    /// <summary>
    /// Data point representation
    /// </summary>
    public class BasicTimeSeriesItem : ITimeSeriesItem
    {
        public DateTime TimeStamp { get; private set; }
        public double Value { get; private set; }

        public BasicTimeSeriesItem(DateTime time, double value)
        {
            Value = value;
            TimeStamp = time;
        }
    }

    /// <summary>
    /// Data point representation
    /// </summary>
    public class TickTimeSeriesItem : ITimeSeriesItem
    {
        public DateTime TimeStamp { get { return TimeManager.Instance().GetTime(_mTick); } }
        public double Value { get; private set; }

        private readonly int _mTick;

        public TickTimeSeriesItem(int tick, double value)
        {
            Value = value;
            _mTick = tick;
        }
    }

    /// <summary>
    /// Implementation of a non continious time series.
    /// </summary>
    public class SparseTimeSeries : ITimeSeries
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

    /// <summary>
    /// Implementation of a continious time series.
    /// </summary>
    [ProtoContract]
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
            return _mValues[tick]*_mScale;
        }

        public double GetAverage()
        {
            return _mValues.Average()*_mScale;
        }

        public void SetScale(double scale)
        {
            _mScale = scale;
        }

        public IEnumerator<ITimeSeriesItem> GetEnumerator()
        {
            return _mValues.Select((value, index) => new TickTimeSeriesItem(index, value*_mScale)).GetEnumerator();
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
