using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;

namespace BusinessLogic.Storages
{
    public class VirtualStorage : IStorage
    {

        private readonly BasicStorage _mCore;
        private readonly double _mEfficiency;

        public double Capacity { get; set; }

        public VirtualStorage(BasicStorage core, double eff)
        {
            _mCore = core;
            _mEfficiency = eff;

            Name = "Virtual storage";
        }

        public string Name { get; set; }

        public double Efficiency
        {
            get { return _mEfficiency; }
        }

        public double Inject(double amount)
        {
            // It is NOT possible to discharge the reservoir; the generator is already at max.
            if (amount <= 1e-5) return amount;
            return _mCore.InternalInject(amount, _mEfficiency, Capacity);
        }

        public double InjectMax(Response response)
        {
            // It is NOT possible to discharge the reservoir; the generator is already at max.
            if (response.Equals(Response.Discharge)) return 0;
            return _mCore.InternalInjectMax(response, _mEfficiency, Capacity);
        }

        public double RemainingEnergy(Response response)
        {
            // It is NOT possible to discharge the reservoir; the generator is already at max.
            if (response.Equals(Response.Discharge)) return 0;
            return _mCore.InternalRemainingEnergy(response, _mEfficiency);
        }

        public double AvailableEnergy(Response response)
        {
            // It is NOT possible to discharge the reservoir; the generator is already at max.
            if (response.Equals(Response.Discharge)) return 0;
            return _mCore.InternalAvailableEnergy(response, _mEfficiency, Capacity);
        }

        #region Delegation

        public double ChargeLevel
        {
            get { return _mCore.ChargeLevel; }
        }

        public double InitialEnergy
        {
            get { return _mCore.InitialEnergy; }
        }

        public double NominalEnergy
        {
            get { return _mCore.NominalEnergy; }
        }

        public void ResetEnergy()
        {
            ((IStorage)_mCore).ResetEnergy();
        }

        #endregion

        #region Nothing to measure/listen

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries>();
        }

        public bool Measuring
        {
            get { return false; }
        }

        public void Start(int ticks)
        {
        }

        public void Clear()
        {
        }

        public void Sample(int tick)
        {
        }

        public void TickChanged(int tick)
        {
        }

        #endregion

    }
}
