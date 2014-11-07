using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Utils;

namespace BusinessLogic.ExportStrategies
{
    public class ConstrainedFlowExportStrategy : IExportStrategy
    {

        // Set > 0 to ensure termination. If lower than 1e-6 (with flow optimizer af 1e-10) it will crash.
        public double Tolerance { get { return 1e-4; } }

        private readonly EdgeSet _mEdges;
        private readonly ConstrainedFlowOptimizer _mConstrainedFlowOptimizer;

        private List<Node> _mNodes;
        private Response _mSystemResponse;
        private double[] _mMismatches;
        private double[] _mStorageMap;
        private int _mStorageLevel;

        private readonly double[] _mLoLims;
        private readonly double[] _mHiLims;
        private readonly double[,] _mFlows;

        public ConstrainedFlowExportStrategy(List<Node> nodes, EdgeSet edges)
        {
            if (nodes.Count != edges.NodeCount) throw new ArgumentException("Nodes and edges do not match.");

            _mNodes = nodes;
            _mEdges = edges;

            _mConstrainedFlowOptimizer = new ConstrainedFlowOptimizer(nodes.Count);
            _mConstrainedFlowOptimizer.SetEdges(edges);

            _mLoLims = new double[nodes.Count];
            _mHiLims = new double[nodes.Count];
            _mFlows = new double[nodes.Count,nodes.Count];
        }

        public void Bind(List<Node> nodes, double[] mismatches)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;

            _mStorageMap =
                _mNodes.SelectMany(item => item.StorageCollection.Efficiencies())
                    .Distinct()
                    .OrderByDescending(item => item)
                    .ToArray();
        }

        public BalanceResult BalanceSystem(int tick)
        {
            var result = new BalanceResult {Curtailment = 0.0};
            _mSystemResponse = (_mMismatches.Sum() > 0) ? Response.Charge : Response.Discharge;
            _mFlows.MultiLoop(indices => _mFlows[indices[0], indices[1]] = 0);

            // Loop through levels.
            for (_mStorageLevel = 0; _mStorageLevel < _mStorageMap.Length; _mStorageLevel++)
            {
                // Is the system balanced?
                if (_mMismatches.All(item => Math.Abs(item) < Tolerance)) break;
                // Is there any storage available?
                var storage =
                    _mNodes.Select(item => item.StorageCollection)
                        .Where(item => item.Contains(_mStorageMap[_mStorageLevel]))
                        .Select(item => item.Get(_mStorageMap[_mStorageLevel]).RemainingCapacity(_mSystemResponse))
                        .Sum();
                if (Math.Abs(storage) < Tolerance) continue;

                // Record curtailment, if any.
                if (_mStorageMap[_mStorageLevel] == -1) result.Curtailment = _mMismatches.Sum();

                DoFlowStuff(tick, _mStorageMap[_mStorageLevel]);
            }

            if (Measurering) RecordFlow();

            result.Failure = (result.Curtailment < -_mNodes.Count*Tolerance);
            return result;
        }

        private void DoFlowStuff(int tick, double efficiency)
        {
            // Setup limits.
            for (int idx = 0; idx < _mNodes.Count; idx++)
            {
                if (!_mNodes[idx].StorageCollection.Contains(efficiency))
                {
                    _mLoLims[idx] = 0;
                    _mHiLims[idx] = 0;
                    continue;
                }
                var storage = _mNodes[idx].StorageCollection.Get(efficiency);
                // IMPORTANT: Since storages might be losse, it is only legal to charge OR discharge, otherwise energy dissipates.
                _mLoLims[idx] = (_mMismatches.Sum() > 0) ? -storage.RemainingCapacity(Response.Charge) : 0;
                _mHiLims[idx] = (_mMismatches.Sum() > 0) ? 0 : -storage.RemainingCapacity(Response.Discharge);
            }

            // TODO: Pass capacity used in prios steps to solver (recorded in _mFlow).
            // Determine FLOWS using Gurobi optimization.
            _mConstrainedFlowOptimizer.SetNodes(_mMismatches, _mLoLims, _mHiLims);
            _mConstrainedFlowOptimizer.Solve();

            // Charge based on flow optimization results.
            for (int index = 0; index < _mNodes.Count; index++)
            {
                _mMismatches[index] = _mConstrainedFlowOptimizer.NodeOptimum[index];

                if (!_mNodes[index].StorageCollection.Contains(efficiency)) continue;
                _mNodes[index].StorageCollection.Get(efficiency).Inject(tick, -_mConstrainedFlowOptimizer.StorageOptimum[index]);
            }

            // Save flow result temporarily.
            _mFlows.MultiLoop(indices => _mFlows[indices[0], indices[1]] +=
                _mConstrainedFlowOptimizer.Flows[indices[0], indices[1]]);
        }

        #region Measurement

        private void RecordFlow()
        {
            for (int i = 0; i < _mNodes.Count; i++)
            {
                for (int j = i; j < _mNodes.Count; j++)
                {
                    if (!_mEdges.Connected(i, j)) continue;
                    _mFlowTimeSeriesMap[i + _mNodes.Count * j].AppendData(_mFlows[i, j] - _mFlows[j, i]);
                }
            }
        }

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
                    if (!_mEdges.Connected(i, j)) continue;
                    _mFlowTimeSeriesMap.Add(i + _mNodes.Count * j,
                        new DenseTimeSeries(_mNodes[i].Abbreviation + Environment.NewLine + _mNodes[j].Abbreviation));
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
            var result = _mFlowTimeSeriesMap.Values.ToList();
            foreach (var ts in result) ts.Properties.Add("Flow", "NewFlow");
            return result;
        }

        public bool Measurering { get; private set; }

        private Dictionary<int, ITimeSeries> _mFlowTimeSeriesMap = new Dictionary<int, ITimeSeries>();

        #endregion

    }
}
