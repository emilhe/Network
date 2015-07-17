using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.Utils;

namespace BusinessLogic.Storages
{
    public class SimpleHydroReservoirStorage : IStorage
    {

        private readonly BasicStorage _mCore;
        private readonly ITimeSeries _mInflowPattern;
        private readonly double _mYearlyInflow;

        public SimpleHydroReservoirStorage(double resSize, ITimeSeries inflowPattern, double yearlyInflow)
        {
            // Initial filling level is assumed = 70%
            _mCore = new BasicStorage("Hydro reservoir", 1, resSize, 0.7*resSize);
            _mInflowPattern = inflowPattern;
            _mYearlyInflow = yearlyInflow;
        }

        public bool Measuring
        {
            get { return _mCore.Measuring; }
        }

        public void Start(int ticks)
        {
            ((IMeasureable) _mCore).Start(ticks);
        }

        public void Clear()
        {
            ((IMeasureable) _mCore).Clear();
        }

        public void Sample(int tick)
        {
            ((IMeasureable) _mCore).Sample(tick);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return ((IMeasureable) _mCore).CollectTimeSeries();
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
            return ((IStorage) _mCore).Inject(amount);
        }

        public double InjectMax(Response response)
        {
            return ((IStorage) _mCore).InjectMax(response);
        }

        public double RemainingEnergy(Response response)
        {
            return ((IStorage) _mCore).RemainingEnergy(response);
        }

        public double AvailableEnergy(Response response)
        {
            return ((IStorage) _mCore).AvailableEnergy(response);
        }

        public void ResetEnergy()
        {
            ((IStorage) _mCore).ResetEnergy();
        }

        public double Capacity
        {
            get { return _mCore.Capacity; }
            set { _mCore.Capacity = value; }
        }

        public void TickChanged(int tick)
        {
            var weight = (_mInflowPattern == null)? 1.0/Stuff.HoursInYear : _mInflowPattern.GetValue(tick);
            _mCore.Inject(weight*_mYearlyInflow);
        }

    }
}