using System;
using DataItems;

namespace SimpleNetwork.Interfaces
{
    public enum Response
    {
        Charge,
        Discharge
    }

    /// <summary>
    /// Storage abstraction.
    /// </summary>
    public interface IStorage : IMeasureable
    {
        /// <summary>
        /// Name/description of the storage.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Efficiency of the storage; a value between 1 (optimal) and zero.
        /// </summary>
        double Efficiency { get; }

        /// <summary>
        /// Nominal capacity of the storage.
        /// </summary>
        double Capacity { get; }

        /// <summary>
        /// Injects an amount of energy (positive or negative) into the storage.
        /// </summary>
        /// <param name="tick"> tick </param>
        /// <param name="amount"> amount of energy </param>
        /// <returns> remaining energy </returns>
        double Inject(int tick, double amount);

        /// <summary>
        /// Restore (= fully charge/discharge) the storage.
        /// </summary>
        /// <param name="tick"> tick </param>
        /// <param name="response"> should we charge or discharge? </param>
        /// <returns> cost of discharge </returns>
        double Restore(int tick, Response response);

        /// <summary>
        /// Remaining capacity for a specific response; charge or dicharge.
        /// </summary>
        /// <param name="response"> needed backup response </param>
        /// <returns> remaining capacity </returns>
        double RemainingCapacity(Response response);
        
        /// <summary>
        /// Reset capacity (the be used when a new simulation is started).
        /// </summary>
        void ResetCapacity();
    }

    #region Rechargeable storage

    /// <summary>
    /// Storage (rechargeable).
    /// </summary>
    public class BasicStorage : IStorage
    {
        private bool _mMeasurering;
        private double _mRemainingCapacity;
        private readonly double _mInitialCapacity;

        public bool Measurering
        {
            get { return _mMeasurering; }
        }

        public ITimeSeries TimeSeries { get; private set; }

        public string Name { get; private set; }
        public double Efficiency { get; private set; }
        public double Capacity { get; private set; }

        public BasicStorage(string name, double efficiency, double capacity, double initialCapacity = 0)
        {
            Name = name;
            Capacity = capacity;
            Efficiency = efficiency;

            _mRemainingCapacity = initialCapacity;
            _mInitialCapacity = initialCapacity;
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
            _mRemainingCapacity = _mInitialCapacity;
        }

        private double EffectiveEnergyReleasedOnDrainEmpty()
        {
            return _mRemainingCapacity*Efficiency;
        }

        private double EffectiveEnergyNeededToRestoreFullCapacity()
        {
            return (Capacity - _mRemainingCapacity)/Efficiency;
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
                return remainder;
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
                return excess;
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
            ((IMeasureable) _mCore).StartMeasurement();
        }

        public void Reset()
        {
            ((IMeasureable) _mCore).Reset();
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
            return ((IStorage) _mCore).Inject(tick, amount);
        }

        public double Restore(int tick, Response response)
        {
            return ((IStorage) _mCore).Restore(tick, response);
        }

        public double RemainingCapacity(Response response)
        {
            return ((IStorage) _mCore).RemainingCapacity(response);
        }

        public void ResetCapacity()
        {
            ((IStorage) _mCore).ResetCapacity();
        }
    }

    /// <summary>
    /// Hydrogen storage model (efficiency = 0.6).
    /// </summary>
    public class HydrogenStorage : IStorage
    {

        private readonly BasicStorage _mCore;

        public HydrogenStorage(double capacity)
        {
            _mCore = new BasicStorage("Hydrogen storage", 0.6, capacity);
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
            ((IMeasureable) _mCore).StartMeasurement();
        }

        public void Reset()
        {
            ((IMeasureable) _mCore).Reset();
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
            return ((IStorage) _mCore).Inject(tick, amount);
        }

        public double Restore(int tick, Response response)
        {
            return ((IStorage) _mCore).Restore(tick, response);
        }

        public double RemainingCapacity(Response response)
        {
            return ((IStorage) _mCore).RemainingCapacity(response);
        }

        public void ResetCapacity()
        {
            ((IStorage) _mCore).ResetCapacity();
        }
    }

    #endregion

    #region Nonrechargeable storage

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
            ((IMeasureable) _mCore).StartMeasurement();
        }

        public void Reset()
        {
            ((IMeasureable) _mCore).Reset();
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
            return amount > 0 ? amount : ((IStorage) _mCore).Inject(tick, amount);
        }

        public double Restore(int tick, Response response)
        {
            // A backup cannot be charged.
            return (response == Response.Charge)? 0 : ((IStorage) _mCore).Restore(tick, response);
        }

        public double RemainingCapacity(Response response)
        {
            // A backup cannot be charged.
            return response == Response.Charge ? 0 : ((IStorage) _mCore).RemainingCapacity(response);
        }

        public void ResetCapacity()
        {
            ((IStorage) _mCore).ResetCapacity();
        }
    }

    /// <summary>
    /// Biomass backup model (efficiency = 1).
    /// </summary>
    public class HydroBiomassBackup : IStorage
    {
        private readonly BasicBackup _mCore;

        public HydroBiomassBackup(double capacity)
        {
            _mCore = new BasicBackup("Hydro-biomass backup", capacity);
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
            ((IMeasureable) _mCore).StartMeasurement();
        }

        public void Reset()
        {
            ((IMeasureable) _mCore).Reset();
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
            return ((IStorage) _mCore).Inject(tick, amount);
        }

        public double Restore(int tick, Response response)
        {
            return ((IStorage) _mCore).Restore(tick, response);
        }

        public double RemainingCapacity(Response response)
        {
            return ((IStorage) _mCore).RemainingCapacity(response);
        }

        public void ResetCapacity()
        {
            ((IStorage) _mCore).ResetCapacity();
        }
    }

    #endregion

}
