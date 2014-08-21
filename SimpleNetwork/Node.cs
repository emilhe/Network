using System;
using System.Collections.Generic;
using System.Linq;
using DataItems;
using SimpleNetwork.Interfaces;
using ITimeSeries = SimpleNetwork.Interfaces.ITimeSeries;

namespace SimpleNetwork
{

    public class Node
    {
        private readonly ITimeSeries _mLoadTimeSeries;

        public string CountryName { get; set; }

        public ITimeSeries LoadTimeSeries { get { return _mLoadTimeSeries; } }
        public Dictionary<int,IStorage> Storages { get; set; }
        public List<IGenerator> Generators { get; set; }
        public List<IMeasureable> Measureables
        {
            get
            {
                var result = new List<IMeasureable>();
                result.AddRange(Generators);
                result.AddRange(Storages.Values);
                return result;
            }
        }

        public Node(string name, ITimeSeries loadTimeSeries)
        {
            CountryName = name;
            _mLoadTimeSeries = loadTimeSeries;

            Generators = new List<IGenerator>();
            Storages = new Dictionary<int, IStorage>();
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return (from measureable in Measureables where measureable.Measurering select measureable.TimeSeries).ToList();
        }

        public double GetDelta(int tick)
        {
            return Generators.Sum(generator => generator.GetProduction(tick)) - GetLoad(tick);
        }

        private double GetLoad(int tick)
        {
            return _mLoadTimeSeries.GetValue(tick);
        }   

    }

}
