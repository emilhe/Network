using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using Gurobi;

namespace BusinessLogic.Utils
{
    class QuadraticOptimizer : IOptimizer
    {

        public bool ExtractFlows { get; set; }
        public bool ExtractStorageOptima { get; set; }

        public double[,] Flows { get; private set; }
        public double[] NodeOptima { get; private set; }
        public List<double[]> StorageOptima { get; private set; }

        private readonly SystemOptimizer _mCore;
        private GRBQuadExpr _mFlowObjective;

        public QuadraticOptimizer(int n, int m)
        {
            _mCore = new SystemOptimizer(n, m) {OnSolveCompleted = SolveQuadratic};

            Flows = new double[n, n];
            NodeOptima = new double[n];
            StorageOptima = new List<double[]>();
            for (int i = 0; i < m; i++) StorageOptima.Add(new double[n]);
        }

        private void SolveQuadratic()
        {
            // Add new constraints. TODO: ADD STORAGE CONSTRAINTS TOO
            GRBLinExpr sum = 0.0;
            foreach (var expr in _mCore.Wrap.NodeExprs) sum.Add(expr);
            var optimumConstr = _mCore.Wrap.Model.AddConstr(sum, GRB.LESS_EQUAL, _mCore.BalanceOptimum, "Optimal balance");
            // Set new balancing objective and optimize.
            _mCore.Wrap.Model.SetObjective(_mFlowObjective);
            _mCore.Wrap.Model.Update();
            _mCore.Wrap.Model.Optimize();
            // Extract results.
            ExtractResultsFromModel();
            // Remove new constraints.
            _mCore.Wrap.Model.Remove(optimumConstr);

            if (OnSolveCompleted != null) OnSolveCompleted();
        }

        private void ExtractResultsFromModel()
        {
            try
            {
                // Extract node optima.
                for (int i = 0; i < _mCore.N; i++)
                {
                    NodeOptima[i] = _mCore.Wrap.NodeExprs[i].Value * Math.Sign(_mCore.Deltas[i]);
                }

                // Extract storage optima.
                if (ExtractStorageOptima)
                {
                    for (int j = 0; j < _mCore.N; j++)
                    {
                        for (int i = 0; i < _mCore.Wrap.Storages.Count; i++)
                        {
                            StorageOptima[i][j] = _mCore.Wrap.Storages[i][j].Get(GRB.DoubleAttr.X);
                        }
                    }
                }

                // Extract flow optima.
                if (ExtractFlows)
                {
                    for (int i = 0; i < _mCore.N; i++)
                    {
                        for (int j = 0; j < _mCore.N; j++)
                        {
                            if (!_mCore.Edges.EdgeExists(i, j)) continue;

                            Flows[i, j] = _mCore.Wrap.Edges[i + _mCore.N * j].Get(GRB.DoubleAttr.X);
                        }
                    }
                }
            }
            catch (Exception)
            {
                _mCore.Wrap.Model.ComputeIIS();
                _mCore.Wrap.Model.Write(@"C:\flowModel.ilp");
                throw;
            }
        }

        /// <summary>
        /// Set flow objective (when edges change).
        /// </summary>
        private GRBQuadExpr BuildFlowObjective()
        {
            GRBQuadExpr flowObjective = 0.0;

            for (int i = 0; i < _mCore.N; i++)
            {
                for (int j = i; j < N; j++)
                {
                    if (!_mCore.Edges.Connected(i, j)) continue;
                    // Note that the SQUARED flow is minimized (to minimize the needed capacity).
                    flowObjective.AddTerm(_mCore.Edges.GetEdgeCost(i, j), _mCore.Wrap.Edges[i + j * N], _mCore.Wrap.Edges[i + j * N]);
                }
            }

            return flowObjective;
        }

        #region Delegation

        public void Solve()
        {
            ((IOptimizer)_mCore).Solve();
        }

        public Action OnSolveCompleted { private get; set; }

        public int N
        {
            get { return _mCore.N; }
        }

        public EdgeCollection Edges
        {
            get { return _mCore.Edges; }
        }

        public ModelWrapper3 Wrap
        {
            get { return _mCore.Wrap; }
        }

        public double[] Deltas
        {
            get { return _mCore.Deltas; }
        }

        public void SetEdges(EdgeCollection edges)
        {
            ((IOptimizer)_mCore).SetEdges(edges);
            _mFlowObjective = BuildFlowObjective();
        }

        public void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits)
        {
            ((IOptimizer)_mCore).SetNodes(nodes, lowLimits, highLimits);
        }

        #endregion

    }
}
