using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Utils;

namespace BusinessLogic.ExportStrategies
{
    /// <summary>
    /// Synchronized flow. Use when flow is unconstrained. Does NOT use gurobi.
    /// </summary>
    public class UncSyncScheme: IExportScheme
    {

        private int N;
        private int I;

        private readonly EdgeCollection _mEdges;
        private readonly PhaseAngleFlow _mFlow;
        private readonly INode[] _mNodes;
        private readonly double[] _mBalProjVec;
        private readonly List<double[]> _mStoProjVec;

        private double[] _mMismatches;
        private double[] _mInjections;
        private double[] _mFlows;

        private DiagonalMatrix a;
        private DenseMatrix w;

        public UncSyncScheme(INode[] nodes, EdgeCollection edges, double[] weights = null)
        {
            _mNodes = nodes;
            _mEdges = edges;
            _mFlow = new PhaseAngleFlow(_mEdges.IncidenceMatrix);
            N = _mNodes.Length;
            I = _mNodes[0].Storages.Count;

            // Balancing projection vector.
            _mBalProjVec = weights ?? _mNodes.Select(node => CountryInfo.GetMeanLoad(node.Name)).ToArray().Norm();
            // Storage projection matrix.
            if (I == 0) return;
            a = new DiagonalMatrix(I, I);
            w = new DenseMatrix(N, I);
            for (int i = 0; i < I; i++)
            {
                w.SetColumn(i, _mNodes.Select(node => node.Storages[i].NominalEnergy).ToArray().Norm());
            }
        }

        public void Bind(double[] mismatches)
        {
            _mMismatches = mismatches;
            _mInjections = new double[mismatches.Length];
        }

        public void BalanceSystem()
        {
            var remaining = _mMismatches.Sum();
            if (I > 0) remaining = ApplyStorage(remaining);     
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

        private double ApplyStorage(double remaining)
        {
            var resp = (remaining > 0) ? Response.Charge : Response.Discharge;
            // Update a(t) vector.
            for (int i = 0; i < I; i++)
            {
                // Are we done?
                if (Math.Abs(remaining) < 1e-6)
                {
                    a[i, i] = 0;
                    continue;
                }
                // Determine coefficient.
                var capacity = _mNodes.Select(item => item.Storages[i].AvailableEnergy(resp)).Sum();
                a[i, i] = (remaining > 0) ? Math.Min(remaining, capacity) : Math.Max(remaining, capacity);
                remaining -= a[i, i];
            }
            // Calculate S and inject.
            var s = w * a;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < I; j++)
                {
                    _mMismatches[i] -= s[i, j];
                    _mNodes[i].Storages[j].Inject(s[i, j]);
                }
            }
            return remaining;
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
