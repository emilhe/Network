using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using Utils;

namespace BusinessLogic
{

    public class Node : IMeasureable, ITickListener
    {

        public string CountryName { get; set; }
        public string Abbreviation { get { return CountryInfo.GetAbbrev(CountryName); } }
        public ITimeSeries LoadTimeSeries { get; private set; }
        public StorageCollection StorageCollection { get; set; }
        public List<IGenerator> Generators { get; set; }

        public double Load { get; private set; }
            
        public Node(string name, ITimeSeries loadTimeSeries)
        {
            CountryName = name;
            LoadTimeSeries = loadTimeSeries;

            Generators = new List<IGenerator>();
            StorageCollection = new StorageCollection {new Curtailment()};
        }

        public double GetDelta()
        {
            return Generators.Sum(generator => generator.Production) - Load;
        }

        #region Measurement

        public bool Measuring { get; private set; }

        public List<ITimeSeries> CollectTimeSeries()
        {
            var result = new List<ITimeSeries>();
            foreach (var generator in Generators.Where(item => item.Measuring))
            {
                result.AddRange(generator.CollectTimeSeries());
            }
            foreach (var item in StorageCollection.Where(item => item.Value.Measuring))
            {
                result.AddRange(item.Value.CollectTimeSeries());
            }
            // Bind country dependence.
            foreach (var ts in result) ts.Properties.Add("Country", CountryName);
            return result;
        }

        public void Start()
        {
            foreach (var generator in Generators) generator.Start();
            foreach (var item in StorageCollection) item.Value.Start();
            Measuring = true;
        }

        public void Clear()
        {
            foreach (var generator in Generators) generator.Clear();
            foreach (var item in StorageCollection) item.Value.Clear();
            Measuring = false;
        }

        public void Sample(int tick)
        {
            foreach (var generator in Generators) generator.Sample(tick);
            foreach (var item in StorageCollection) item.Value.Sample(tick);
        }

        #endregion

        public void TickChanged(int tick)
        {
            Load = LoadTimeSeries.GetValue(tick);
            foreach (var generator in Generators) generator.TickChanged(tick);
            //foreach (var item in StorageCollection) item.Value.TickChanged(tick);
        }
    }

}
