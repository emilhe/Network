using System;
using DataItems;
using DataItems.TimeSeries;
using SimpleNetwork.Interfaces;
using ITimeSeries = SimpleNetwork.Interfaces.ITimeSeries;

namespace SimpleNetwork.Generators
{
    /// <summary>
    /// Generator which generates a random amount of energy (for test purposes).
    /// </summary>
    public class RandomGenerator : IGenerator
    {
        private readonly Random _mRand = new Random(DateTime.Now.Millisecond);

        private bool _mMeasurering;
        public bool Measurering
        {
            get { return _mMeasurering; }
        }

        public ITimeSeries TimeSeries { get; private set; }

        public string Name { get; private set; }

        private readonly double _mGeneration;

        public RandomGenerator(string name, double generation)
        {
            Name = name;
            _mGeneration = generation;
        }

        public double GetProduction(int tick)
        {
            var prod = _mRand.Next(0, (int)_mGeneration * 2);
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
