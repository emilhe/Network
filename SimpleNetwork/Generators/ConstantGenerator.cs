using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using DataItems.TimeSeries;
using SimpleNetwork.Interfaces;
using ITimeSeries = SimpleNetwork.Interfaces.ITimeSeries;

namespace SimpleNetwork.Generators
{
    class ConstantGenerator : IGenerator
    {
        private bool _mMeasurering;
        public bool Measurering
        {
            get { return _mMeasurering; }
        }

        public string Name { get; private set; }
        private readonly double _mGeneration;

        public ITimeSeries TimeSeries { get; private set; }

        public ConstantGenerator(string name, double generation)
        {
            Name = name;
            _mGeneration = generation;
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

        public double GetProduction(int tick)
        {
            // For now, the hyrdo energy production is "linear".
            var prod = _mGeneration/(365.25*24);
            if (_mMeasurering) TimeSeries.AddData(tick, prod);
            return prod;
        }
    }
}
