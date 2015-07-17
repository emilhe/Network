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

        public HydroReservoirGenerator(double yearlyInflow, ITimeSeries inflowPattern, BasicStorage reservoir)
        {
            _mInternalReservoir = reservoir;
            _mInflowPattern = inflowPattern;
            _mYearlyInflow = yearlyInflow;
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
        }

        public void Clear()
        {
            _mTimeSeries = null;
            _mMeasuring = false;
        }

        public void Sample(int tick)
        {
            if (!_mMeasuring) return;
            _mTimeSeries.AppendData(Production);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries> { _mTimeSeries };
        }

        #endregion

        public void TickChanged(int tick)
        {
            // Always produce AS MUCH as possible.
            Production = -_mInternalReservoir.AvailableEnergy(Response.Discharge);
            // Inject inflow minus production into the reservoir.
            if (_mInflowPattern == null) _mInternalReservoir.Inject(_mYearlyInflow/Stuff.HoursInYear - Production);
            else _mInternalReservoir.Inject(_mInflowPattern.GetValue(tick) * _mYearlyInflow - Production);
        }
    }
}
