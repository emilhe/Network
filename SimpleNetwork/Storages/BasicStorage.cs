using System;
using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Storages
{
    /// <summary>
    /// Storage (rechargeable).
    /// </summary>
    public class BasicStorage : IStorage
    {

        private double _mRemainingCapacity;

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

            LimitIn = double.PositiveInfinity;
            LimitOut = double.NegativeInfinity;

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

        public double LimitIn { get; set; }
        public double LimitOut { get; set; }

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

        public double Inject(double amount)
        {
            if (amount > 0) return Charge(amount);
            return -Discharge(-amount);
        }

        public double Restore(Response response)
        {
            if (response == Response.Charge)
            {
                var cost = EffectiveEnergyNeededToRestoreFullCapacity();
                Refill();
                return -cost;
            }
            else
            {
                var cost = EffectiveEnergyReleasedOnDrainEmpty();
                Drain();
                return cost;
            }
        }

        /// <summary>
        /// Discharge the battery. Returns how much energy is still needed (that is NOT discharged).
        /// </summary>
        /// <param name="toDischarge"></param>
        /// <returns> the energy which was not stored </returns>
        private double Discharge(double toDischarge)
        {
            toDischarge /= Efficiency;
            if ((_mRemainingCapacity - toDischarge) < 0)
            {
                var remainder = toDischarge - _mRemainingCapacity;
                Drain();
                return (remainder*Efficiency);
            }
            // There is power; discharge.
            _mRemainingCapacity -= toDischarge;
            return 0;
        }

        /// <summary>
        /// Charge the battery. Returns how much energy is left (that is NOT stored).
        /// </summary>
        /// <param name="toCharge"></param>
        /// <returns> the energy which was not stored </returns>
        private double Charge(double toCharge)
        {
            toCharge *= Efficiency;
            if ((_mRemainingCapacity + toCharge) > Capacity)
            {
                var excess = (toCharge + _mRemainingCapacity) - Capacity;
                Refill();
                return excess/Efficiency;
            }
            // There is still room; just charge.
            _mRemainingCapacity += toCharge;
            return 0;
        }

        // TODO: Fix refill/drain problems..

        private void Refill()
        {
            _mRemainingCapacity = Capacity;
        }

        private void Drain()
        {
            _mRemainingCapacity = 0;
        }

        #endregion

        #region Measurement

        private ITimeSeries _mTimeSeries;
        private bool _mMeasuring;

        public bool Measuring { get { return _mMeasuring; } }

        public void Start()
        {
            _mTimeSeries = new DenseTimeSeries(Name);
            _mMeasuring = true;
        }

        public void Clear()
        {
            _mTimeSeries = null;
            _mMeasuring = false;
        }

        public void Sample(int tick)
        {
            if (!_mMeasuring) return;
            _mTimeSeries.AppendData(_mRemainingCapacity);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries> { _mTimeSeries };
        }

        #endregion

    }

}
