using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Gurobi;
using Utils;

namespace BusinessLogic.ExportStrategies
{
    class ConSyncScheme : IExportScheme
    {

        private readonly QuadFlowOptimizer _mOptimizer;
        private readonly EdgeCollection _mEdges;

        private IList<INode> _mNodes;
        private double[] _mMismatches;

        #region REHINK THIS PART

        //private Response _mSystemResponse;
        //private readonly double[] _mLoLims;
        //private readonly double[] _mHiLims;
        //private readonly double[,] _mFlows;

        // TODO: Remove HACK
        public ConSyncScheme(List<CountryNode> nodes, EdgeCollection edges)
            : this(nodes.Select(item => (INode) item).ToList(), edges)
        {
        }

        public ConSyncScheme(IList<INode> nodes, EdgeCollection edges)
        {
            _mNodes = nodes;
            _mEdges = edges;
            
            // Corresponds to the projection vector.
            var weights = _mNodes.Select(node => 1.0 / CountryInfo.GetMeanLoad(node.Name)).ToArray();
            weights.Mult(1.0 / weights.Sum());

            var core = new CoreOptimizer(_mEdges, 0, item => ObjectiveFactory.QuadraticBalancing(item, weights));
            _mOptimizer = new QuadFlowOptimizer(core, core.ApplyNodalConstrs, core.RemoveNodalConstrs); // HERE NO STORAGE IS ASSUMED!!
        }

        #endregion

        public void Bind(IList<INode> nodes, double[] mismatches)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;
        }

        public void BalanceSystem()
        {
            // Do balancing.
            _mOptimizer.SetNodes(_mMismatches, null, null);
            _mOptimizer.Solve();
            for (int i = 0; i < _mNodes.Count; i++)
            {
                _mNodes[i].Balancing.CurrentValue = _mOptimizer.NodeOptima[i];
                _mMismatches[i] = 0;
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
                    _mFlowTimeSeriesMap.Add(i + _mNodes.Count * j, ts);
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
                    _mFlowTimeSeriesMap[i + _mNodes.Count * j].AppendData(_mOptimizer.Flows[i, j] - _mOptimizer.Flows[j, i]);
                }
            }
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            var result = _mFlowTimeSeriesMap.Select(item => (ITimeSeries)item.Value).ToList();
            foreach (var ts in result) ts.Properties.Add("Flow", "NewFlow");
            return result;
        }

        private Dictionary<int, DenseTimeSeries> _mFlowTimeSeriesMap = new Dictionary<int, DenseTimeSeries>();

        #endregion
    }
}
