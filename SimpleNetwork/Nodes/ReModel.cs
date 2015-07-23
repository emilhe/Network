using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Generators;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.Nodes
{
    public class ReModel
    {

        public string Name { get; private set; }
        public double AvgLoad { get { return _mAvgLoad; } }
        //public double AvgDeficit { get { return _mAvgLoad - AvgProduction; } }
        //public double AvgProduction { get; set; }
        public int Count { get { return LoadTimeSeries.Count; } }
        public Dictionary<string, double> AvgProduction { get; set; } 

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

        public double OffshoreFraction
        {
            get { return _mOffshoreFraction; }
            set
            {
                _mOffshoreFraction = value;
                UpdateScaling();
            }
        }

        public ITimeSeries LoadTimeSeries { get; private set; }
        public ITimeSeries OnshoreWindTimeSeries { get; private set; }
        public ITimeSeries OffshoreWindTimeSeries { get; private set; }
        public ITimeSeries SolarTimeSeries { get; private set; }

        private double _mAlpha;
        private double _mGamma;
        private double _mOffshoreFraction;
        private readonly double _mAvgLoad;

        public ReModel(string name, ITimeSeries load, ITimeSeries solar, ITimeSeries onshoreWind, ITimeSeries offshoreWind = null)
        {
            Name = name;
            LoadTimeSeries = load;
            OnshoreWindTimeSeries = onshoreWind;
            OffshoreWindTimeSeries = offshoreWind;
            SolarTimeSeries = solar;

            _mAvgLoad = LoadTimeSeries.GetAverage();
            AvgProduction = new Dictionary<string, double>();

            Alpha = 0.5;
            Gamma = 1.0;
        }

        public List<IGenerator> GetGenerators()
        {
            var result = new List<IGenerator>
            {
                new TimeSeriesGenerator("Solar", SolarTimeSeries),
                new TimeSeriesGenerator("Onshore wind", OnshoreWindTimeSeries)
            };

            // Add offshore ts only if a valid ts is present (some countries have no offshore regions).
            if (OffshoreWindTimeSeries == null || OffshoreWindTimeSeries.Count < 10) return result;

            result.Add(new TimeSeriesGenerator("Offshore wind", OffshoreWindTimeSeries));
            return result;
        }

        public void SetOffset(int ticks)
        {
            LoadTimeSeries.SetOffset(ticks);
            OnshoreWindTimeSeries.SetOffset(ticks);
            if (OffshoreWindTimeSeries != null) OffshoreWindTimeSeries.SetOffset(ticks);
            SolarTimeSeries.SetOffset(ticks);
        }

        private void UpdateScaling()
        {
            var totalProd = _mGamma*_mAvgLoad;
            var scale = totalProd - AvgProduction.Values.Sum();
            OnshoreWindTimeSeries.SetScale(_mAlpha * scale * (1 - OffshoreFraction));
            if (OffshoreWindTimeSeries != null) OffshoreWindTimeSeries.SetScale(_mAlpha * scale * OffshoreFraction);
            SolarTimeSeries.SetScale((1 - _mAlpha) * scale);
        }

    }
}
