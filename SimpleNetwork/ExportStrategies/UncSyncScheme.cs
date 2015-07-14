using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using Utils;

namespace BusinessLogic.ExportStrategies
{
    /// <summary>
    /// Synchronized flow. Use when flow is unconstrained. Does NOT use gurobi.
    /// </summary>
    public class UncSyncScheme: IExportScheme
    {

        public static double Power { get; set; }
        public static double Bias { get; set; }

        private int N;
        private int I;
        private double meanLoad;

        private readonly EdgeCollection _mEdges;
        private readonly PhaseAngleFlow _mFlow;
        private readonly INode[] _mNodes;
        private readonly double[] _mBalProjVec;
        private readonly List<double[]> _mStoProjVec;

        private double[] _mMismatches;
        private double[] _mFlows;

        public UncSyncScheme(INode[] nodes, EdgeCollection edges, double[] weights = null)
        {
            _mNodes = nodes;
            _mEdges = edges;
            _mFlow = new PhaseAngleFlow(_mEdges.IncidenceMatrix);
            N = _mNodes.Length;
            I = _mNodes[0].Storages.Count;
            meanLoad = nodes.Select(item => ((CountryNode) item).Model.AvgLoad).Sum();

            // Balancing projection vector.
            _mBalProjVec = weights ?? _mNodes.Select(node => ((CountryNode)node).Model.AvgLoad).ToArray().Norm();
            //_mBalProjVec = weights ?? _mNodes.Select(node => ((CountryNode) node).Model.AvgDeficit).ToArray().Norm();
        }

        public void Bind(double[] mismatches)
        {
            _mMismatches = mismatches;
        }

        public void BalanceSystem()
        {
            if (I > 0) ApplyStorage();
            var remaining = _mMismatches.Sum();
            // Dump the rest in the balancing vector.
            for (int i = 0; i < _mNodes.Length; i++)
            {
                var nodalBalance = remaining*_mBalProjVec[i];
                _mMismatches[i] -= nodalBalance;
                _mNodes[i].Balancing.CurrentValue = nodalBalance;
            }
            // Calculate flows (make optional?).
            _mFlows = _mFlow.CalculateFlows(_mMismatches);
            //_mMismatches.Fill(0);
        }

        private void ApplyStorage()
        {
            var bias = meanLoad*Bias;
            _mMismatches.Add(bias / _mMismatches.Length);
            for (int i = 0; i < I; i++)
            {
                // Is there anything to balance?
                var remaining = _mMismatches.Sum();
                var resp = (remaining > 0) ? Response.Charge : Response.Discharge;
                if (remaining == 0) return;
                // Determine how resources should be distributed.
                var injVec = _mNodes.Select(item => item.Storages[i].AvailableEnergy(resp)).ToArray();
                var lvl = (resp == Response.Charge) ? 1 : Math.Pow(_mNodes.Select(item => item.Storages[i].ChargeLevel).Average(), Power);
                var capacity = injVec.Sum();
                var toInject = (resp == Response.Charge) ? Math.Min(remaining, capacity) : Math.Max(remaining * lvl, capacity);
                if (toInject == 0) continue;
                injVec.Norm(toInject);
                // Apply injections.
                for (int j = 0; j < N; j++)
                {
                    _mMismatches[j] -= (injVec[j]-_mNodes[j].Storages[i].Inject(injVec[j]));
                }
            }
            _mMismatches.Add(-bias / _mMismatches.Length);
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
