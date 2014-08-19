using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.Storages
{
    /// <summary>
    /// Battery storage model (efficiency = 1).
    /// </summary>
    public class BatteryStorage : IStorage
    {

        private readonly BasicStorage _mCore;

        public BatteryStorage(double capacity)
        {
            _mCore = new BasicStorage("Battery storage", 1, capacity);
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
            ((IMeasureable)_mCore).StartMeasurement();
        }

        public void Reset()
        {
            ((IMeasureable)_mCore).Reset();
        }

        public string Name
        {
            get { return _mCore.Name; }
        }

        public double Efficiency
        {
            get { return _mCore.Efficiency; }
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
