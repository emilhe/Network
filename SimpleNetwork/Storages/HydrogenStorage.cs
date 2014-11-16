using System.Collections.Generic;
using BusinessLogic.Interfaces;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Storages
{
    /// <summary>
    /// Hydrogen storage model (efficiency = 0.6).
    /// </summary>
    public class HydrogenStorage : IStorage
    {

        private readonly BasicStorage _mCore;

        public HydrogenStorage(double capacity, double initalCapacity = 0)
        {
            _mCore = new BasicStorage("Hydrogen storage", 0.6, capacity, initalCapacity);
        }

        public void Sample(int tick)
        {
            _mCore.Sample(tick);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return ((IMeasureable) _mCore).CollectTimeSeries();
        }

        public bool Measuring
        {
            get { return _mCore.Measuring; }
        }

        public void Start()
        {
            ((IMeasureable) _mCore).Start();
        }

        public void Clear()
        {
            _mCore.Clear();
        }

        public string Name
        {
            get { return _mCore.Name; }
        }

        public double Efficiency
        {
            get { return _mCore.Efficiency; }
        }

        public double InitialCapacity
        {
            get { return _mCore.InitialCapacity; }
        }

        public double Capacity
        {
            get { return _mCore.Capacity; }
        }

        public double Inject(double amount)
        {
            return ((IStorage)_mCore).Inject(amount);
        }

        public double Restore(Response response)
        {
            return ((IStorage)_mCore).Restore(response);
        }

        public double RemainingCapacity(Response response)
        {
            return ((IStorage)_mCore).RemainingCapacity(response);
        }

        public void ResetCapacity()
        {
            ((IStorage)_mCore).ResetCapacity();
        }

        public double LimitIn
        {
            get { return _mCore.LimitIn; }
            set { _mCore.LimitIn = value; }
        }

        public double LimitOut
        {
            get { return _mCore.LimitOut; }
            set { _mCore.LimitOut = value; }
        }

    }
}
