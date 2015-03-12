using System.Collections.Generic;
using BusinessLogic.Interfaces;

namespace BusinessLogic.TimeSeries
{
    public class Measureable : IMeasureable
    {

        public string Name { get; private set; }

        public Measureable(string name)
        {
            Name = name;
        }

        public double CurrentValue { get; set; }

        #region Measurement

        private DenseTimeSeries _mTimeSeries;
        private bool _mMeasuring;

        public bool Measuring { get { return _mMeasuring; } }

        public void Start(int ticks)
        {
            _mTimeSeries = new DenseTimeSeries(Name, ticks);
            _mTimeSeries.AppendData(CurrentValue);
            _mMeasuring = true;
        }

        public void Clear()
        {
            _mTimeSeries = null;
            _mMeasuring = false;
        }

        public void Sample(int tick)
        {
            if (!_mMeasuring) return;
            _mTimeSeries.AppendData(CurrentValue);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries> { _mTimeSeries };
        }

        #endregion

    }
}
