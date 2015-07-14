using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;

namespace BusinessLogic.Generators
{
    class BiomassGenerator : IGenerator
    {

        private readonly BasicStorage _mInternalReservoir;
        private readonly double _mHourlyInflow;

        public IStorage InverseGenerator { get; private set; }

        public BiomassGenerator(double cap, double inflow)
        {
            _mInternalReservoir = new BasicStorage("Internal biomass", 1, inflow, inflow * 0.5) { Capacity = cap / 1000 };
            _mHourlyInflow = inflow/8766;

            InverseGenerator = new VirtualStorage(_mInternalReservoir, 1);
        }

        public string Name
        {
            get { return "Biomass generator"; }
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
            Production = -_mInternalReservoir.AvailableEnergy(Response.Discharge)*
                         Math.Pow(_mInternalReservoir.ChargeLevel, UncSyncScheme.Power);
            ((VirtualStorage)InverseGenerator).Capacity = Production;
            _mInternalReservoir.InternalInject(_mHourlyInflow - Production, 1, double.PositiveInfinity);
        }

    }
}
