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

        public void DistributePower(List<Node> nodes, double[] mismatches, double efficiency, int tick)
        {
            // Setup limits.
            for (int idx = 0; idx < nodes.Count; idx++)
            {
                if (!nodes[idx].StorageCollection.Contains(efficiency))
                {
                    _mLoLims[idx] = 0;
                    _mHiLims[idx] = 0;
                    continue;
                }
                var storage = nodes[idx].StorageCollection.Get(efficiency);
                // IMPORTANT: Since storages might be losse, it is only legal to charge OR discharge, otherwise energy dissipates.
                _mLoLims[idx] = (mismatches.Sum() > 0) ? 0 : storage.RemainingCapacity(Response.Discharge);
                _mHiLims[idx] = (mismatches.Sum() > 0) ? storage.RemainingCapacity(Response.Charge) : 0;
            }

            // Determine FLOWS using Gurobi optimization.
            _flowOptimizer.SetNodes(mismatches, _mLoLims, _mHiLims);
            _flowOptimizer.Solve();

            // Charge based on flow optimization results.
            for (int index = 0; index < nodes.Count; index++)
            {
                if (!nodes[index].StorageCollection.Contains(efficiency))
                {
                    mismatches[index] = _flowOptimizer.NodeOptimum[index];
                    continue;
                }
                mismatches[index] = nodes[index].StorageCollection.Get(efficiency).Inject(tick, _flowOptimizer.NodeOptimum[index]);
            }
        }

    }
}
