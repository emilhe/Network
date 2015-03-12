using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using SimpleImporter;

namespace BusinessLogic.TimeSeries
{
    /// <summary>
    /// Use when performance is critical; arrays are slightly faster than lists.
    /// </summary>
    public class ImmutableTimeSeries : ITimeSeries
    {

        public int Count { get { return _mValues.Length ; } }

        private readonly double[] _mValues;
        private double _mScale = 1;
        private int _mOffset;

        public ImmutableTimeSeries(TimeSeriesDal source)
        {
            _mCore.Properties = source.Properties;
            _mValues = source.Data == null ? new double[0] : source.Data.ToArray();
        }

        public void SetOffset(int ticks)
        {
            _mOffset = ticks;
        }

        public double GetValue(int tick)
        {
            return _mValues[tick + _mOffset]*_mScale;
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
            throw new NotImplementedException();
        }

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

        #region Enumeration

        public IEnumerator<ITimeSeriesItem> GetEnumerator()
        {
            return _mValues.Select((value, index) => new TickTimeSeriesItem(index + _mOffset, value * _mScale)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }

}
