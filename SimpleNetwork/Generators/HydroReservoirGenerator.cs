﻿using System;
using System.Collections.Generic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Utils;

namespace BusinessLogic.Generators
{
    public class HydroReservoirGenerator : IGenerator
    {

        private readonly BasicStorage _mInternalReservoir;
        private readonly double[] _mInflowPattern;

        public IStorage InverseGenerator { get; private set; }
        public IStorage Pump { get; private set; }

        public HydroReservoirGenerator(HydroInfo info)
        {
            _mInternalReservoir = new BasicStorage("Internal reservoir", 1, info.ReservoirCapacity*1e3,
                info.ReservoirCapacity*1e3*0.5) {Capacity = info.Capacity/1000};
            _mInflowPattern = info.InflowPattern;

            const double pumpEff = 0.8;
            InverseGenerator = new VirtualStorage(_mInternalReservoir, 1);
            Pump = new VirtualStorage(_mInternalReservoir, pumpEff) {Capacity = info.PumpCapacity/pumpEff/1000, Name = "Pump"};
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
            Production = -_mInternalReservoir.AvailableEnergy(Response.Discharge) *
                         Math.Pow(_mInternalReservoir.ChargeLevel, UncSyncScheme.Power);
            ((VirtualStorage)InverseGenerator).Capacity = Production;
            // The inflow pattern is in GWh/day.
            var day = Math.Ceiling((double)tick%Stuff.HoursInYear/24)%365;
            _mInternalReservoir.InternalInject(_mInflowPattern[(int)day]/24 - Production, 1, double.PositiveInfinity);
        }
    }
}
