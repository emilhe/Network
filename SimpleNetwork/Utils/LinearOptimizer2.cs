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
    public class LinearOptimizer2 : IOptimizer
    {

        public bool ExtractFlows { get; set; }
        public bool ExtractStorageOptima { get; set; }

        public double[,] Flows { get; private set; }
        public double[] NodeOptima { get; private set; }
        public List<double[]> StorageOptima { get; private set; }

        private Dictionary<int, GRBVar> _mEdgeDummies { get; set; }
        private const double _mEdgeWeight = 1;

        private readonly CoreOptimizer _mCore;

        public LinearOptimizer2(int n, int m)
        {
            _mCore = new CoreOptimizer(n, m)
            {
                OnSolveCompleted = ExtractResultsFromModel,
                SetupAdditionalVariables = SetupAdditionalVariables,
                SetupAdditionalConstraints = SetupAdditionalConstraints,
                SetupAdditionalObjectives = SetupAdditionalObjectives
            };

            Flows = new double[n, n];
            NodeOptima = new double[n];
            StorageOptima = new List<double[]>();
            for (int i = 0; i < m; i++) StorageOptima.Add(new double[n]);
            _mEdgeDummies = new Dictionary<int, GRBVar>();

            // So far, just track it all.
            ExtractFlows = true;
            ExtractStorageOptima = true;
        }

        private void SetupAdditionalVariables()
        {
            for (int i = 0; i < _mCore.N; i++)
            {
                for (int j = i; j < _mCore.N; j++)
                {
                    if (!_mCore.Edges.Connected(i, j)) continue;
                    // Dummy variables to allow ABSOLUTE minimization.
                    _mEdgeDummies.Add(i + j * _mCore.N, _mCore.Wrap.Model.AddVar(-double.MaxValue, double.MaxValue, 0, _mCore.Precision, "edgeDummy" + i + j));
                }
            }
        }

        private void SetupAdditionalConstraints()
        {
            // Dummy variables to allow ABSOLUTE minimization.
            foreach (var pair in _mCore.Wrap.Edges)
            {
                var dummy = _mEdgeDummies[pair.Key];
                _mCore.Wrap.Model.AddConstr(dummy, GRB.GREATER_EQUAL, pair.Value, "edgeDummyConstPlus" + pair.Key);
                _mCore.Wrap.Model.AddConstr(dummy, GRB.GREATER_EQUAL, -pair.Value, "edgeDummyConstMinus" + pair.Key);
            }
        }

        private void SetupAdditionalObjectives(GRBLinExpr obj)
        {
            // Third objective: Low flow (linear).
            foreach (var dummy in _mEdgeDummies) obj.MultAdd(_mEdgeWeight, dummy.Value);
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

            if(OnSolveCompleted != null) OnSolveCompleted();
        }

        #region Delegation

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
            ((IOptimizer) _mCore).SetEdges(edges);
        }

        public void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits)
        {
            ((IOptimizer) _mCore).SetNodes(nodes, lowLimits, highLimits);
        }

        public void Solve()
        {
            ((IOptimizer) _mCore).Solve();
        }

        #endregion

    }
}
