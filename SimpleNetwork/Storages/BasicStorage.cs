using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using DataItems.TimeSeries;
using SimpleNetwork.Interfaces;
using ITimeSeries = SimpleNetwork.Interfaces.ITimeSeries;

namespace SimpleNetwork.Storages
{
    /// <summary>
    /// Storage (rechargeable).
    /// </summary>
    public class BasicStorage : IStorage
    {
        private bool _mMeasurering;
        private double _mRemainingCapacity;

        public bool Measurering
        {
            get { return _mMeasurering; }
        }

        public ITimeSeries TimeSeries { get; private set; }

        public string Name { get; private set; }
        public double Efficiency { get; private set; }
        public double InitialCapacity { get; private set; }
        public double Capacity { get; private set; }

        public BasicStorage(string name, double efficiency, double capacity, double initialCapacity = 0)
        {
            Name = name;
            Capacity = capacity;
            Efficiency = efficiency;
            InitialCapacity = initialCapacity;

            _mRemainingCapacity = initialCapacity;
        }

        #region Capacity info

        /// <summary>
        /// Minus means energy TO RELASE, plus means energy to be stored.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public double RemainingCapacity(Response response)
        {
            switch (response)
            {
                case Response.Charge:
                    return EffectiveEnergyNeededToRestoreFullCapacity();
                case Response.Discharge:
                    return -EffectiveEnergyReleasedOnDrainEmpty();
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }

        public void ResetCapacity()
        {
            _mRemainingCapacity = InitialCapacity;
        }

        private double EffectiveEnergyReleasedOnDrainEmpty()
        {
            return _mRemainingCapacity * Efficiency;
        }

        private double EffectiveEnergyNeededToRestoreFullCapacity()
        {
            return (Capacity - _mRemainingCapacity) / Efficiency;
        }

        #endregion

        #region Charge/discharge

        public double Inject(int tick, double amount)
        {
            if (amount > 0) return Charge(tick, amount);
            return -Discharge(tick, -amount);
        }

        public double Restore(int tick, Response response)
        {
            if (response == Response.Charge)
            {
                var cost = EffectiveEnergyNeededToRestoreFullCapacity();
                Restore(tick);
                return -cost;
            }
            else
            {
                var cost = EffectiveEnergyReleasedOnDrainEmpty();
                Drain(tick);
                return cost;
            }
        }

        /// <summary>
        /// Discharge the battery. Returns how much energy is still needed (that is NOT discharged).
        /// </summary>
        /// <param name="tick"> tick </param>
        /// <param name="toDischarge"></param>
        /// <returns> the energy which was not stored </returns>
        private double Discharge(int tick, double toDischarge)
        {
            toDischarge /= Efficiency;
            if ((_mRemainingCapacity - toDischarge) < 0)
            {
                var remainder = toDischarge - _mRemainingCapacity;
                Drain(tick);
                return (remainder*Efficiency);
            }
            // There is power; discharge.
            _mRemainingCapacity -= toDischarge;
            if (_mMeasurering) TimeSeries.AddData(tick, _mRemainingCapacity);
            return 0;
        }

        /// <summary>
        /// Charge the battery. Returns how much energy is left (that is NOT stored).
        /// </summary>
        /// <param name="tick"> tick </param>
        /// <param name="toCharge"></param>
        /// <returns> the energy which was not stored </returns>
        private double Charge(int tick, double toCharge)
        {
            toCharge *= Efficiency;
            if ((_mRemainingCapacity + toCharge) > Capacity)
            {
                var excess = (toCharge + _mRemainingCapacity) - Capacity;
                Restore(tick);
                return excess/Efficiency;
            }
            // There is still room; just charge.
            _mRemainingCapacity += toCharge;
            if (_mMeasurering) TimeSeries.AddData(tick, _mRemainingCapacity);
            return 0;
        }

        private void Restore(int tick)
        {
            _mRemainingCapacity = Capacity;
            if (_mMeasurering) TimeSeries.AddData(tick, _mRemainingCapacity);
        }

        private void Drain(int tick)
        {
            _mRemainingCapacity = 0;
            if (_mMeasurering) TimeSeries.AddData(tick, _mRemainingCapacity);
        }

        #endregion

        public void StartMeasurement()
        {
            TimeSeries = new SparseTimeSeries(Name);
            _mMeasurering = true;
        }

        public void Reset()
        {
            TimeSeries = null;
            _mMeasurering = false;
        }

    }
}
