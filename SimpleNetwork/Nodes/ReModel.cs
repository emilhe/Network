using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.Nodes
{
    public class ReModel
    {

        public string Name { get; private set; }
        public double AvgLoad { get { return _mAvgLoad; } }
        public int Count { get { return LoadTimeSeries.Count; } }

        public double Alpha
        {
            get { return _mAlpha; }
            set
            {
                _mAlpha = value;
                UpdateScaling();
            }
        }

        public double Gamma
        {
            get { return _mGamma; }
            set
            {
                _mGamma = value;
                UpdateScaling();
            }
        }

        public DenseTimeSeries LoadTimeSeries { get; private set; }
        public DenseTimeSeries WindTimeSeries { get; private set; }
        public DenseTimeSeries SolarTimeSeries { get; private set; }

        private double _mAlpha;
        private double _mGamma;
        private readonly double _mAvgLoad;

        public ReModel(string name, DenseTimeSeries load, DenseTimeSeries wind, DenseTimeSeries solar)
        {
            Name = name;
            LoadTimeSeries = load;
            WindTimeSeries = wind;
            SolarTimeSeries = solar;

            _mAvgLoad = LoadTimeSeries.Average(item => item.Value);

            Alpha = 0.5;
            Gamma = 1.0;
        }

        public void SetOffset(int ticks)
        {
            LoadTimeSeries.SetOffset(ticks);
            WindTimeSeries.SetOffset(ticks);
            SolarTimeSeries.SetOffset(ticks);
        }

        private void UpdateScaling()
        {
            WindTimeSeries.SetScale(_mAlpha*_mAvgLoad*_mGamma);
            SolarTimeSeries.SetScale((1 - _mAlpha)*_mAvgLoad*_mGamma);
        }

    }
}
