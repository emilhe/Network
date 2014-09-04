using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Generators
{
    class ConstantGenerator : IGenerator
    {
        public bool Measurering { get; private set; }

        public string Name { get; private set; }
        private readonly double _mGeneration;

        public Interfaces.ITimeSeries TimeSeries { get; private set; }

        public ConstantGenerator(string name, double generation)
        {
            Name = name;
            _mGeneration = generation;
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

        public double GetProduction(int tick)
        {
            // For now, the hyrdo energy production is "linear".
            var prod = _mGeneration/(8766);
            if (Measurering) TimeSeries.AddData(tick, prod);
            return prod;
        }
    }
}
