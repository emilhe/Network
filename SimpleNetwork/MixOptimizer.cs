using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using SimpleNetwork.Generators;
using SimpleNetwork.Interfaces;
using SimpleNetwork.Storages;

namespace SimpleNetwork
{
    public class MixOptimizer
    {
        private readonly List<Tuple<DenseTimeSeries, DenseTimeSeries, double>> _mProductionTuples;

        public List<Node> Nodes { get; set; }
        public double[] OptimalMix { get; set; }

        private static readonly double[] MixCache =
        {
            0.55, 0.55, 0.45, 0.55, 0.55, 0.5, 0.55, 0.65, .5, 0.65, 0.7, 0.55, 0.6, 0.45, 0.5, 0.5, 0.65, 0.5, 0.6, 0.5,
            0.6, 0.6, 0.75, 0.6, 0.6, 0.5, 0.5, 0.5, 0.55, 0.75
        };

        /// <summary>
        /// Construction. More/different constuctors to be added...
        /// </summary>
        /// <param name="data"> data to build nodes from </param>
        public MixOptimizer(List<CountryData> data)
        {
            Nodes = new List<Node>(data.Count);
            _mProductionTuples = new List<Tuple<DenseTimeSeries, DenseTimeSeries, double>>(data.Count);
            OptimalMix = new double[data.Count];

            foreach (var country in data)
            {
                var load = country.TimeSeries.Single(item => item.Name.Equals("Load"));
                var node = new Node(country.Abbreviation, load);
                var avgLoad = load.GetAverage();
                var wind = country.TimeSeries.Single(item => item.Name.Equals("Wind"));
                var solar = country.TimeSeries.Single(item => item.Name.Equals("Solar"));
                _mProductionTuples.Add(new Tuple<DenseTimeSeries, DenseTimeSeries, double>(wind, solar, avgLoad));

                node.PowerGenerators = new List<IGenerator>
                        {
                            new TimeSeriesGenerator("WindPower", wind),
                            new TimeSeriesGenerator("SolarPower", solar)
                        };
                node.Storages = new Dictionary<int, IStorage>
                {
                    {0, new BatteryStorage(6*avgLoad)}, // Fixed for now
                    {1, new HydrogenStorage(68.18*avgLoad)}, //  25TWh*(6hourLoad/2.2TWh) = 68.18; To be country dependent
                    {2, new BasicBackup("Hydro-biomass backup", 409.09*avgLoad)} // 150TWh*(6hourLoad/2.2TWh) = 409.09; To be country dependent
                };

                Nodes.Add(node);
            }
        }

        // TODO: TEST METHOD
        public void ReadMixCahce()
        {
            OptimalMix = MixCache;
        }

        /// <summary>
        /// Individual optimization of the mixing factor: The mixing factor will be optimal for each node considered in isolation.
        /// </summary>
        public void OptimizeIndividually(double stepSize = 0.05)
        {
            var exportStrategy = new DefaultExportStrategy(new List<Node> {Nodes[0]}, new EdgeSet(1));
            var system = new NetworkSystem(exportStrategy);
            // TODO: This can be done faster if the flow optimization is simply skipped!
            for (int i = 0; i < Nodes.Count; i++)
            {
                var best = 0.0  ;
                Nodes[i].Storages.Add(3, new BasicBackup("Test backup", 5000*_mProductionTuples[i].Item3));
                for (double mix = 0.3; mix < 0.9; mix += stepSize)
                {
                    SetMix(i, mix); 
                    system.Nodes = new List<Node> { Nodes[i] };
                    system.Simulate(8766); // One year.
                    var result =
                        system.Output.CountryTimeSeriesMap[Nodes[i].CountryName].Single(
                            item => item.Name.Equals("Test backup")).Last().Value;
                    if(result < best) continue;
                    // Wee have a new optimum, let's save it.
                    best = result;
                    OptimalMix[i] = mix;
                }
                Nodes[i].Storages.Remove(3);
                Console.WriteLine("Optimal mix for {0} is {1}.", Nodes[i].CountryName, OptimalMix[i]);
                SetMix(i, OptimalMix[i]);
            }
        }

        /// <summary>
        /// Local optimization of the mixing factor: The mixing factor of each node will be varied individually starting from the initial values.
        /// </summary>
        public void OptimizeLocally(double stepSize = 0.05)
        {
            var edges = new EdgeSet(Nodes.Count);
            for (int i = 0; i < Nodes.Count - 1; i++) edges.AddEdge(i, i + 1);
            var exportStrategy = new DefaultExportStrategy(Nodes, edges);
            var system = new NetworkSystem(exportStrategy);
            // TODO: This can be done faster if the flow optimization is simply skipped!
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Storages[2] = new BasicBackup("Test backup", (5000 * _mProductionTuples[i].Item3));
                var best = Try(system, i, OptimalMix[i]);
                var shift = 0;
                var guess = Try(system, i, OptimalMix[i] + stepSize);
                var increase = guess > best;
                if (!increase) guess = Try(system, i, OptimalMix[i] - stepSize);
                // Try increasing alpha.
                if (increase)
                {
                    while (guess > best)
                    {
                        shift += 1;
                        guess = Try(system, i, OptimalMix[i] + stepSize*(shift + 1));
                    }
                }
                // Try decreasing alpha.
                else
                {
                    while (guess > best)
                    {
                        shift = -1;
                        guess = Try(system, i, OptimalMix[i] + stepSize * (shift - 1));
                    }
                }
                // Wee have an optimum, let's save it.
                OptimalMix[i] = OptimalMix[i] + stepSize*shift;
                Nodes[i].Storages[2] = new BasicBackup("Hydro-biomass backup", 409.09 * _mProductionTuples[i].Item3);
                Console.WriteLine("Optimal mix for {0} is {1}.", Nodes[i].CountryName, OptimalMix[i]);
                SetMix(i, OptimalMix[i]);
            }
        }

        public void SetPenetration(double penetration)
        {
            for (int i = 0; i < _mProductionTuples.Count; i++)
            {
                SetMix(i, OptimalMix[i], penetration);
            }
        }

        private double Try(NetworkSystem system, int i, double mix)
        {
            SetMix(i, mix);
            system.Simulate(8766);
            return system.Output.CountryTimeSeriesMap[Nodes[i].CountryName].Single(
                item => item.Name.Equals("Test backup")).Last().Value;
        }

        private void SetMix(int index, double mix, double penetration = 1.00)
        {
            _mProductionTuples[index].Item1.SetScale(mix * penetration * _mProductionTuples[index].Item3);
            _mProductionTuples[index].Item2.SetScale((1 - mix) * penetration * _mProductionTuples[index].Item3);
        }

    }
}
