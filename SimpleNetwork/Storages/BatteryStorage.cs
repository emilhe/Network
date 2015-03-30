using System.Collections.Generic;
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
            ((IMeasureable)_mCore).Start(ticks);
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
            return ((IStorage)_mCore).Inject(amount);
        }

        public double InjectMax(Response response)
        {
            return ((IStorage)_mCore).InjectMax(response);
        }

        public double RemainingEnergy(Response response)
        {
            return ((IStorage)_mCore).RemainingEnergy(response);
        }

        public double AvailableEnergy(Response response)
        {
            return ((IStorage) _mCore).AvailableEnergy(response);
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

        public void TickChanged(int tick)
        {
            ((ITickListener) _mCore).TickChanged(tick);
        }
    }
}
