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

namespace BusinessLogic.ExportStrategies
{
    class UncLocalScheme : IExportScheme
    {

        private readonly LinearOptimizer _mOptimizer;
        private readonly EdgeCollection _mEdges;
        private readonly PhaseAngleFlow _mFlow;
        private readonly INode[] _mNodes;

        private double[] _mMismatches;
        private double[] _mInjections;
        private double[] _mFlows;

        //private readonly double[] _mLoLims;
        //private readonly double[] _mHiLims;
        //private readonly double[,] _mFlows;

        public UncLocalScheme(INode[] nodes, EdgeCollection edges, double[] weights = null)
        {
            _mNodes = nodes;
            _mEdges = edges;
            _mFlow = new PhaseAngleFlow(_mEdges.IncidenceMatrix);
            _mOptimizer = new LinearOptimizer(edges, 0); // HERE NO STORAGE IS ASSUMED!!
        }

        public void Bind(double[] mismatches)
        {
            _mMismatches = mismatches;
            _mInjections = new double[mismatches.Length];
        }

        public void BalanceSystem()
        {
            // Do balancing.
            _mOptimizer.SetNodes(_mMismatches, null, null);
            _mOptimizer.Solve();
            for (int i = 0; i < _mNodes.Length; i++)
            {
                _mNodes[i].Balancing.CurrentValue = _mOptimizer.NodeOptima[i];
                _mInjections[i] = _mOptimizer.NodeOptima[i] - _mMismatches[i];
                _mMismatches[i] = 0;
            }

            _mFlows = _mFlow.CalculateFlows(_mInjections);
        }

        #region Measurement

        public bool Measuring { get; private set; }

        public void Start(int ticks)
        {
            _mFlowTimeSeries = new List<DenseTimeSeries>();
            foreach (var link in _mEdges.Links)
            {
                var from = _mNodes.Single(item => item.Name.Equals(link.From));
                var to = _mNodes.Single(item => item.Name.Equals(link.To));
                var ts = new DenseTimeSeries(from.Abbreviation + Environment.NewLine + to.Abbreviation, ticks);
                ts.Properties.Add("Flow", "Unconstrained localized");                
                ts.Properties.Add("From", from.Name);
                ts.Properties.Add("To", to.Name);
                _mFlowTimeSeries.Add(ts);
            }

            Measuring = true;
        }

        public void Clear()
        {
            _mFlowTimeSeries = null;
            Measuring = false;
        }

        public void Sample(int tick)
        {
            if (!Measuring) return;

            for (int i = 0; i < _mFlows.Length; i++)
            {
                _mFlowTimeSeries[i].AppendData(_mFlows[i]);
            }
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return _mFlowTimeSeries.Select(item => (ITimeSeries) item).ToList();
        }

        private List<DenseTimeSeries> _mFlowTimeSeries = new List<DenseTimeSeries>();

        #endregion

    }
}
