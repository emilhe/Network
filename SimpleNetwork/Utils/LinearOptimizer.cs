//using System;
//using System.Collections.Generic;
//using BusinessLogic.ExportStrategies;
//using BusinessLogic.Interfaces;
//using Gurobi;

//namespace BusinessLogic.Utils
//{
//    public class LinearOptimizer : IOptimizer
//    {

//        public Action OnSolveCompleted { private get; set; }

//        public bool ExtractFlows { get; set; }
//        public double[,] Flows { get; private set; }

//        private Dictionary<int, GRBVar> _mEdgeDummies { get; set; }

//        #region Objective weights

//        private const double _mEdgeWeight = 1;
//        private readonly double _mBalanceWeight = 100;

//        #endregion

//        private readonly CoreOptimizer _mCore;

//        public LinearOptimizer(EdgeCollection edges, int m)
//        {
//            Flows = new double[edges.NodeCount, edges.NodeCount];
//            _mEdgeDummies = new Dictionary<int, GRBVar>();

//            _mCore = new CoreOptimizer(edges, m, BaseObjective, AddVars, AddConstrs)
//            {
//                OnSolveCompleted = ExtractResultsFromModel,
//            };

//            // So far, just track it all.
//            ExtractFlows = true;
//            ExtractStorageOptima = true;
//        }

//        private void AddVars(CoreOptimizer core)
//        {
//            for (int i = 0; i < core.N; i++)
//            {
//                for (int j = i; j < core.N; j++)
//                {
//                    if (!core.Edges.Connected(i, j)) continue;
//                    // Dummy variables to allow ABSOLUTE minimization.
//                    _mEdgeDummies.Add(i + j * core.N, core.Wrap.Model.AddVar(-double.MaxValue, double.MaxValue, 0, core.Precision, "edgeDummy" + i + j));
//                }
//            }
//        }

//        private void AddConstrs(CoreOptimizer core)
//        {
//            // Dummy variables to allow ABSOLUTE minimization.
//            foreach (var pair in core.Wrap.Edges)
//            {
//                var dummy = _mEdgeDummies[pair.Key];
//                core.Wrap.Model.AddConstr(dummy, GRB.GREATER_EQUAL, pair.Value, "edgeDummyConstPlus" + pair.Key);
//                core.Wrap.Model.AddConstr(dummy, GRB.GREATER_EQUAL, -pair.Value, "edgeDummyConstMinus" + pair.Key);
//            }
//        }

//        private GRBExpr BaseObjective(MyModel wrap)
//        {
//            GRBLinExpr obj = 0.0;
//            // Minimal balancing (linear).
//            obj.MultAdd(_mBalanceWeight, ObjectiveFactory.LinearBalancing(wrap));
//            // Low flow (linear).
//            foreach (var dummy in _mEdgeDummies) obj.MultAdd(_mEdgeWeight, dummy.Value);

//            return obj;
//        }

//        private void ExtractResultsFromModel()
//        {
//            try
//            {
//                // Extract flow optima.
//                if (ExtractFlows)
//                {
//                    for (int i = 0; i < _mCore.N; i++)
//                    {
//                        for (int j = 0; j < _mCore.N; j++)
//                        {
//                            if (!_mCore.Edges.EdgeExists(i, j)) continue;

//                            Flows[i, j] = _mCore.Wrap.Edges[i + _mCore.N * j].Get(GRB.DoubleAttr.X);
//                        }
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                _mCore.Wrap.Model.ComputeIIS();
//                _mCore.Wrap.Model.Write(@"C:\flowModel.ilp");
//                throw;
//            }

//            if(OnSolveCompleted != null) OnSolveCompleted();
//        }

//        #region Delegation

//        public bool ExtractStorageOptima
//        {
//            get { return _mCore.ExtractStorageOptima; }
//            set { _mCore.ExtractStorageOptima = value; }
//        }

//        public double[] NodeOptima { get { return _mCore.NodeOptima; } }
//        public List<double[]> StorageOptima { get { return _mCore.StorageOptima; } }

//        public int N
//        {
//            get { return _mCore.N; }
//        }

//        public int M
//        {
//            get { return _mCore.M; }
//        }

//        public EdgeCollection Edges
//        {
//            get { return _mCore.Edges; }
//        }

//        public MyModel Wrap
//        {
//            get { return _mCore.Wrap; }
//        }

//        public double[] Deltas
//        {
//            get { return _mCore.Mismatches; }
//        }

//        public void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits)
//        {
//            ((IOptimizer) _mCore).SetNodes(nodes, lowLimits, highLimits);
//        }

//        public void Solve()
//        {
//            ((IOptimizer) _mCore).Solve();
//        }

//        #endregion

//    }
//}
