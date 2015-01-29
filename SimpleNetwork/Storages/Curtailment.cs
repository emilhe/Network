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

        public double InitialEnergy
        {
            get { return double.PositiveInfinity; }
        }

        public double NominalEnergy
        {
            get { return double.PositiveInfinity; }
        }

        public double Inject(double amount)
        {
            return _mSample = amount;
        }

        public double InjectMax(Response response)
        {
            return 0;
        }

        public double RemainingEnergy(Response response)
        {
            return response.Equals(Response.Charge) ? Double.PositiveInfinity : double.NegativeInfinity;
        }

        public double AvailableEnergy(Response response)
        {
            return RemainingEnergy(response);
        }

        public void ResetEnergy()
        {
            // Not possible.
        }

        public double Capacity { get { return double.PositiveInfinity; } }
        public double LimitOut { get { return double.NegativeInfinity; } }

        #region Measurement

        private DenseTimeSeries _mTimeSeries;
        private bool _mMeasuring;

        public bool Measuring { get { return _mMeasuring; } }

        public void Start(int ticks)
        {
            _mTimeSeries = new DenseTimeSeries(Name, ticks);
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
