using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Generators
{
    /// <summary>
    /// Generator which an amount of energy specified by a time series.
    /// </summary>
    public class TimeSeriesGenerator : IGenerator
    {
        private readonly ITimeSeries _mTimeSeries;

        public bool Measurering { get; private set; }

        public ITimeSeries TimeSeries { get; private set; }
        public ITimeSeries UnderlyingTimeSeries { get { return _mTimeSeries; } }

        public string Name { get; private set; }

        public TimeSeriesGenerator(string name, ITimeSeries ts)
        {
            Name = name;
            _mTimeSeries = ts;
        }

        public double GetProduction(int tick)
        {
            var prod = _mTimeSeries.GetValue(tick);
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
