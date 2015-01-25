using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Generators
{
    class ConstantGenerator : IGenerator
    {
        public string Name { get; private set; }

        public double Production
        {
            // The energy production is constant.
            get { return _mGeneration/(8766); }
        }

        private readonly double _mGeneration;

        public ConstantGenerator(string name, double generation)
        {
            Name = name;
            _mGeneration = generation;
        }

        #region Measurement
        
        private ITimeSeries _mTimeSeries;
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
            return new List<ITimeSeries>{_mTimeSeries};
        }

        #endregion

        public void TickChanged(int tick)
        {
            // Do nothing.
        }

    }
}
