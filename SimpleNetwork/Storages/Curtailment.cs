using System;
using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.Storages
{
    public class Curtailment : IStorage
    {

        private double _mSample;

        public string Name
        {
            get { return "Curtailment"; }
        }

        public double Efficiency
        {
            get { return -1; }
        }

        public double InitialCapacity
        {
            get { return double.PositiveInfinity; }
        }

        public double Capacity
        {
            get { return double.PositiveInfinity; }
        }

        public double Inject(double amount)
        {
            return _mSample = amount;
        }

        public double Restore(Response response)
        {
            return 0;
        }

        public double RemainingCapacity(Response response)
        {
            return response.Equals(Response.Charge) ? Double.PositiveInfinity : double.NegativeInfinity;
        }

        public void ResetCapacity()
        {
            // Not possible.
        }

        public double LimitIn { get { return double.PositiveInfinity; } }
        public double LimitOut { get { return double.NegativeInfinity; } }

        #region Measurement

        private ITimeSeries _mTimeSeries;
        private bool _mMeasuring;

        public bool Measuring { get { return _mMeasuring; } }

        public void Start()
        {
            _mTimeSeries = new DenseTimeSeries(Name);
            _mTimeSeries.AppendData(_mSample);
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
            _mTimeSeries.AppendData(_mSample);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries> { _mTimeSeries };
        }

        #endregion

    }
}
