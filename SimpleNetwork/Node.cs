using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using Utils;

namespace BusinessLogic
{

    public class Node : IMeasureableNode
    {
        public string CountryName { get; set; }
        public string Abbreviation { get { return CountryInfo.GetAbbrev(CountryName); } }
        public ITimeSeries LoadTimeSeries { get; private set; }
        public StorageCollection StorageCollection { get; set; }
        public List<IGenerator> Generators { get; set; }

        public Node(string name, ITimeSeries loadTimeSeries)
        {
            CountryName = name;
            LoadTimeSeries = loadTimeSeries;

            Generators = new List<IGenerator>();
            StorageCollection = new StorageCollection();
            StorageCollection.Add(new Curtailment());
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            var result = new List<ITimeSeries>();
            result.AddRange(Generators.Select(item => item.TimeSeries));
            result.AddRange(StorageCollection.Storages().Select(item => item.TimeSeries));
            // Bind country dependence.
            foreach (var ts in result) ts.Properties.Add("Country", CountryName);
            return result;
        }

        public double GetDelta(int tick)
        {
            return Generators.Sum(generator => generator.GetProduction(tick)) - GetLoad(tick);
        }

        private double GetLoad(int tick)
        {
            return LoadTimeSeries.GetValue(tick);
        }

        public void StartMeasurement()
        {
            foreach (var gen in Generators) gen.StartMeasurement();
            foreach (var sto in StorageCollection.Storages()) sto.StartMeasurement();
        }

        public void Reset()
        {
            foreach (var gen in Generators) gen.Reset();
            foreach (var sto in StorageCollection.Storages()) sto.Reset();
        }

        public bool Measurering
        {
            get
            {
                if (Generators.Any()) return Generators[0].Measurering;
                if (StorageCollection.Storages().Any()) return StorageCollection.Storages().First().Measurering;

                return false;
            }
        }

    }

}
