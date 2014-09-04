using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.ExportStrategies.DistributionStrategies
{
    public class MinimalFlowStrategy : IDistributionStrategy
    {
        public double Tolerance { get { return 1e-4; } }

        private readonly FlowOptimizer _flowOptimizer;
        private readonly List<Node> _mNodes;
        private readonly EdgeSet _mEdges;
        private readonly double[] _mLoLims;
        private readonly double[] _mHiLims;

        public MinimalFlowStrategy(List<Node> nodes, EdgeSet edges)
        {
            if (nodes.Count != edges.NodeCount) throw new ArgumentException("Nodes and edges do not match.");

            _mNodes = nodes;
            _mEdges = edges;

            _flowOptimizer = new FlowOptimizer(nodes.Count);
            _flowOptimizer.SetEdges(edges);

            _mLoLims = new double[nodes.Count];
            _mHiLims = new double[nodes.Count];
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

            // Record flows.
            if (!Measurering) return;
            for (int i = 0; i < _mNodes.Count; i++)
            {
                for (int j = i; j < _mNodes.Count; j++)
                {
                    if(!_mEdges.EdgeExists(i,j)) continue;
                    _mFlowTimeSeriesMap[i + _mNodes.Count*j].AddData(tick,
                        _flowOptimizer.Flows[i, j] - _flowOptimizer.Flows[j, i]);
                }
            }
        }

        public void EqualizePower(double[] mismatches)
        {
            throw new NotImplementedException();
        }

        #region Measurement

        public void StartMeasurement()
        {
            Measurering = true;
            InitializeTimeSeriesFromEdges();
        }

        private void InitializeTimeSeriesFromEdges()
        {
            _mFlowTimeSeriesMap = new Dictionary<int, ITimeSeries>();

            for (int i = 0; i < _mNodes.Count; i++)
            {
                for (int j = i; j < _mNodes.Count; j++)
                {
                    if (!_mEdges.EdgeExists(i, j)) continue;
                    _mFlowTimeSeriesMap.Add(i + _mNodes.Count*j,
                        new SparseTimeSeries(_mNodes[i].Abbreviation + Environment.NewLine + _mNodes[j].Abbreviation));
                }
            }
        }

        public void Reset()
        {
            Measurering = false;
            _mFlowTimeSeriesMap = null;
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return _mFlowTimeSeriesMap.Values.ToList();
        }

        public bool Measurering { get; private set; }

        private Dictionary<int, ITimeSeries> _mFlowTimeSeriesMap = new Dictionary<int, ITimeSeries>();

        #endregion

    }
}
