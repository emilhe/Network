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
            _mCore = new BasicStorage("Hydrogen storage", 0.7071067811865475, capacity, initalCapacity); // Eff = sqrt(0.5)
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
