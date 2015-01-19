using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;

namespace BusinessLogic.Storages
{
    public class HydroReservoirStorage : IStorage
    {

        private readonly BasicStorage _mCore;

        public HydroReservoirStorage(BasicStorage core)
        {
            _mCore = core;
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

        public void Sample(int tick)
        {
            ((IMeasureable) _mCore).Sample(tick);
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
            // It is NOT possible to discharge the reservoir; the generator is already at max.
            if (amount < 0) return amount;
            return ((IStorage)_mCore).Inject(amount);
        }

        public double InjectMax(Response response)
        {
            // It is NOT possible to discharge the reservoir; the generator is already at max.
            if (response.Equals(Response.Discharge)) return 0;
            return ((IStorage)_mCore).InjectMax(response);
        }

        public double RemainingEnergy(Response response)
        {
            return ((IStorage)_mCore).RemainingEnergy(response);
        }

        public double AvailableEnergy(Response response)
        {
            // It is NOT possible to discharge the reservoir; the generator is already at max.
            if (response.Equals(Response.Discharge)) return 0;
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

    }
}
