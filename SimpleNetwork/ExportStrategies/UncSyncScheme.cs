using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.TimeSeries;
using Utils;

namespace BusinessLogic.ExportStrategies
{
    
    public class UncSyncScheme : IExportScheme
    {

        private readonly EdgeCollection _mEdges;
        private readonly PhaseAngleFlow _mFlow;

        private IList<INode> _mNodes;
        private double[] _mMismatches;
        private double[] _mInjections;
        private double[] _mFlows;

        private double[] _mWeights;

        #region REHING THIS PART

        //private Response _mSystemResponse;
        //private readonly double[] _mLoLims;
        //private readonly double[] _mHiLims;
        //private readonly double[,] _mFlows;

                // TODO: Remove HACK
        public UncSyncScheme(List<CountryNode> nodes, EdgeCollection edges, double[] weights = null)
            : this(nodes.Select(item => (INode) item).ToList(), edges, weights)
        {
        }

        public UncSyncScheme(IList<INode> nodes, EdgeCollection edges, double[] weights = null)
        {
            //if (nodes.Count != edges.NodeCount) throw new ArgumentException("Nodes and edges do not match.");

            _mNodes = nodes;
            _mEdges = edges;
            _mFlow = new PhaseAngleFlow(_mEdges.IncidenceMatrix);

            if (weights != null)
            {
                _mWeights = weights;
                return;
            }

            // Corresponds to the projection vector.
            _mWeights = _mNodes.Select(node => CountryInfo.GetMeanLoad(node.Name)).ToArray().Norm();

            //_mLoLims = new double[nodes.Count];
            //_mHiLims = new double[nodes.Count];
            //_mFlows = new double[nodes.Count,nodes.Count];
        }

        #endregion

        public void Bind(IList<INode> nodes, double[] mismatches)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;
            _mInjections = new double[mismatches.Length];
        }

        public void BalanceSystem()
        {
            // Do balancing.
            var toBalance = _mMismatches.Sum();
            for (int i = 0; i < _mNodes.Count; i++)
            {
                _mNodes[i].Balancing.CurrentValue = toBalance*_mWeights[i];
                _mInjections[i] = toBalance * _mWeights[i] - _mMismatches[i];
                _mMismatches[i] = 0;
            }
            // Calculate flows (make optional? Check performance...).
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
                ts.Properties.Add("Flow", "Unconstrained synchronized");                
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
