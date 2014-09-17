using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;

namespace BusinessLogic.ExportStrategies
{
    public class ConstrainedFlowExportStrategy : IExportStrategy
    {

        public double Tolerance { get { return 1e-4; } }

        private readonly EdgeSet _mEdges;
        private readonly ConstrainedFlowOptimizer _constrainedFlowOptimizer;

        private List<Node> _mNodes;
        private Response _mSystemResponse;
        private double[] _mMismatches;
        private double[] _mStorageMap;
        private int _mStorageLevel;

        private readonly double[] _mLoLims;
        private readonly double[] _mHiLims;

        public ConstrainedFlowExportStrategy(List<Node> nodes, EdgeSet edges)
        {
            if (nodes.Count != edges.NodeCount) throw new ArgumentException("Nodes and edges do not match.");

            _mNodes = nodes;
            _mEdges = edges;

            _constrainedFlowOptimizer = new ConstrainedFlowOptimizer(nodes.Count);
            _constrainedFlowOptimizer.SetEdges(edges);

            _mLoLims = new double[nodes.Count];
            _mHiLims = new double[nodes.Count];
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

            // Loop through levels.
            for (_mStorageLevel = 0; _mStorageLevel < _mStorageMap.Length; _mStorageLevel++)
            {
                // TODO: Should it rather be the out commented line?
                //if (_mMismatches.All(item => Math.Abs(item)  < Tolerance)) break;
                if (Math.Abs(_mMismatches.Sum()) < _mNodes.Count * Tolerance) break;
                // Add more smart stuff here (break earlier to enchance performance).
                var storage =
                    _mNodes.Select(item => item.StorageCollection)
                        .Where(item => item.Contains(_mStorageMap[_mStorageLevel]))
                        .Select(item => item.Get(_mStorageMap[_mStorageLevel]).RemainingCapacity(_mSystemResponse))
                        .Sum();
                if (Math.Abs(storage) < Tolerance) continue;

                // Calculate curtailment.
                if (_mStorageMap[_mStorageLevel] == -1) result.Curtailment = _mMismatches.Sum();
 
                DoFlowStuff(tick, _mStorageMap[_mStorageLevel]);
            }

            result.Failure = (result.Curtailment < 0);
            return result;
        }

        private void DoFlowStuff(int tick, double efficiency)
        {
            // TODO: What about efficiency? ...
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

            // TODO: What about flow capacity used in prior steps?
            // Determine FLOWS using Gurobi optimization.
            _constrainedFlowOptimizer.SetNodes(_mMismatches, _mLoLims, _mHiLims);
            try
            {
                _constrainedFlowOptimizer.Solve();
            }
            catch (Exception e)
            {
                var hest = 5;
            }

            // Charge based on flow optimization results.
            for (int index = 0; index < _mNodes.Count; index++)
            {
                _mMismatches[index] = _constrainedFlowOptimizer.NodeOptimum[index];

                if (!_mNodes[index].StorageCollection.Contains(efficiency)) continue;
                _mNodes[index].StorageCollection.Get(efficiency).Inject(tick, -_constrainedFlowOptimizer.StorageOptimum[index]);
            }

            // Record flows.
            if (!Measurering) return;
            for (int i = 0; i < _mNodes.Count; i++)
            {
                for (int j = i; j < _mNodes.Count; j++)
                {
                    if (!_mEdges.EdgeExists(i, j)) continue;
                    _mFlowTimeSeriesMap[i + _mNodes.Count * j].AddData(tick,
                        _constrainedFlowOptimizer.Flows[i, j] - _constrainedFlowOptimizer.Flows[j, i]);
                }
            }
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
                    _mFlowTimeSeriesMap.Add(i + _mNodes.Count * j,
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
            var result = _mFlowTimeSeriesMap.Values.ToList();
            foreach (var ts in result) ts.Properties.Add("Flow", "NewFlow");
            return result;
        }

        public bool Measurering { get; private set; }

        private Dictionary<int, ITimeSeries> _mFlowTimeSeriesMap = new Dictionary<int, ITimeSeries>();

        #endregion

    }
}
