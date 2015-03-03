using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
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

        private IList<INode> _mNodes;
        private Response _mSystemResponse;
        private double[] _mMismatches;
        private double[] _mStorageMap;
        private int _mStorageLevel;

        private readonly double[] _mLoLims;
        private readonly double[] _mHiLims;
        private readonly double[,] _mFlows;

        // TODO: Remove HACK
        public ConstrainedFlowExportStrategy(List<CountryNode> nodes, EdgeSet edges)
            : this(nodes.Select(item => (INode) item).ToList(), edges)
        {
        }

        public ConstrainedFlowExportStrategy(IList<INode> nodes, EdgeSet edges)
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

        public void Bind(IList<INode> nodes, double[] mismatches)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;

            _mStorageMap =
                _mNodes.SelectMany(item => item.StorageCollection.Select(subItem => subItem.Key))
                    .Distinct()
                    .OrderByDescending(item => item)
                    .ToArray();
        }

        public void BalanceSystem()
        {
            // Reset flow vectors.
            _mFlows.MultiLoop(indices => _mFlows[indices[0], indices[1]] = 0);

            // Transmission.
            DoFlowStuff(-1);

            // Storage interaction.
            _mSystemResponse = (_mMismatches.Sum() > 0) ? Response.Charge : Response.Discharge;
            for (_mStorageLevel = 0; _mStorageLevel < _mStorageMap.Length; _mStorageLevel++)
            {
                // Is the system balanced?
                if (_mMismatches.All(item => Math.Abs(item) < Tolerance)) break;
                // Is there any storage available?
                var storage =
                    _mNodes.Select(item => item.StorageCollection)
                        .Where(item => item.Contains(_mStorageMap[_mStorageLevel]))
                        .Select(item => item.Get(_mStorageMap[_mStorageLevel]).AvailableEnergy(_mSystemResponse))
                        .Sum();
                 if (Math.Abs(storage) < Tolerance) continue;

                DoFlowStuff(_mStorageMap[_mStorageLevel]);
            }

            // Dump the rest in the balancing vector.
            for (int index = 0; index < _mNodes.Count; index++)
            {
                _mNodes[index].Balancing.Inject(_mMismatches[index]);
                _mMismatches[index] = 0;
            }
        }

        private void DoFlowStuff(double efficiency)
        {
            SetupLimits(efficiency);

            // TODO: Pass capacity used in prios steps to solver (recorded in _mFlow).
            // Determine FLOWS using Gurobi optimization.
            _mConstrainedFlowOptimizer.SetNodes(_mMismatches, _mLoLims, _mHiLims);
            _mConstrainedFlowOptimizer.Solve();

            // Charge based on flow optimization results.
            for (int index = 0; index < _mNodes.Count; index++)
            {
                _mMismatches[index] = _mConstrainedFlowOptimizer.NodeOptimum[index];

                if (!_mNodes[index].StorageCollection.Contains(efficiency)) continue;
                _mNodes[index].StorageCollection.Inject(-_mConstrainedFlowOptimizer.StorageOptimum[index]);
            }

            // Save flow result temporarily.
            _mFlows.MultiLoop(indices => _mFlows[indices[0], indices[1]] +=
                _mConstrainedFlowOptimizer.Flows[indices[0], indices[1]]);
        }

        private void SetupLimits(double efficiency)
        {
            // Setup limits.
            for (int idx = 0; idx < _mNodes.Count; idx++)
            {
                // Transmission.
                if (efficiency == -1)
                {
                    _mLoLims[idx] = 0;
                    _mHiLims[idx] = 0;
                    continue;
                }
                // No storage available.
                if (!_mNodes[idx].StorageCollection.Contains(efficiency))
                {
                    _mLoLims[idx] = 0;
                    _mHiLims[idx] = 0;
                    continue;
                }
                // Storage found.
                var storage = _mNodes[idx].StorageCollection.Get(efficiency);
                // IMPORTANT: Since storages might be losse, it is only legal to charge OR discharge, otherwise energy dissipates (HACK!!).
                _mLoLims[idx] = (_mMismatches.Sum() > 0) ? -storage.AvailableEnergy(Response.Charge) : 0;
                _mHiLims[idx] = (_mMismatches.Sum() > 0) ? 0 : -storage.AvailableEnergy(Response.Discharge);
            }
        }

        #region Measurement

        public bool Measuring { get; private set; }

        public void Start(int ticks)
        {
            _mFlowTimeSeriesMap = new Dictionary<int, DenseTimeSeries>();

            for (int i = 0; i < _mNodes.Count; i++)
            {
                for (int j = i; j < _mNodes.Count; j++)
                {
                    if (!_mEdges.Connected(i, j)) continue;
                    var ts = new DenseTimeSeries(_mNodes[i].Abbreviation + Environment.NewLine + _mNodes[j].Abbreviation, ticks);
                    ts.Properties.Add("From", _mNodes[i].Name);
                    ts.Properties.Add("To", _mNodes[j].Name);
                    _mFlowTimeSeriesMap.Add(i + _mNodes.Count * j,ts);
                }
            }

            Measuring = true;
        }

        public void Clear()
        {
            _mFlowTimeSeriesMap = null;
            Measuring = false;
        }

        public void Sample(int tick)
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

        public List<ITimeSeries> CollectTimeSeries()
        {
            var result = _mFlowTimeSeriesMap.Select(item => (ITimeSeries) item.Value).ToList();
            foreach (var ts in result) ts.Properties.Add("Flow", "NewFlow");
            return result;
        }

        private Dictionary<int, DenseTimeSeries> _mFlowTimeSeriesMap = new Dictionary<int, DenseTimeSeries>();

        #endregion

    }
}
