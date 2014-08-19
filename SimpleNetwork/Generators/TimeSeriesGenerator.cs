using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.Generators
{
    /// <summary>
    /// Generator which an amount of energy specified by a time series.
    /// </summary>
    public class TimeSeriesGenerator : IGenerator
    {
        private readonly ITimeSeries _mTimeSeries;

        private bool _mMeasurering;
        public bool Measurering
        {
            get { return _mMeasurering; }
        }

        public ITimeSeries TimeSeries { get; private set; }

        public string Name { get; private set; }

        public TimeSeriesGenerator(string name, ITimeSeries ts)
        {
            Name = name;
            _mTimeSeries = ts;
        }

        public double GetProduction(int tick)
        {
            var prod = _mTimeSeries.GetValue(tick);
            if (_mMeasurering) TimeSeries.AddData(tick, prod);
            return prod;
        }

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
}
