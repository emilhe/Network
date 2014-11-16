using System.Collections.Generic;
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

        public void Sample(int tick)
        {
             _mCore.Sample(tick);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return _mCore.CollectTimeSeries();
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

        public double Inject(double amount)
        {
            // Only negative energy (discharge) can be injected.
            return amount > 0 ? amount : ((IStorage)_mCore).Inject(amount);
        }

        public double Restore(Response response)
        {
            // A backup cannot be charged.
            return (response == Response.Charge) ? 0 : ((IStorage)_mCore).Restore(response);
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
