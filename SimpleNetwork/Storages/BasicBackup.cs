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
        private readonly double _mInflow;
        private readonly double _mEff;

        public BasicBackup(string name, double capacity, double initialCapacity, double inflow, double eff)
        {
            _mEff = eff;
            _mInflow = inflow;
            _mCore = new BasicStorage(name, 1, capacity, initialCapacity);
        }

        public BasicBackup(string name, double capacity)
        {
            _mInflow = 0;
            _mEff = 0;
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

        public void Start(int ticks)
        {
            ((IMeasureable) _mCore).Start(ticks);
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
            get { return _mEff; }
        }

        public double InitialEnergy
        {
            get { return _mCore.InitialEnergy; }
        }

        public double NominalEnergy
        {
            get { return _mCore.NominalEnergy; }
        }

        public double Inject(double amount)
        {
            // Only negative energy (discharge) can be injected.
            return amount > 0 ? amount : ((IStorage)_mCore).Inject(amount);
        }

        public double InjectMax(Response response)
        {
            // A backup cannot be charged.
            return (response == Response.Charge) ? 0 : ((IStorage)_mCore).InjectMax(response);
        }

        public double RemainingEnergy(Response response)
        {
            // A backup cannot be charged.
            return response == Response.Charge ? 0 : ((IStorage)_mCore).RemainingEnergy(response);
        }

        public double AvailableEnergy(Response response)
        {
            // A backup cannot be charged.
            return response == Response.Charge ? 0 : ((IStorage)_mCore).AvailableEnergy(response);
        }

        public void ResetEnergy()
        {
            ((IStorage)_mCore).ResetEnergy();
        }

        public double Capacity
        {
            get { return _mCore.Capacity; }
            set { _mCore.Capacity = value; }
        }

        public double ChargeLevel
        {
            get { return _mCore.ChargeLevel; }
        }

        public void TickChanged(int tick)
        {
            _mCore.Inject(_mInflow);
            ((ITickListener) _mCore).TickChanged(tick);
        }
    }
}
