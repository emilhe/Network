//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BusinessLogic.ExportStrategies;
//using Gurobi;

//namespace BusinessLogic.Utils
//{

//    public class LinearOptimizer
//    {

//        public double Tolerance { get { return 1E-10; } }

//        public double[,] Flows { get; private set; }
//        //public double[] NodeOptima { get; private set; }
//        //public List<double[]> StorageOptima { get; private set; }

//        public double OptimalBalance { get; private set; }

//        #region Fields

//        private const char Precision = GRB.CONTINUOUS;
//        private const bool DebugLog = false;

//        private bool Ready { get { return NodesSet && EdgesSet; } }
//        private bool NodesSet { get { return _mDeltas != null; } }
//        private bool EdgesSet { get { return _mEdges != null; } }

//        private readonly GRBEnv _mEnv;
//        private readonly int _mN;
//        private EdgeCollection _mEdges;

//        //private readonly ModelWrapper _mFlow;
//        private readonly ModelWrapper2 _mBalance;
//        private GRBConstr _mOptimumConstr;

//        private double[] _mDeltas;

//        #endregion

//        #region Objective weights

//        private double _mBalanceWeight = 100;
//        private double _mStorageWeight = 10;
//        private double _mEdgeWeight = 1;

//        #endregion

//        #region Public methods

//        /// <summary>
//        /// Initialization.
//        /// </summary>
//        /// <param name="n"> problem size (number of nodes) </param>
//        /// <param name="m"> number of storage levels </param>
//        public LinearOptimizer(int n, int m)
//        {
//            _mEnv = new GRBEnv();
//            _mEnv.Set(GRB.IntParam.LogToConsole, 0);
//            _mN = n;

//            _mBalance = new ModelWrapper2(_mEnv, n, m);

//            Flows = new double[_mN, _mN];
//        }

//        /// <summary>
//        /// Set the network edges.
//        /// </summary>
//        /// <param name="edges"> edges </param>
//        public void SetEdges(EdgeCollection edges)
//        {
//            if (edges.NodeCount != _mN)
//            {
//                throw new ArgumentException("Dismension mismatch between edges and FlowOptimizer.");
//            }

//            _mEdges = edges;

//            SetupVariables(_mBalance);
//        }

//        /// <summary>
//        /// Set the network nodes.
//        /// </summary>
//        /// <param name="nodes"> nodes </param>
//        /// <param name="lowLimits"> lower limit (discharge) </param>
//        /// <param name="highLimits"> higher limit (charge) </param>
//        public void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits)
//        {
//            var now = DateTime.Now;
//            if (nodes.Length != _mN)
//            {
//                throw new ArgumentException("Dismension mismatch between nodes and FlowOptimizer.");
//            }

//            _mDeltas = nodes;

//            if (!Ready) return;

//            SetConstraints(_mBalance, lowLimits, highLimits);
//            SetBalanceObjective();

//            // Update model.
//            if (DebugLog) Console.WriteLine("Setup: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
//            //_mFlow.Model.Update();
//            if (DebugLog) Console.WriteLine("Flow: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
//            _mBalance.Model.Update();
//            if (DebugLog) Console.WriteLine("Balance: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

//        }

//        public void Solve()
//        {
//            var now = DateTime.Now;
//            if (!Ready) throw new ArgumentException(SolveError());

//            // Solve models.
//            SolveBalanceModel();
//            if (DebugLog) Console.WriteLine("Solve balance: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

//            // Make results publicly available.
//            ExtractResultsFromModel();
//            if (DebugLog) Console.WriteLine("Extract results: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

//            // Remove old constraints.
//            foreach (var constr in _mBalance.Constraints) _mBalance.Model.Remove(constr);
//            if (DebugLog) Console.WriteLine("Remove constraints: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
//        }

//        private void SolveBalanceModel()
//        {
//            // First, solve to get optimal balance results.
//            _mBalance.Model.Optimize();

//            try
//            {
//                OptimalBalance = 0;
//                foreach (var expr in _mBalance.NodeExprs) OptimalBalance += expr.Value;
//            }
//            catch (Exception e)
//            {
//                _mBalance.Model.ComputeIIS();
//                _mBalance.Model.Write(@"C:\Temp\balanceModel.ilp");
//                throw;
//            }
//        }

//        public void Dispose()
//        {
//            _mEnv.Dispose();
//        }

//        #endregion

//        #region Private methods

//        private void ExtractResultsFromModel()
//        {
//            var now = DateTime.Now;
//            try
//            {
//                for (int i = 0; i < _mBalance.Storages.Count; i++)
//                {
//                    for (int j = 0; j < _mN; j++)
//                    {
//                        // Starting with initial value.
//                        StorageOptima[i][j] = _mBalance.Storages[i][j].Get(GRB.DoubleAttr.X);
//                    }
//                }
//                if (DebugLog) Console.WriteLine("Extract storage: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

//                for (int i = 0; i < _mN; i++)
//                {
//                    for (int j = 0; j < _mN; j++)
//                    {
//                        if (!_mEdges.EdgeExists(i, j)) continue;

//                        Flows[i, j] = _mBalance.Edges[i + _mN * j].Get(GRB.DoubleAttr.X);
//                    }
//                }
//                if (DebugLog) Console.WriteLine("Extract edges: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
//            }
//            catch (Exception e)
//            {
//                _mBalance.Model.ComputeIIS();
//                _mBalance.Model.Write(@"C:\flowModel.ilp");
//                throw;
//            }

//            for (int i = 0; i < _mN; i++)
//            {
//                // Starting with initial value.
//                NodeOptima[i] = _mDeltas[i] + StorageOptima.Select(item => item[i]).Sum();
//                // Subtract/Add export/imports.
//                for (int j = 0; j < _mN; j++)
//                {
//                    // TODO: Correct order of signs?
//                    NodeOptima[i] = NodeOptima[i] - Flows[j, i] + Flows[i, j];
//                }
//            }
//            if (DebugLog) Console.WriteLine("Calc opts: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
//        }

//        /// <summary>
//        /// Set objective.
//        /// </summary>
//        private void SetConstraints(ModelWrapper2 m, List<double[]> lowLimits, List<double[]> highLimits)
//        {
//            // Update nodal balancing objectives. 
//            for (int i = 0; i < _mN; i++)
//            {
//                m.NodeExprs[i] = 0.0;
//                var sign = (_mDeltas[i] > 0) ? 1 : -1;
//                // Add mismatch.
//                m.NodeExprs[i].AddConstant(sign * _mDeltas[i]);
//                // Add edges.
//                for (int j = 0; j < _mN; j++)
//                {
//                    if (_mEdges.EdgeExists(i, j)) m.NodeExprs[i].AddTerm(+sign, m.Edges[i + _mN * j]);
//                    if (_mEdges.EdgeExists(j, i)) m.NodeExprs[i].AddTerm(-sign, m.Edges[j + _mN * i]);
//                }
//                // Add storage.
//                for (int k = 0; k < m.Storages.Count; k++)
//                {
//                    m.NodeExprs[i].AddTerm(sign, m.Storages[k][i]);
//                    m.Storages[k][i].Set(GRB.DoubleAttr.LB, lowLimits[k][i]);
//                    m.Storages[k][i].Set(GRB.DoubleAttr.UB, highLimits[k][i]);
//                }
//                // Ensure that too much balancing is not applied.
//                m.Constraints[i] = m.Model.AddConstr(m.NodeExprs[i], GRB.GREATER_EQUAL, 0, "const" + i);
//            }
//        }

//        #region Setup

//        /// <summary>
//        /// Set variables (when edges change).
//        /// </summary>
//        private void SetupVariables(ModelWrapper2 m)
//        {
//            for (int i = 0; i < _mN; i++)
//            {
//                // Add storage.
//                for (int j = 0; j < m.Storages.Count; j++)
//                {
//                    m.Storages[j][i] = m.Model.AddVar(-double.MaxValue, double.MaxValue, 0, Precision, "storage" + i + "level" + j);
//                    m.StorageDummies[j][i] = m.Model.AddVar(-double.MaxValue, double.MaxValue, 0, Precision, "storageDummy" + i + "level" + j);
//                }
//                // Add edges.
//                for (int j = i; j < _mN; j++)
//                {
//                    if (!_mEdges.Connected(i, j)) continue;
//                    var cap = _mEdges.GetEdgeCapacity(i, j);
//                    m.Edges.Add(i + j * _mN, m.Model.AddVar(-cap, cap, 0, Precision, "edge" + i + j));
//                    // Add dummy varaibles
//                    m.EdgeDummies.Add(i + j * _mN, m.Model.AddVar(-double.MaxValue, double.MaxValue, 0, Precision, "edgeDummy" + i + j));
//                }
//            }

//            m.Model.Update();

//            // Add edge dummy varaible constraints.
//            foreach (var pair in m.Edges)
//            {
//                var dummy = m.EdgeDummies[pair.Key];
//                m.Model.AddConstr(dummy, GRB.GREATER_EQUAL, pair.Value, "edgeDummyConstPlus" + pair.Key);
//                m.Model.AddConstr(dummy, GRB.GREATER_EQUAL, -pair.Value, "edgeDummyConstMinus" + pair.Key);
//            }
//            // Add storage dummy varaible constraints.
//            for (int i = 0; i < _mN; i++)
//            {
//                for (int j = 0; j < m.Storages.Count; j++)
//                {
//                    var dummy = m.StorageDummies[j][i];
//                    m.Model.AddConstr(dummy, GRB.GREATER_EQUAL, m.Storages[j][i], "storageDummyConstPlus" + i + "level" + j);
//                    m.Model.AddConstr(dummy, GRB.GREATER_EQUAL, m.Storages[j][i], "storageDummyConstPlus" + i + "level" + j);
//                }
//            }

//            m.Model.Update();
//        }

//        /// <summary>
//        /// Set balance objective (when nodes change).
//        /// </summary>
//        private void SetBalanceObjective()
//        {
//            GRBLinExpr obj = 0.0;

//            // Main objective: Minimize balancing.
//            foreach (var expr in _mBalance.NodeExprs) obj.MultAdd(_mBalanceWeight, expr);
//            // Secondary objective: Low flow.
//            foreach (var dummy in _mBalance.EdgeDummies) obj.MultAdd(_mEdgeWeight, dummy.Value);
//            // Third objective: Low storage usage.
//            for (int i = 0; i < _mBalance.StorageDummies.Count; i++)
//            {
//                var level = _mBalance.StorageDummies[i];
//                foreach (var dummy in level) obj.MultAdd((i + 1) * _mStorageWeight, dummy);
//            }

//            _mBalance.Model.SetObjective(obj, GRB.MINIMIZE);
//        }

//        #endregion

//        #endregion

//        #region Help methods

//        private string SolveError()
//        {
//            return "Cannot optimize flow; variables not set: " + (NodesSet ? "" : "nodes ") +
//                   (EdgesSet ? "" : "edges");
//        }

//        #endregion

//    }

//    public class ModelWrapper2
//    {
//        // Edge variables.
//        public Dictionary<int, GRBVar> Edges { get; set; }
//        public Dictionary<int, GRBVar> EdgeDummies { get; set; }
//        // Storage variables.
//        public List<GRBVar[]> Storages { get; set; }
//        public List<GRBVar[]> StorageDummies { get; set; }
//        // Nodal balancing expressions.
//        public GRBLinExpr[] NodeExprs { get; set; }
//        // Model constraints.
//        public GRBConstr[] Constraints { get; set; }

//        public GRBModel Model { get; set; }

//        public ModelWrapper2(GRBEnv env, int n, int m)
//        {
//            Model = new GRBModel(env);

//            EdgeDummies = new Dictionary<int, GRBVar>();
//            Edges = new Dictionary<int, GRBVar>();

//            Constraints = new GRBConstr[n];
//            NodeExprs = new GRBLinExpr[n];

//            Storages = new List<GRBVar[]>();
//            StorageDummies = new List<GRBVar[]>();
//            for (int i = 0; i < m; i++)
//            {
//                Storages.Add(new GRBVar[n]);
//                StorageDummies.Add(new GRBVar[n]);
//            }
//        }

//    }

//}
