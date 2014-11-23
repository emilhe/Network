using System;
using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.Generators
{
    class SuperGenerator : IGenerator
    {

        public string Name { get; private set; }
        public double Production { get; private set; }

        public List<IGenerator> Children { get; set; } 

        public SuperGenerator(List<IGenerator> generators, string name)
        {
            Name = name;
            Children = generators;
        }

        public void TickChanged(int tick)
        {
            Production = 0;

            foreach (var child in Children)
            {
                child.TickChanged(tick);
                Production += child.Production;
            }
        }

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
            if (_mMeasuring) _mTimeSeries.AppendData(Production);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries> { _mTimeSeries };
        }

        #endregion

    }
}
