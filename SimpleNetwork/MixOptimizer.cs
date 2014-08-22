﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using SimpleNetwork.ExportStrategies;
using SimpleNetwork.ExportStrategies.DistributionStrategies;
using SimpleNetwork.Generators;
using SimpleNetwork.Interfaces;
using SimpleNetwork.Storages;
using SimpleNetwork.TimeSeries;

namespace SimpleNetwork
{
    public class MixOptimizer
    {
        private readonly MixController _mMixController;

        public List<Node> Nodes { get; set; }
        public double[] OptimalMix { get; set; }

        #region Mix cache

        private static readonly double[] MixCache =
        {
            0.55, 0.55, 0.45, 0.55, 0.55, 0.5, 0.55, 0.65, .5, 0.65, 0.7, 0.55, 0.6, 0.45, 0.5, 0.5, 0.65, 0.5, 0.6, 0.5,
            0.6, 0.6, 0.75, 0.6, 0.6, 0.5, 0.5, 0.5, 0.55, 0.75
        };

        public void ReadMixCahce()
        {
            OptimalMix = MixCache;
        }

        #endregion

        /// <summary>
        /// Construction. More/different constuctors to be added...
        /// </summary>
        /// <param name="data"> data to build nodes from </param>
        public MixOptimizer(List<Node> data)
        {
            Nodes = data;
            OptimalMix = new double[data.Count];
            _mMixController = new MixController(data);
        }

        /// <summary>
        /// Individual optimization of the mixing factor: The mixing factor will be optimal for each node considered in isolation.
        /// </summary>
        public void OptimizeIndividually(double stepSize = 0.05)
        {
            var model = new NetworkModel(new List<Node> {Nodes[0]}, new NoExportStrategy(), new BottomUpStrategy());
            var system = new Simulation(model);
            
            for (int i = 0; i < Nodes.Count; i++)
            {
                var best = 0.0  ;
                for (double mix = 0.3; mix < 0.9; mix += stepSize)
                {
                    _mMixController.Mixes[i] = mix;
                    _mMixController.Execute();
                    system.Nodes = new List<Node> { Nodes[i] };
                    system.Simulate(8766); // One year.
                    var result = -system.Output.SystemTimeSeries["Curtailment"].Last().Value;
                    if(result < best) continue;
                    // Wee have a new optimum, let's save it.
                    best = result;
                    OptimalMix[i] = mix;
                }
                Console.WriteLine("Optimal mix for {0} is {1}.", Nodes[i].CountryName, OptimalMix[i]);
                _mMixController.Mixes[i] = OptimalMix[i];
                _mMixController.Execute();

            }
        }

        ///// <summary>
        ///// Local optimization of the mixing factor: The mixing factor of each node will be varied individually starting from the initial values.
        ///// </summary>
        //public void OptimizeLocally(double stepSize = 0.05)
        //{
        //    var edges = new EdgeSet(Nodes.Count);
        //    for (int i = 0; i < Nodes.Count - 1; i++) edges.AddEdge(i, i + 1);
        //    var exportStrategy = new NetworkModel(Nodes, new CooperativeExportStrategy(), new MinimalFlowStrategy(edges));
        //    var system = new Simulation(exportStrategy);
        //    // TODO: This can be done faster if the flow optimization is simply skipped!
        //    for (int i = 0; i < Nodes.Count; i++)
        //    {
        //        Nodes[i].StorageCollection[2] = new BasicBackup("Test backup", (5000 * Nodes[i].LoadTimeSeries.GetAverage() * 5000));
        //        var best = Try(system, i, OptimalMix[i]);
        //        var shift = 0;
        //        var guess = Try(system, i, OptimalMix[i] + stepSize);
        //        var increase = guess > best;
        //        if (!increase) guess = Try(system, i, OptimalMix[i] - stepSize);
        //        // Try increasing alpha.
        //        if (increase)
        //        {
        //            while (guess > best)
        //            {
        //                shift += 1;
        //                guess = Try(system, i, OptimalMix[i] + stepSize*(shift + 1));
        //            }
        //        }
        //        // Try decreasing alpha.
        //        else
        //        {
        //            while (guess > best)
        //            {
        //                shift = -1;
        //                guess = Try(system, i, OptimalMix[i] + stepSize * (shift - 1));
        //            }
        //        }
        //        // Wee have an optimum, let's save it.
        //        OptimalMix[i] = OptimalMix[i] + stepSize*shift;
        //        Nodes[i].StorageCollection[2] = new BasicBackup("Hydro-biomass backup", 409.09 * Nodes[i].LoadTimeSeries.GetAverage() * 5000);
        //        Console.WriteLine("Optimal mix for {0} is {1}.", Nodes[i].CountryName, OptimalMix[i]);
        //        _mMixController.Mixes[i] = OptimalMix[i];
        //        _mMixController.Execute();
        //    }
        //}

        private double Try(Simulation system, int i, double mix)
        {
            _mMixController.Mixes[i] = mix;
            _mMixController.Execute();
            system.Simulate(8766);
            return system.Output.CountryTimeSeriesMap[Nodes[i].CountryName].Single(
                item => item.Name.Equals("Test backup")).Last().Value;
        }

    }
}
