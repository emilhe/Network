using System;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.Storages
{
    public class Curtailment : IStorage
    {

        private bool _mMeasurering;

        public bool Measurering
        {
            get { return _mMeasurering; }
        }

        public ITimeSeries TimeSeries { get; private set; }

        public void StartMeasurement()
        {
            TimeSeries = new SparseTimeSeries(Name);
            _mMeasurering = true;
        }

        public void Reset()
        {
            TimeSeries = null;
            _mMeasurering = false;
        }

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

        public double Inject(int tick, double amount)
        {
            if (_mMeasurering) TimeSeries.AddData(tick, amount);
            return 0;
        }

        public double Restore(int tick, Response response)
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
    }
}
