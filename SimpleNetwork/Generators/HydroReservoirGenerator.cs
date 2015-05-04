using System;
using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;

namespace BusinessLogic.Generators
{
    public class HydroReservoirGenerator : IGenerator
    {

        private readonly BasicStorage _mInternalReservoir;
        private readonly ITimeSeries _mInflowPattern;
        private readonly double _mYearlyInflow;

        public IStorage InverseGenerator { get; private set; }
        public IStorage Pump { get; private set; }

        public HydroReservoirGenerator(double res, double cap, double pump, double yearlyInflow, ITimeSeries inflowPattern)
        {
            _mInternalReservoir = new BasicStorage("Internal reservoir", 1, res, res){Capacity = cap};
            _mInflowPattern = inflowPattern;
            _mYearlyInflow = yearlyInflow;

            InverseGenerator = new VirtualStorage(_mInternalReservoir, 1);
            Pump = new VirtualStorage(_mInternalReservoir, 0.9){Capacity = pump/0.9};
        }   

        public string Name
        {
            get { return "Hydro reservoir generator"; }
        }

        public double Production { get; set; }

        #region Measurement

        private DenseTimeSeries _mTimeSeries;
        private bool _mMeasuring;

        public bool Measuring { get { return _mMeasuring; } }

        public void Start(int ticks)
        {
            _mTimeSeries = new DenseTimeSeries(Name, ticks);
            _mMeasuring = true;
            // Propagate to internal reservoir.
            _mInternalReservoir.Start(ticks);
        }

        public void Clear()
        {
            _mTimeSeries = null;
            _mMeasuring = false;
            // Propagate to internal reservoir.
            _mInternalReservoir.Clear();
        }

        public void Sample(int tick)
        {
            if (!_mMeasuring) return;
            _mTimeSeries.AppendData(Production);
            // Propagate to internal reservoir.
            _mInternalReservoir.Sample(tick);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            var ts = _mInternalReservoir.CollectTimeSeries();
            ts.Add(_mTimeSeries);
            return ts;
        }

        #endregion

        public void TickChanged(int tick)
        {
            // Always produce AS MUCH as possible.
            Production = -_mInternalReservoir.AvailableEnergy(Response.Discharge);
            ((VirtualStorage)InverseGenerator).Capacity = Production;
            // Inject inflow minus production into the reservoir.
            if (_mInflowPattern == null) _mInternalReservoir.Inject(_mYearlyInflow/Stuff.HoursInYear - Production);
            else _mInternalReservoir.Inject(_mInflowPattern.GetValue(tick) * _mYearlyInflow - Production);
        }
    }
}
