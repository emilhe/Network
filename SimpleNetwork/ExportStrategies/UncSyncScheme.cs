using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Utils;

namespace BusinessLogic.ExportStrategies
{
    /// <summary>
    /// Synchronized flow. Use when flow is unconstrained. Does NOT use gurobi.
    /// </summary>
    public class UncSyncScheme: IExportScheme
    {

        private readonly EdgeCollection _mEdges;
        private readonly PhaseAngleFlow _mFlow;
        private readonly INode[] _mNodes;
        private readonly double[] _mBalProjVec;
        private readonly List<double[]> _mStoProjVec;

        private double[] _mMismatches;
        private double[] _mInjections;
        private double[] _mFlows;

        public UncSyncScheme(INode[] nodes, EdgeCollection edges, double[] weights = null)
        {
            _mNodes = nodes;
            _mEdges = edges;
            _mFlow = new PhaseAngleFlow(_mEdges.IncidenceMatrix);

            if (weights != null)
            {
                _mBalProjVec = weights;
                return;
            }

            // Balancing projection vector.
            _mBalProjVec = _mNodes.Select(node => CountryInfo.GetMeanLoad(node.Name)).ToArray().Norm();
            // Storage projection vector(s).
            _mStoProjVec = new List<double[]>(_mNodes[0].Storages.Count);
            for (int i = 0; i < _mNodes[0].Storages.Count; i++)
            {
                _mStoProjVec.Add(_mNodes.Select(node => node.Storages[i].NominalEnergy).ToArray().Norm());
            }
        }

        public void Bind(double[] mismatches)
        {
            _mMismatches = mismatches;
            _mInjections = new double[mismatches.Length];
        }

        public void BalanceSystem()
        {
            // Consider storage.
            var toBalance = _mMismatches.Sum();
            _mInjections.Fill(0);
            for (int i = 0; i < _mNodes[0].Storages.Count; i++)
            {
                var proj = _mStoProjVec[i];
                for (int j = 0; j < _mNodes.Length; j++)
                {
                    var balanced = (proj[j]*toBalance - _mNodes[j].Storages[i].Inject(proj[j]*toBalance));
                    _mInjections[j] += balanced;
                }
                toBalance = _mMismatches.Sum() - _mInjections.Sum();
            }
            // Dump the rest in the balancing vector.
            for (int i = 0; i < _mNodes.Length; i++)
            {
                var nodalBalance = toBalance*_mBalProjVec[i];
                _mInjections[i] += nodalBalance - _mMismatches[i];
                _mNodes[i].Balancing.CurrentValue = nodalBalance;
                _mMismatches[i] = 0;
            }
            // Calculate flows (make optional?).
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
