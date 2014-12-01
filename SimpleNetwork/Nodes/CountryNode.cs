using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Generators;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using Utils;

namespace BusinessLogic.Nodes
{

    public class CountryNode : INode
    {

        public string Name { get { return Model.Name; } }
        public string Abbreviation { get { return CountryInfo.GetAbbrev(Name); } }

        public ReModel Model { get; set; }
        public List<IGenerator> Generators { get; set; }
        public StorageCollection StorageCollection { get; set; }

        private double _mLoad { get; set; }
    
        public CountryNode(ReModel model)
        {
            Model = model;

            Generators = new List<IGenerator>
            {
                new TimeSeriesGenerator("Wind",Model.WindTimeSeries),
                new TimeSeriesGenerator("Solar",Model.SolarTimeSeries)
            };
            StorageCollection = new StorageCollection {new Curtailment()};
        }

        public double GetDelta()
        {
            return Generators.Sum(generator => generator.Production) - _mLoad;
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
            foreach (var ts in result) ts.Properties.Add("Country", Name);
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
            _mLoad = Model.LoadTimeSeries.GetValue(tick);
            foreach (var generator in Generators) generator.TickChanged(tick);
            //foreach (var item in StorageCollection) item.Value.TickChanged(tick);
        }

    }

}
