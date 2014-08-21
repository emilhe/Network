using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using SimpleNetwork.Interfaces;
using ITimeSeries = SimpleNetwork.Interfaces.ITimeSeries;

namespace SimpleNetwork.Storages
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
