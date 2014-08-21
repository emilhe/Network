using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies.DistributionStrategies
{
    public class MinimalFlowStrategy : IDistributionStrategy
    {
        public double Tolerance { get { return 1e-4; } }

        private readonly FlowOptimizer _flowOptimizer;
        private readonly double[] _mLoLims;
        private readonly double[] _mHiLims;

        public MinimalFlowStrategy(EdgeSet edges)
        {
            _flowOptimizer = new FlowOptimizer(edges.NodeCount);
            _flowOptimizer.SetEdges(edges);

            _mLoLims = new double[edges.NodeCount];
            _mHiLims = new double[edges.NodeCount];
        }

        public void DistributePower(List<Node> nodes, double[] mismatches, int storageLevel, int tick)
        {
            // Setup limits.
            var idx = 0;
            foreach (var storage in nodes.Select(item => item.Storages[storageLevel]))
            {
                // IMPORTANT: Since storages might be losse, it is only legal to charge OR discharge, BOTH (energy dissipates).
                _mLoLims[idx] = (mismatches.Sum() > 0)? 0 : storage.RemainingCapacity(Response.Discharge);
                _mHiLims[idx] = (mismatches.Sum() > 0)? storage.RemainingCapacity(Response.Charge) : 0;
                idx++;
            }

            // Determine FLOWS using Gurobi optimization.
            _flowOptimizer.SetNodes(mismatches, _mLoLims, _mHiLims);
            _flowOptimizer.Solve();

            // Charge based on flow optimization results.
            for (int index = 0; index < nodes.Count; index++)
            {
                mismatches[index] = nodes[index].Storages[storageLevel].Inject(tick, _flowOptimizer.NodeOptimum[index]);
            }
        }

    }
}
