using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.Storages
{
    public class HydroReservoirGenerator : IGenerator
    {

        private readonly BasicStorage _mInternalReservoir;
        private readonly ITimeSeries _mInflowPattern;
        private readonly double _mGeneratorCapacity;
        private readonly double _mYearlyInflow;

        public HydroReservoirGenerator(double generatorCapacity, double yearlyInflow, ITimeSeries inflowPattern, BasicStorage reservoir)
        {
            _mGeneratorCapacity = generatorCapacity;
            _mInternalReservoir = reservoir;
            _mInflowPattern = inflowPattern;
            _mYearlyInflow = yearlyInflow;

            Production = Math.Min(_mGeneratorCapacity, _mInternalReservoir.RemainingCapacity(Response.Discharge));
            _mInternalReservoir.Inject(_mInflowPattern.GetValue(0) * _mYearlyInflow - Production);
        }   

        public string Name
        {
            get { return "Hydro reservoir"; }
        }

        public double Production { get; set; }

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
            Production = Math.Min(_mGeneratorCapacity, _mInternalReservoir.RemainingCapacity(Response.Discharge));
            // Inject inflow minus production into the reservoir.
            _mInternalReservoir.Inject(_mInflowPattern.GetValue(tick) * _mYearlyInflow - Production);
        }
    }
}
