using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;
using Utils;

namespace BusinessLogic.Nodes
{

    public class CountryNode : INode
    {

        public string Name { get { return Model.Name; } }
        public string Abbreviation { get { return CountryInfo.GetAbbrev(Name); } }

        public ReModel Model { get; set; }
        public Measureable Balancing { get; set; }
        public List<IGenerator> Generators { get; set; }
        public List<IStorage> Storages { get; set; }

        private double _mLoad { get; set; }

        public CountryNode(ReModel model)
        {
            Model = model;

            Balancing = new Measureable("Balancing");
            Balancing.Properties.Add("Country", Name);

            Generators = Model.GetGenerators();
            Storages = new List<IStorage>();
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
            foreach (var storage in Storages.Where(item => item.Measuring))
            {
                result.AddRange(storage.CollectTimeSeries());
            }
            // Bind country dependence.
            foreach (var ts in result) ts.Properties.Add("Country", Name);
            return result;
        }

        public double CurrentValue
        {
            get { return Balancing.CurrentValue; }
        }

        public double Curtailment
        {
            get { return Math.Max(0, Balancing.CurrentValue); }
        }

        public double Backup
        {
            get { return Math.Max(0, -Balancing.CurrentValue); }
        }

        public void Start(int ticks)
        {
            foreach (var generator in Generators) generator.Start(ticks);
            foreach (var storage in Storages) storage.Start(ticks);
            Measuring = true;
        }

        public void Clear()
        {
            foreach (var generator in Generators) generator.Clear();
            foreach (var storage in Storages) storage.Clear();
            Measuring = false;
        }

        public void Sample(int tick)
        {
            foreach (var generator in Generators) generator.Sample(tick);
            foreach (var storage in Storages) storage.Sample(tick);
        }

        #endregion

        public void TickChanged(int tick)
        {
            _mLoad = Model.LoadTimeSeries.GetValue(tick);
            foreach (var generator in Generators) generator.TickChanged(tick);
            foreach (var storage in Storages) storage.TickChanged(tick);
        }

    }

}
