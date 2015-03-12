using System;
using System.Collections.Generic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using Gurobi;

namespace BusinessLogic.Utils
{
    public class CoreOptimizer : IOptimizer
    {

        #region Fields

        public char Precision = GRB.CONTINUOUS;
        public bool DebugLog = false;

        public EdgeCollection Edges { get; private set; }
        public double[] Deltas { get; private set; }        
        public int N { get; private set; }

        private bool Ready { get { return NodesSet && EdgesSet; } }
        private bool NodesSet { get { return Deltas != null; } }
        private bool EdgesSet { get { return Edges != null; } }

        private readonly GRBEnv _mEnv;
        public ModelWrapper3 Wrap { get; private set; }

        #endregion

        #region Delegation

        public Action OnSolveCompleted { private get; set; }
        public Action SetupAdditionalVariables { private get; set; }
        public Action SetupAdditionalConstraints { private get; set; }
        public Action<GRBLinExpr> SetupAdditionalObjectives { private get; set; }

        #endregion

        #region Objective weights

        private readonly double _mBalanceWeight = 100;
        private readonly double _mStorageWeight = 10;

        #endregion

        #region Public methods

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="n"> problem size (number of nodes) </param>
        /// <param name="m"> number of storage levels </param>
        public CoreOptimizer(int n, int m)
        {
            _mEnv = new GRBEnv();
            _mEnv.Set(GRB.IntParam.LogToConsole, 0);
            N = n;

            Wrap = new ModelWrapper3(_mEnv, n, m);
        }

        /// <summary>
        /// Set the network edges.
        /// </summary>
        /// <param name="edges"> edges </param>
        public void SetEdges(EdgeCollection edges)
        {
            if (edges.NodeCount != N)
            {
                throw new ArgumentException("Dismension mismatch between edges and FlowOptimizer.");
            }

            Edges = edges;

            SetupVariables(Wrap);
        }

        /// <summary>
        /// Set the network nodes.
        /// </summary>
        /// <param name="nodes"> nodes </param>
        /// <param name="lowLimits"> lower limit (discharge) </param>
        /// <param name="highLimits"> higher limit (charge) </param>
        public void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits)
        {
            var now = DateTime.Now;
            if (nodes.Length != N)
            {
                throw new ArgumentException("Dismension mismatch between nodes and FlowOptimizer.");
            }

            Deltas = nodes;

            if (!Ready) return;

            SetConstraints(Wrap, lowLimits, highLimits);
            SetBalanceObjective();

            // Update model.
            if (DebugLog) Console.WriteLine("Setup: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
            //_mFlow.Model.Update();
            if (DebugLog) Console.WriteLine("Flow: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
            Wrap.Model.Update();
            if (DebugLog) Console.WriteLine("Balance: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

        }

        public void Solve()
        {
            if (!Ready) throw new ArgumentException(SolveError());
            DateTime now = DateTime.Now;

            // Solve models.
            Wrap.Model.Optimize();
            if (DebugLog) Console.WriteLine("Solve balance: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

            // Make results publicly available.
            if(OnSolveCompleted != null) OnSolveCompleted();
            if (DebugLog) Console.WriteLine("Extract results: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

            // Remove old constraints.
            foreach (var constr in Wrap.Constraints) Wrap.Model.Remove(constr);
            if (DebugLog) Console.WriteLine("Remove constraints: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
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
        private void SetConstraints(ModelWrapper3 m, List<double[]> lowLimits, List<double[]> highLimits)
        {
            // Update nodal balancing objectives. 
            for (int i = 0; i < N; i++)
            {
                m.NodeExprs[i] = 0.0;
                var sign = (Deltas[i] > 0) ? 1 : -1;
                // Add mismatch.
                m.NodeExprs[i].AddConstant(sign * Deltas[i]);
                // Add edges.
                for (int j = 0; j < N; j++)
                {
                    if (Edges.EdgeExists(i, j)) m.NodeExprs[i].AddTerm(+sign, m.Edges[i + N * j]);
                    if (Edges.EdgeExists(j, i)) m.NodeExprs[i].AddTerm(-sign, m.Edges[j + N * i]);
                }
                // Add storage.
                for (int k = 0; k < m.Storages.Count; k++)
                {
                    m.NodeExprs[i].AddTerm(sign, m.Storages[k][i]);
                    m.Storages[k][i].Set(GRB.DoubleAttr.LB, lowLimits[k][i]);
                    m.Storages[k][i].Set(GRB.DoubleAttr.UB, highLimits[k][i]);
                }
                // Ensure that too much balancing is not applied.
                m.Constraints[i] = m.Model.AddConstr(m.NodeExprs[i], GRB.GREATER_EQUAL, 0, "const" + i);
            }
        }

        #region Setup

        /// <summary>
        /// Set variables (when edges change).
        /// </summary>
        private void SetupVariables(ModelWrapper3 m)
        {
            for (int i = 0; i < N; i++)
            {
                // Add storage.
                for (int j = 0; j < m.Storages.Count; j++)
                {
                    m.Storages[j][i] = m.Model.AddVar(-double.MaxValue, double.MaxValue, 0, Precision, "storage" + i + "level" + j);
                    m.StorageDummies[j][i] = m.Model.AddVar(-double.MaxValue, double.MaxValue, 0, Precision, "storageDummy" + i + "level" + j);
                }
                // Add edges.
                for (int j = i; j < N; j++)
                {
                    if (!Edges.Connected(i, j)) continue;
                    var cap = Edges.GetEdgeCapacity(i, j);
                    m.Edges.Add(i + j * N, m.Model.AddVar(-cap, cap, 0, Precision, "edge" + i + j));
                }
            }

            if(SetupAdditionalVariables != null) SetupAdditionalVariables();
            m.Model.Update();

            // Add storage dummy varaible constraints.
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < m.Storages.Count; j++)
                {
                    var dummy = m.StorageDummies[j][i];
                    m.Model.AddConstr(dummy, GRB.GREATER_EQUAL, m.Storages[j][i], "storageDummyConstPlus" + i + "level" + j);
                    m.Model.AddConstr(dummy, GRB.GREATER_EQUAL, m.Storages[j][i], "storageDummyConstPlus" + i + "level" + j);
                }
            }

            if(SetupAdditionalConstraints != null) SetupAdditionalConstraints();
            m.Model.Update();
        }

        /// <summary>
        /// Set balance objective (when nodes change).
        /// </summary>
        private void SetBalanceObjective()
        {
            GRBLinExpr obj = 0.0;

            // Main objective: Minimize balancing.
            foreach (var expr in Wrap.NodeExprs) obj.MultAdd(_mBalanceWeight, expr);
            // Secondary objective: Low storage usage.
            for (int i = 0; i < Wrap.StorageDummies.Count; i++)
            {
                var level = Wrap.StorageDummies[i];
                foreach (var dummy in level) obj.MultAdd((i + 1) * _mStorageWeight, dummy);
            }
            if (SetupAdditionalObjectives != null) SetupAdditionalObjectives(obj);

            Wrap.Model.SetObjective(obj, GRB.MINIMIZE);
        }

        #endregion

        #endregion

        #region Help methods

        private string SolveError()
        {
            return "Cannot optimize flow; variables not set: " + (NodesSet ? "" : "nodes ") +
                   (EdgesSet ? "" : "edges");
        }

        #endregion

    }

    public class ModelWrapper3
    {
        // Edge variables.
        public Dictionary<int, GRBVar> Edges { get; set; }
        //public Dictionary<int, GRBVar> EdgeDummies { get; set; }
        // Storage variables.
        public List<GRBVar[]> Storages { get; set; }
        public List<GRBVar[]> StorageDummies { get; set; }
        // Nodal balancing expressions.
        public GRBLinExpr[] NodeExprs { get; set; }
        // Model constraints.
        public GRBConstr[] Constraints { get; set; }

        public GRBModel Model { get; set; }

        public ModelWrapper3(GRBEnv env, int n, int m)
        {
            Model = new GRBModel(env);

            //EdgeDummies = new Dictionary<int, GRBVar>();
            Edges = new Dictionary<int, GRBVar>();

            Constraints = new GRBConstr[n];
            NodeExprs = new GRBLinExpr[n];

            Storages = new List<GRBVar[]>();
            StorageDummies = new List<GRBVar[]>();
            for (int i = 0; i < m; i++)
            {
                Storages.Add(new GRBVar[n]);
                StorageDummies.Add(new GRBVar[n]);
            }
        }

    }
}
