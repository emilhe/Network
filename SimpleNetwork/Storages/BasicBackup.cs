using BusinessLogic.Interfaces;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Storages
{
    /// <summary>
    /// Backup (non rechargeable).
    /// </summary>
    public class BasicBackup : IStorage
    {
        private readonly BasicStorage _mCore;

        public BasicBackup(string name, double capacity)
        {
            _mCore = new BasicStorage(name, 1, capacity, capacity);
        }

        public bool Measurering
        {
            get { return _mCore.Measurering; }
        }

        public Interfaces.ITimeSeries TimeSeries
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
            // A backup cannot be charged.
            get { return 0; }
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
            // Only negative energy (discharge) can be injected.
            return amount > 0 ? amount : ((IStorage)_mCore).Inject(tick, amount);
        }

        public double Restore(int tick, Response response)
        {
            // A backup cannot be charged.
            return (response == Response.Charge) ? 0 : ((IStorage)_mCore).Restore(tick, response);
        }

        public double RemainingCapacity(Response response)
        {
            // A backup cannot be charged.
            return response == Response.Charge ? 0 : ((IStorage)_mCore).RemainingCapacity(response);
        }

        public void ResetCapacity()
        {
            ((IStorage)_mCore).ResetCapacity();
        }
    }
}
