using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
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

        public double ChargeLevel
        {
            get
            {
                if (NominalEnergy == 0) return 0;
                return _mRemainingEnergy/NominalEnergy;
            }
        }

        /// <summary>
        /// Minus means energy TO RELASE, plus means energy to be stored.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public double RemainingEnergy(Response response)
        {
            return InternalRemainingEnergy(response, Efficiency);
        }

        /// <summary>
        /// Minus means energy TO RELASE, plus means energy to be stored.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public double AvailableEnergy(Response response)
        {
            return InternalAvailableEnergy(response, Efficiency, Capacity);
        }

        public double InternalRemainingEnergy(Response response, double eff)
        {
            switch (response)
            {
                case Response.Charge:
                    return EffectiveEnergyNeededToRestoreFullCapacity(eff);
                case Response.Discharge:
                    return -EffectiveEnergyReleasedOnDrainEmpty(eff);
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }

        public double InternalAvailableEnergy(Response response, double eff, double cap)
        {
            switch (response)
            {
                case Response.Charge:
                    return Math.Min(cap, EffectiveEnergyNeededToRestoreFullCapacity(eff));
                case Response.Discharge:
                    return Math.Max(-cap, -EffectiveEnergyReleasedOnDrainEmpty(eff));
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }

        private double EffectiveEnergyReleasedOnDrainEmpty(double eff)
        {
            return _mRemainingEnergy * eff;
        }

        private double EffectiveEnergyNeededToRestoreFullCapacity(double eff)
        {
            return (NominalEnergy - _mRemainingEnergy) / eff;
        }

        #endregion

        #region Charge/discharge

        public double Inject(double amount)
        {
            return InternalInject(amount, Efficiency, Capacity);
        }

        public double InjectMax(Response response)
        {
            return InternalInjectMax(response, Efficiency, Capacity);
        } 

        public double InternalInjectMax(Response response, double eff, double cap)
        {
            var max = (response == Response.Charge)
                ? EffectiveEnergyNeededToRestoreFullCapacity(eff)
                : -EffectiveEnergyReleasedOnDrainEmpty(eff);
            var remainder = InternalInject(max, eff, cap);
            return -(max - remainder);
        }

        public double InternalInject(double amount, double eff, double cap)
        {
            if (amount > 0) return Charge(amount, eff, cap);
            return -Discharge(-amount, eff, cap);
        }

        /// <summary>
        /// Discharge the battery. Returns how much energy is still needed (that is NOT discharged).
        /// </summary>
        /// <param name="toDischarge"></param>
        /// <param name="eff"></param>
        /// <returns> the energy which was not stored </returns>
        private double Discharge(double toDischarge, double eff, double cap)
        {
            var extra = 0.0;
            // Take capacity into account.
            if (toDischarge > cap)
            {
                extra = toDischarge - cap;
                toDischarge = cap;
            }
            // Take efficiency into account.
            toDischarge /= eff;
            if ((_mRemainingEnergy - toDischarge) < 0)
            {
                var remainder = toDischarge - _mRemainingEnergy;
                _mRemainingEnergy = 0;
                return (remainder * eff) + extra;
            }
            // There is power; discharge.
            _mRemainingEnergy -= toDischarge;
            return extra;
        }

        /// <summary>
        /// Charge the battery. Returns how much energy is left (that is NOT stored).
        /// </summary>
        /// <param name="toCharge"></param>
        /// <param name="eff"></param>
        /// <returns> the energy which was not stored </returns>
        private double Charge(double toCharge, double eff, double cap)
        {
            var extra = 0.0;
            // Take capacity into account.
            if (toCharge > cap)
            {
                extra = toCharge - cap;
                toCharge = cap;
            }
            // Take efficiency into account.
            toCharge *= eff;
            if ((_mRemainingEnergy + toCharge) > NominalEnergy)
            {
                var excess = (toCharge + _mRemainingEnergy) - NominalEnergy;
                _mRemainingEnergy = NominalEnergy;
                return excess / eff + extra;
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

        public void TickChanged(int tick)
        {
            // Do nothing..
        }
    }

}
