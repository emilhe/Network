using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;

namespace SimpleNetwork.Interfaces
{
    /// <summary>
    /// Power generation abstraction.
    /// </summary>
    public interface IGenerator : IMeasureable
    {
        string Name { get; }
        double GetProduction(int tick);
    }

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
            var prod = _mRand.Next(0, (int) _mGeneration*2);
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

    /// <summary>
    /// Generator which an amount of energy specified by a time series.
    /// </summary>
    public class TsGenerator : IGenerator
    {
        private readonly ITimeSeries _mTimeSeries;
        
        private bool _mMeasurering;
        public bool Measurering
        {
            get { return _mMeasurering; }
        }

        public ITimeSeries TimeSeries { get; private set; }

        public string Name { get; private set; }

        public TsGenerator(string name, ITimeSeries ts)
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
