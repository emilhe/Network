using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using Gurobi;
using Utils;

namespace BusinessLogic.Utils
{
    public class CoreOptimizer : IOptimizer
    {

        #region Fields

        public bool ExtractStorageOptima { get; set; }

        public double[] NodeOptima { get; private set; }
        public List<double[]> StorageOptima { get; private set; }

        public char Precision = GRB.CONTINUOUS;
        public bool DebugLog = false;
        public double Tol { get; set; }

        public EdgeCollection Edges { get; private set; }
        public double[] Mismatches { get; private set; }
        public int N { get; private set; }
        public int M { get; private set; }

        private readonly GRBEnv _mEnv;
        public MyModel Wrap { get; private set; }

        #endregion



        #region Delegation

        public Action OnSolveCompleted { private get; set; }
        private readonly Func<MyModel, GRBExpr> _mBaseObjFunc;

        #endregion

        #region Objective weights

        private readonly double _mStorageWeight = 10;

        #endregion

        #region Public methods

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="n"> problem size (number of nodes) </param>
        /// <param name="m"> number of storage levels </param>
        public CoreOptimizer(EdgeCollection edges, int m, Func<MyModel, GRBExpr> baseObjFunc, Action<CoreOptimizer> addVars = null, Action<CoreOptimizer> addConstrs = null)
        {
            _mEnv = new GRBEnv();
            _mEnv.Set(GRB.IntParam.LogToConsole, 0);
            //_mEnv.Set(GRB.DoubleParam.FeasibilityTol, Delta);
            _mBaseObjFunc = baseObjFunc;

            Edges = edges;
            N = edges.NodeCount;
            M = m;
            Wrap = new MyModel(_mEnv, N, m);
            SetupVariables(Wrap, addVars, addConstrs);

            NodeOptima = new double[N];
            StorageOptima = new List<double[]>();
            for (int i = 0; i < m; i++) StorageOptima.Add(new double[N]);
        }

        /// <summary>
        /// Set the network nodes.
        /// </summary>
        /// <param name="nodes"> nodes </param>
        /// <param name="lowLimits"> lower limit (discharge) </param>
        /// <param name="highLimits"> higher limit (charge) </param>
        public void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits)
        {
            if (nodes.Length != N)
            {
                throw new ArgumentException("Dismension mismatch between nodes and FlowOptimizer.");
            }

            Mismatches = nodes;
            SetConstraints(Wrap, lowLimits, highLimits);
            PrepareObjective();

            Wrap.Model.Update();
        }

        public void Solve()
        {
            DateTime now = DateTime.Now;
            try
            {
                // Solve models.
                Wrap.Model.Optimize();
                if (DebugLog) Console.WriteLine("Solve balance: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

                // Extract node optima.
                for (int i = 0; i < N; i++)
                {
                    NodeOptima[i] = Wrap.NodeExprs[i].Value;
                }

                // Extract storage optima.
                if (ExtractStorageOptima)
                {
                    for (int j = 0; j < N; j++)
                    {
                        for (int i = 0; i < Wrap.Storages.Count; i++)
                        {
                            StorageOptima[i][j] = Wrap.Storages[i][j].Get(GRB.DoubleAttr.X);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Wrap.Model.ComputeIIS();
                Wrap.Model.Write(@"C:\Temp\flowModel.ilp");
                throw;
            }

            // Signal solve completed.
            if (OnSolveCompleted != null) OnSolveCompleted();
            if (DebugLog) Console.WriteLine("Extract results: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
        }

        public void Dispose()
        {
            _mEnv.Dispose();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Setup constraints.
        /// </summary>
        private void SetConstraints(MyModel m, List<double[]> lowLimits, List<double[]> highLimits)
        {
            for (int i = 0; i < N; i++)
            {
                // Update storage charge levels.
                for (int k = 0; k < m.Storages.Count; k++)
                {
                    m.Storages[k][i].Set(GRB.DoubleAttr.LB, lowLimits[k][i]);
                    m.Storages[k][i].Set(GRB.DoubleAttr.UB, highLimits[k][i]);
                }
                // Update delta.
                m.NodeExprConstrsPos[i].Set(GRB.DoubleAttr.RHS, Mismatches[i]);
                m.NodeExprConstrsNeg[i].Set(GRB.DoubleAttr.RHS, -Mismatches[i]);
                m.NodeExprs[i].AddConstant(Mismatches[i] - m.NodeExprs[i].Constant);
            }

            m.Model.Update();
        }

        #region Setup

        /// <summary>
        /// Set variables (when edges change).
        /// </summary>
        private void SetupVariables(MyModel m, Action<CoreOptimizer> addVars, Action<CoreOptimizer> addConstrs)
        {
            for (int i = 0; i < N; i++)
            {
                // Add storage.
                for (int j = 0; j < m.Storages.Count; j++)
                {
                    m.Storages[j][i] = m.Model.AddVar(-double.MaxValue, double.MaxValue, 0, Precision,
                        "storage" + i + "level" + j);
                    m.StorageDummies[j][i] = m.Model.AddVar(-double.MaxValue, double.MaxValue, 0, Precision,
                        "storageDummy" + i + "level" + j);
                }
                // Add node exprs dummy.
                m.NodeExprsDummies[i] = m.Model.AddVar(-double.MaxValue, double.MaxValue, 0, Precision, "nodeExprDummy" + i);
                // Add edges.
                for (int j = i; j < N; j++)
                {
                    if (!Edges.Connected(i, j)) continue;
                    var cap = Edges.GetEdgeCapacity(i, j);
                    m.Edges.Add(i + j*N, m.Model.AddVar(-cap, cap, 0, Precision, "edge" + i + j));
                }
            }

            if (addVars != null) addVars(this);
            m.Model.Update();

            // Add storage dummy varaible constraints.
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < m.Storages.Count; j++)
                {
                    var dummy = m.StorageDummies[j][i];
                    m.Model.AddConstr(dummy, GRB.GREATER_EQUAL, m.Storages[j][i],
                        "storageDummyConstPlus" + i + "level" + j);
                    m.Model.AddConstr(dummy, GRB.GREATER_EQUAL, -m.Storages[j][i],
                        "storageDummyConstPlus" + i + "level" + j);
                }
            }

            // Setup nodal balancing objectives. 
            for (int i = 0; i < N; i++)
            {
                m.NodeExprs[i] = NodeExpr(m, i, +1);
                // Add node exprs constraints.
                var pos = NodeExpr(m, i, +1);
                var neg = NodeExpr(m, i, -1);
                var dummy = m.NodeExprsDummies[i];
                neg.Add(dummy);
                m.NodeExprConstrsPos[i] = m.Model.AddConstr(neg, GRB.GREATER_EQUAL, 0, "nodeExprDummyConstPlus" + i);
                pos.Add(dummy);
                m.NodeExprConstrsNeg[i] = m.Model.AddConstr(pos, GRB.GREATER_EQUAL, 0, "nodeExprDummyConstMinus" + i);
            }

            if (addConstrs != null) addConstrs(this);
            m.Model.Update();
        }

        private GRBLinExpr NodeExpr(MyModel model, int i, double sign)
        {
            var expr = new GRBLinExpr();
            // Add edges.
            for (int j = 0; j < N; j++)
            {
                if (Edges.EdgeExists(i, j)) expr.AddTerm(+sign, model.Edges[i + N * j]);
                if (Edges.EdgeExists(j, i)) expr.AddTerm(-sign, model.Edges[j + N * i]);
            }
            // Add storage.
            for (int k = 0; k < model.Storages.Count; k++)
            {
                expr.AddTerm(-sign, model.Storages[k][i]);
            }
            return expr;
        }

        /// <summary>
        /// Set objective (when nodes change).
        /// </summary>
        private void PrepareObjective()
        {
            var baseObj = _mBaseObjFunc(Wrap);

            if (baseObj is GRBLinExpr)
            {
                var linObj = baseObj as GRBLinExpr;
                linObj.Add(StorageObjective());
                Wrap.Model.SetObjective(linObj, GRB.MINIMIZE);
                return;
            }

            if (baseObj is GRBQuadExpr)
            {
                var quadObj = baseObj as GRBQuadExpr;
                quadObj.Add(StorageObjective());
                Wrap.Model.SetObjective(quadObj, GRB.MINIMIZE);
                return;
            }

            throw new ArgumentException("Invalid expression.");
        }

        /// <summary>
        /// Storage part of objective.
        /// </summary>
        private GRBLinExpr StorageObjective()
        {
            var linExpr = new MyLinExpr();
            // Minimize storage usage in levels.
            for (int i = 0; i < Wrap.StorageDummies.Count; i++)
            {
                var level = Wrap.StorageDummies[i];
                foreach (var dummy in level) linExpr.AddTerm((i + 1)*_mStorageWeight, dummy);
            }
            return linExpr.GrbLinExpr;
        }

        #endregion

        #region Balancing constraints

        private GRBConstr _mSystemBalancingConstr;
        private GRBConstr[] _mNodalBalancingConstrs;
        private List<GRBConstr> _mSystemStorageConstr;
        private List<GRBConstr[]> _mNodalStorageConstrs;


        public void ApplySystemConstr()
        {
            GRBLinExpr sum = 0.0;
            foreach (var dummy in Wrap.NodeExprsDummies)
            {
                sum.Add(dummy);
            }
            _mSystemBalancingConstr = Wrap.Model.AddConstr(sum, GRB.LESS_EQUAL, sum.Value + Tol, "Optimal balance");

            _mSystemStorageConstr = new List<GRBConstr>(Wrap.Storages.Count);
            for (int j = 0; j < Wrap.Storages.Count; j++)
            {
                sum = 0.0;
                foreach (var dummy in Wrap.StorageDummies[j])
                {
                    sum.Add(dummy);
                }
                _mSystemStorageConstr.Add(Wrap.Model.AddConstr(sum, GRB.LESS_EQUAL, sum.Value + Tol,
                    "Optimal storage balance " + j));
            }
        }

        public void RemoveSystemConstr()
        {
            Wrap.Model.Remove(_mSystemBalancingConstr);
            foreach (var constr in _mSystemStorageConstr) Wrap.Model.Remove(constr);
        }

        public void SetTmpTol(double tmpTol)
        {
            var rhs = _mSystemBalancingConstr.Get(GRB.DoubleAttr.RHS) - Tol + tmpTol;
            Wrap.Model.Remove(_mSystemBalancingConstr);
            GRBLinExpr sum = 0.0;
            foreach (var dummy in Wrap.NodeExprsDummies) sum.Add(dummy);
            _mSystemBalancingConstr = Wrap.Model.AddConstr(sum, GRB.LESS_EQUAL, rhs, "Optimal balance");
            Wrap.Model.Update();
        }

        public void ApplyNodalConstrs()
        {
            _mNodalBalancingConstrs = new GRBConstr[N];
            for (int i = 0; i < Wrap.NodeExprs.Length; i++)
            {
                _mNodalBalancingConstrs[i] = Wrap.Model.AddConstr(Wrap.NodeExprs[i], GRB.EQUAL,
                    Wrap.NodeExprs[i].Value, "Optimal nodal balance " + i);
            }

            _mNodalStorageConstrs = new List<GRBConstr[]>(Wrap.Storages.Count);
            for (int j = 0; j < Wrap.Storages.Count; j++)
            {
                _mNodalStorageConstrs[j] = new GRBConstr[N];
                for (int i = 0; i < Wrap.Storages[j].Length; i++)
                {
                    _mNodalStorageConstrs[j][i] = Wrap.Model.AddConstr(Wrap.Storages[j][i], GRB.EQUAL,
                        Wrap.Storages[j][i].Get(GRB.DoubleAttr.X), "Optimal storage balance " + i + j);
                }
            }
        }

        public void RemoveNodalConstrs()
        {
            foreach (var constr in _mNodalBalancingConstrs) Wrap.Model.Remove(constr);
            foreach (var level in _mNodalStorageConstrs)
            {
                foreach (var constr in level)
                {
                    Wrap.Model.Remove(constr);
                }   
            }
        }

        #endregion

        #endregion

    }

    public class MyModel
    {
        // Edge variables.
        public Dictionary<int, GRBVar> Edges { get; set; }
        // Storage variables.
        public List<GRBVar[]> Storages { get; set; }
        public List<GRBVar[]> StorageDummies { get; set; }
        // Nodal balancing expressions.
        //public MyLinExpr[] NodeExprs { get; set; }
        public GRBLinExpr[] NodeExprs { get; set; }
        public GRBVar[] NodeExprsDummies { get; set; }
        // Model constraints.
        public GRBConstr[] NodeExprConstrsPos { get; set; }
        public GRBConstr[] NodeExprConstrsNeg { get; set; }


        public GRBModel Model { get; set; }

        public MyModel(GRBEnv env, int n, int m)
        {
            Model = new GRBModel(env);

            Edges = new Dictionary<int, GRBVar>();
            NodeExprConstrsPos = new GRBConstr[n];
            NodeExprConstrsNeg = new GRBConstr[n];
            NodeExprs = new GRBLinExpr[n];
            NodeExprsDummies = new GRBVar[n]; ;

            Storages = new List<GRBVar[]>();
            StorageDummies = new List<GRBVar[]>();
            for (int i = 0; i < m; i++)
            {
                Storages.Add(new GRBVar[n]);
                StorageDummies.Add(new GRBVar[n]);
            }
        }

    }

    public class MyLinExpr
    {
        public List<GRBVar> GRBVars { get; private set; }
        public List<double> Coeffs { get; private set; }
        public double Constant { get; private set; }

        public GRBLinExpr GrbLinExpr
        {
            get
            {
                if (_mLinEvaluated) return _mGrbLinExpr;

                // Build expression.
                _mGrbLinExpr = 0.0;
                _mGrbLinExpr.AddTerms(Coeffs.ToArray(), GRBVars.ToArray());
                _mGrbLinExpr.AddConstant(Constant);

                _mLinEvaluated = true;
                return _mGrbLinExpr;
            }
        }

        public GRBQuadExpr GrbQuadExpr
        {
            get
            {
                if (_mQuadEvaluated) return _mGrbQuadExpr;

                // Build expression.
                _mGrbQuadExpr = 0.0;
                for (int i = 0; i < GRBVars.Count; i++)
                {
                    _mGrbQuadExpr.AddTerms(Coeffs.ToArray().Mult(Coeffs[i]), new GRBVar[GRBVars.Count].Fill(GRBVars[i]), GRBVars.ToArray());
                }
                _mGrbQuadExpr.AddTerms(Coeffs.ToArray().Mult(Constant * 2), GRBVars.ToArray());
                _mGrbQuadExpr.AddConstant(Constant * Constant);

                _mQuadEvaluated = true;
                return _mGrbQuadExpr;
            }
        }

        private GRBLinExpr _mGrbLinExpr;
        private GRBQuadExpr _mGrbQuadExpr;

        private bool _mLinEvaluated;
        private bool _mQuadEvaluated;

        public MyLinExpr()
        {
            GRBVars = new List<GRBVar>();
            Coeffs = new List<double>();
        }

        public void AddTerm(double coeff, GRBVar grbVar)
        {
            if(_mLinEvaluated || _mQuadEvaluated) throw new ArgumentException("Expression fixed.");

            GRBVars.Add(grbVar);
            Coeffs.Add(coeff);
        }

        public void AddConstant(double coeff)
        {
            if (_mLinEvaluated || _mQuadEvaluated) throw new ArgumentException("Expression fixed.");

            Constant += coeff;
        }

    }

}
