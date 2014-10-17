using BusinessLogic.Interfaces;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Storages
{
    /// <summary>
    /// Battery storage model (efficiency = 1).
    /// </summary>
    public class BatteryStorage : IStorage
    {

        private readonly BasicStorage _mCore;

        public BatteryStorage(double capacity, double initialCapacity = 0)
        {
            _mCore = new BasicStorage("Battery storage", 1, capacity, initialCapacity);
        }

        public bool Measurering
        {
            get { return _mCore.Measurering; }
        }

        public ITimeSeries TimeSeries
        {
            get { return _mCore.TimeSeries; }
        }

        public void StartMeasurement()
        {
            ((IMeasureableLeaf)_mCore).StartMeasurement();
        }

        public void Reset()
        {
            ((IMeasureableLeaf)_mCore).Reset();
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

        public double Inject(int tick, double amount)
        {
            return ((IStorage)_mCore).Inject(tick, amount);
        }

        public double Restore(int tick, Response response)
        {
            return ((IStorage)_mCore).Restore(tick, response);
        }

        public double RemainingCapacity(Response response)
        {
            return ((IStorage)_mCore).RemainingCapacity(response);
        }

        public void ResetCapacity()
        {
            ((IStorage)_mCore).ResetCapacity();
        }
    }
}
