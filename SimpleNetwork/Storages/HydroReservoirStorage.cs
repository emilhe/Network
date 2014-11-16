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

        public HydroReservoirStorage(double reservoirCapacity, double initialFillingLevel)
        {
            _mCore = new BasicStorage("Hydro reservoir", 1, reservoirCapacity, reservoirCapacity * initialFillingLevel);
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
            // It is NOT possible to discharge the reservoir; the generator is already at max.
            if (amount < 0) return amount;
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
