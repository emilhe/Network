using System;
using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Generators
{
    /// <summary>
    /// Generator which generates a random amount of energy (for test purposes).
    /// </summary>
    public class RandomGenerator : IGenerator
    {
        private readonly Random _mRand = new Random(DateTime.Now.Millisecond);
        private readonly double _mGeneration;

        public string Name { get; private set; }

        public double Production { get; private set; }

        public RandomGenerator(string name, double generation)
        {
            Name = name;
            _mGeneration = generation;
            _mTimeSeries = new DenseTimeSeries(name);

            Production = _mRand.Next(0, (int)_mGeneration * 2);
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


        public void TickChanged(int tick)
        {
            Production = _mRand.Next(0, (int)_mGeneration * 2);            
        }

    }
}
