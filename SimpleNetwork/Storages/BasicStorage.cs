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

        private double _mRemainingEnergy;

        public string Name { get; private set; }
        public double Efficiency { get; private set; }
        public double InitialEnergy { get; private set; }
        public double NominalEnergy { get; private set; }
        public double Capacity { get; set; }

        public BasicStorage(string name, double efficiency, double nominalEnergy, double initialEnergy = 0)
        {
            Name = name;
            NominalEnergy = nominalEnergy;
            Efficiency = efficiency;
            InitialEnergy = initialEnergy;
            Capacity = double.PositiveInfinity;

            _mRemainingEnergy = initialEnergy;
        }

        #region Energy info

        /// <summary>
        /// Minus means energy TO RELASE, plus means energy to be stored.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public double RemainingEnergy(Response response)
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

        /// <summary>
        /// Minus means energy TO RELASE, plus means energy to be stored.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public double AvailableEnergy(Response response)
        {
            switch (response)
            {
                case Response.Charge:
                    return Math.Min(Capacity, EffectiveEnergyNeededToRestoreFullCapacity());
                case Response.Discharge:
                    return Math.Max(-Capacity,-EffectiveEnergyReleasedOnDrainEmpty());
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }

        private double EffectiveEnergyReleasedOnDrainEmpty()
        {
            return _mRemainingEnergy * Efficiency;
        }

        private double EffectiveEnergyNeededToRestoreFullCapacity()
        {
            return (NominalEnergy - _mRemainingEnergy) / Efficiency;
        }

        #endregion

        #region Charge/discharge

        public double Inject(double amount)
        {
            if (amount > 0) return Charge(amount);
            return -Discharge(-amount);
        }

        public double InjectMax(Response response)
        {
            var max = (response == Response.Charge)
                ? EffectiveEnergyNeededToRestoreFullCapacity()
                : - EffectiveEnergyReleasedOnDrainEmpty();
            var remainder = Inject(max);
            return -(max - remainder);
        }

        /// <summary>
        /// Discharge the battery. Returns how much energy is still needed (that is NOT discharged).
        /// </summary>
        /// <param name="toDischarge"></param>
        /// <returns> the energy which was not stored </returns>
        private double Discharge(double toDischarge)
        {
            var extra = 0.0;
            // Take capacity into account.
            if (toDischarge > Capacity)
            {
                extra = toDischarge - Capacity;
                toDischarge = Capacity;
            }
            // Take efficiency into account.
            toDischarge /= Efficiency;
            if ((_mRemainingEnergy - toDischarge) < 0)
            {
                var remainder = toDischarge - _mRemainingEnergy;
                _mRemainingEnergy = 0;
                return (remainder*Efficiency) + extra;
            }
            // There is power; discharge.
            _mRemainingEnergy -= toDischarge;
            return extra;
        }

        /// <summary>
        /// Charge the battery. Returns how much energy is left (that is NOT stored).
        /// </summary>
        /// <param name="toCharge"></param>
        /// <returns> the energy which was not stored </returns>
        private double Charge(double toCharge)
        {
            var extra = 0.0;
            // Take capacity into account.
            if (toCharge > Capacity)
            {
                extra = toCharge - Capacity;             
                toCharge = Capacity;
            }
            // Take efficiency into account.
            toCharge *= Efficiency;
            if ((_mRemainingEnergy + toCharge) > NominalEnergy)
            {
                var excess = (toCharge + _mRemainingEnergy) - NominalEnergy;
                _mRemainingEnergy = NominalEnergy;
                return excess/Efficiency + extra;
            }
            // There is still room; just charge.
            _mRemainingEnergy += toCharge;
            return extra;
        }

        public void ResetEnergy()
        {
            _mRemainingEnergy = InitialEnergy;
        }

        #endregion

        #region Measurement

        private DenseTimeSeries _mTimeSeries;
        private bool _mMeasuring;

        public bool Measuring { get { return _mMeasuring; } }

        public void Start(int ticks)
        {
            _mTimeSeries = new DenseTimeSeries(Name, ticks);
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
            _mTimeSeries.AppendData(_mRemainingEnergy);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries> { _mTimeSeries };
        }

        #endregion

    }

}
