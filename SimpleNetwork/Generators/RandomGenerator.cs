using System;
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
        public bool Measurering { get; private set; }
        public Interfaces.ITimeSeries TimeSeries { get; private set; }

        public RandomGenerator(string name, double generation)
        {
            Name = name;
            _mGeneration = generation;
        }

        public double GetProduction(int tick)
        {
            var prod = _mRand.Next(0, (int)_mGeneration * 2);
            if (Measurering) TimeSeries.AddData(tick, prod);
            return prod;
        }

        public void StartMeasurement()
        {
            TimeSeries = new SparseTimeSeries(Name);
            Measurering = true;
        }

        public void Reset()
        {
            TimeSeries = null;
            Measurering = false;
        }
    }
}
