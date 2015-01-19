using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Generators;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.Nodes
{
    public class SuperNode : INode
    {

        public string Name { get; set; }
        public string Abbreviation { get { return "Super"; } }

        public List<INode> Children { get; private set; }

        public StorageCollection StorageCollection { get; private set; }
        public List<IGenerator> Generators { get; private set; }

        public SuperNode(List<CountryNode> children, string name) : this(children.Select(item => (INode)item).ToList(), name) { }

        public SuperNode(List<INode> children, string name)
        {
            Name = name;
            Children = children;

            // So far ALL is generation aggregated; might change in the future.
            var allGenerators = children.SelectMany(item => item.Generators).ToList();
            Generators = new List<IGenerator> {new SuperGenerator(allGenerators, "All Generation")};
            // So far ALL storage is aggregated.           
            StorageCollection = new StorageCollection();
            foreach (var pair in Children.SelectMany(item => item.StorageCollection))
            {
                StorageCollection.Add(pair.Value);
            }
        }

        public double GetDelta()
        {
            return Children.Select(item => item.GetDelta()).Sum();
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
            return result;
        }

        public void Start(int ticks)
        {
            foreach (var generator in Generators) generator.Start(ticks);
            foreach (var item in StorageCollection) item.Value.Start(ticks);
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
            foreach (var generator in Generators) generator.TickChanged(tick);
            //foreach (var item in StorageCollection) item.Value.TickChanged(tick);
        }

    }
}
