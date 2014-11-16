using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Generators
{
    /// <summary>
    /// Generator which an amount of energy specified by a time series.
    /// </summary>
    public class TimeSeriesGenerator : IGenerator
    {

        // TODO: Don't duplicate the time series?
        private readonly ITimeSeries _mUnderlyingTimeSeries;
        public ITimeSeries UnderlyingTimeSeries { get { return _mUnderlyingTimeSeries; } }

        public string Name { get; private set; }

        public double Production { get; private set; }

        public TimeSeriesGenerator(string name, ITimeSeries ts)
        {
            Name = name;
            _mUnderlyingTimeSeries = ts;
        }

        #region Measurement

        private ITimeSeries _mTimeSeries;
        private bool _mMeasuring;

        public bool Measuring { get { return _mMeasuring; } }

        public void Start()
        {
            _mTimeSeries = new DenseTimeSeries(Name);
            _mMeasuring = true;
        }

        public void Clear()
        {
            _mTimeSeries = null;
            _mMeasuring = false;
        }

        public void Sample(int tick)
        {
            if (_mMeasuring) _mTimeSeries.AppendData(Production);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries> { _mTimeSeries };
        }

        #endregion

        public void TickChanged(int tick)
        {
            Production = _mUnderlyingTimeSeries.GetValue(tick);            
        }
    }
}
