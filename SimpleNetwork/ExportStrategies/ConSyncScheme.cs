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
        private readonly INode[] _mNodes;
        private readonly StorageMap _mMap;

        private double[] _mMismatches;
        private readonly double _mBalanceWeight = 1e6;

        public ConSyncScheme(INode[] nodes, EdgeCollection edges)
        {
            _mNodes = nodes;
            _mEdges = edges;
            
            // Corresponds to the projection vector.
            var weights = _mNodes.Select(node => 1.0 / CountryInfo.GetMeanLoad(node.Name)).ToArray();
            weights.Mult(1.0 / weights.Sum());
            _mMap = new StorageMap(nodes);

            var core = new CoreOptimizer(_mEdges, _mMap.Levels, item =>
            {
                var obj = new GRBQuadExpr();
                obj.MultAdd(_mBalanceWeight, ObjectiveFactory.QuadraticBalancing(item, weights));
                return obj;
            });
            _mOptimizer = new QuadFlowOptimizer(core, core.ApplyNodalConstrs, core.RemoveNodalConstrs); // HERE NO STORAGE IS ASSUMED!!
        }

        public void Bind(double[] mismatches)
        {
            _mMismatches = mismatches;
        }

        public void BalanceSystem()
        {
            // Create storage limit vectors.
            _mMap.RefreshLims();
            // Do balancing.
            _mOptimizer.SetNodes(_mMismatches, _mMap.LowLims, _mMap.HighLims);
            _mOptimizer.Solve();
            // Charge storages.
            _mMap.Inject(_mOptimizer.StorageOptima);
            // Dump remaining stuff in balancing vector.
            for (int i = 0; i < _mNodes.Length; i++)
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

            for (int i = 0; i < _mNodes.Length; i++)
            {
                for (int j = i; j < _mNodes.Length; j++)
                {
                    if (!_mEdges.Connected(i, j)) continue;
                    var ts = new DenseTimeSeries(_mNodes[i].Abbreviation + Environment.NewLine + _mNodes[j].Abbreviation, ticks);
                    ts.Properties.Add("From", _mNodes[i].Name);
                    ts.Properties.Add("To", _mNodes[j].Name);
                    _mFlowTimeSeriesMap.Add(i + _mNodes.Length * j, ts);
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
            for (int i = 0; i < _mNodes.Length; i++)
            {
                for (int j = i; j < _mNodes.Length; j++)
                {
                    if (!_mEdges.Connected(i, j)) continue;
                    _mFlowTimeSeriesMap[i + _mNodes.Length * j].AppendData(_mOptimizer.Flows[i, j] - _mOptimizer.Flows[j, i]);
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
