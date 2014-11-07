using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;

namespace BusinessLogic.Utils
{
    public class ConstrainedFlowOptimizer
    {

        public double Tolerance { get { return 1E-10; } }

        public double[,] Flows { get; private set; }
        public double[] NodeOptimum { get; private set; }
        public double[] StorageOptimum { get; private set; }

        public double OptimalBalance { get; private set; }

        #region Fields

        private const char Precision = GRB.CONTINUOUS;
        private const bool DebugLog = false;

        private bool Ready { get { return NodesSet && EdgesSet; } }        
        private bool NodesSet {get { return _mDeltas != null; }}
        private bool EdgesSet { get { return _mEdges != null; } }

        private readonly GRBEnv _mEnv;
        private readonly int _mN;
        private EdgeSet _mEdges;

        private readonly ModelWrapper _mFlow;
        private readonly ModelWrapper _mBalance;
        private GRBConstr _mOptimumConstr;

        private double[] _mDeltas;

        #endregion

        #region Public methods

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="n"> problem size (number of nodes) </param>
        public ConstrainedFlowOptimizer(int n)
        {
            _mEnv = new GRBEnv();
            _mEnv.Set(GRB.IntParam.LogToConsole, 0);
            _mN = n;

            _mFlow = new ModelWrapper(_mEnv, n);
            _mBalance = new ModelWrapper(_mEnv, n);

            Flows = new double[_mN, _mN];
            NodeOptimum = new double[_mN];
            StorageOptimum = new double[_mN];
        }

        /// <summary>
        /// Set the network edges.
        /// </summary>
        /// <param name="edges"> edges </param>
        public void SetEdges(EdgeSet edges)
        {
            if (edges.NodeCount != _mN)
            {
                throw new ArgumentException("Dismension mismatch between edges and FlowOptimizer.");
            }

            _mEdges = edges;
            
            SetupVariables(_mFlow);
            SetupVariables(_mBalance);

            SetFlowObjective();
        }

        /// <summary>
        /// Set the network nodes.
        /// </summary>
        /// <param name="nodes"> nodes </param>
        /// <param name="lowLimits"> lower limit (discharge) </param>
        /// <param name="highLimits"> higher limit (charge) </param>
        public void SetNodes(double[] nodes, double[] lowLimits, double[] highLimits)
        {
            var now = DateTime.Now;
            if (nodes.Length != _mN)
            {
                throw new ArgumentException("Dismension mismatch between nodes and FlowOptimizer.");
            }

            _mDeltas = nodes;

            if (!Ready) return;

            SetConstraints(_mBalance, lowLimits, highLimits);
            SetConstraints(_mFlow, lowLimits, highLimits);
            SetBalanceObjective();

            // Update model.
            if(DebugLog) Console.WriteLine("Setup: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
            _mFlow.Model.Update();
            if (DebugLog) Console.WriteLine("Flow: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
            _mBalance.Model.Update();
            if (DebugLog) Console.WriteLine("Balance: {0}", DateTime.Now.Subtract(now).TotalMilliseconds); 

        }

        public void Solve()
        {
            var now = DateTime.Now;
            if (!Ready) throw new ArgumentException(SolveError());

            // Solve models.
            SolveBalanceModel();
            if (DebugLog) Console.WriteLine("Solve balance: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
            SolveFlowModel();
            if (DebugLog) Console.WriteLine("Solve flow: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

            // Make results publicly available.
            ExtractResultsFromModel();
            if (DebugLog) Console.WriteLine("Extract results: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

            // Remove old constraints.
            _mFlow.Model.Remove(_mOptimumConstr);
            foreach (var constr in _mFlow.Constraints) _mFlow.Model.Remove(constr);
            foreach (var constr in _mBalance.Constraints) _mBalance.Model.Remove(constr);
            if (DebugLog) Console.WriteLine("Remove constraints: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
        }

        private void SolveBalanceModel()
        {
            // First, solve to get optimal balance results.
            _mBalance.Model.Optimize();
            OptimalBalance = 0;
            foreach (var expr in _mBalance.NodeExprs) OptimalBalance += expr.Value;
        }

        private void SolveFlowModel()
        {
            // Next, solve to minimize flow.
            GRBLinExpr sum = 0.0;
            foreach (var expr in _mFlow.NodeExprs) sum.Add(expr);
            _mOptimumConstr = _mFlow.Model.AddConstr(sum, GRB.LESS_EQUAL, OptimalBalance + _mDeltas.Length * Tolerance, "Optimal balance");
            //foreach (var constr in _mConstr) constr.Set(GRB.DoubleAttr.RHS, -Tolerance);
            _mFlow.Model.Update();
            _mFlow.Model.Optimize();
        }

        public void Dispose()
        {
            _mEnv.Dispose(); 
        }

        #endregion

        #region Private methods

        private void ExtractResultsFromModel()
        {
            var now = DateTime.Now;
            for (int i = 0; i < _mN; i++)
            {
                // Starting with initial value.
                StorageOptimum[i] = _mFlow.StorageVariables[i].Get(GRB.DoubleAttr.X);
            }
            if (DebugLog) Console.WriteLine("Extract storage: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

            for (int i = 0; i < _mN; i++)
            {
                for (int j = 0; j < _mN; j++)
                {
                    if (!_mEdges.EdgeExists(i, j)) continue;

                    Flows[i, j] = _mFlow.EdgeVariables[i, j].Get(GRB.DoubleAttr.X);
                }
            }
            if (DebugLog) Console.WriteLine("Extract edges: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);

            for (int i = 0; i < _mN; i++)
            {
                // Starting with initial value.
                NodeOptimum[i] = _mDeltas[i] + StorageOptimum[i];
                // Subtract/Add export/imports.
                for (int j = 0; j < _mN; j++)
                {
                    NodeOptimum[i] = NodeOptimum[i] - Flows[i, j] + Flows[j, i];
                }
            }
            if (DebugLog) Console.WriteLine("Calc opts: {0}", DateTime.Now.Subtract(now).TotalMilliseconds);
        }

        /// <summary>
        /// Set objective (when edges change).
        /// </summary>
        private void SetConstraints(ModelWrapper wrapper, double[] lowLimits, double[] highLimits)
        {
            // Update storage boundaries.
            for (int i = 0; i < _mN; i++)
            {
                wrapper.StorageVariables[i].Set(GRB.DoubleAttr.LB, lowLimits[i]);
                wrapper.StorageVariables[i].Set(GRB.DoubleAttr.UB, highLimits[i]);
            }          
            // Update abs objectives.
            for (int i = 0; i < _mN; i++)
            {
                wrapper.NodeExprs[i] = 0.0;
                // Add mismatch and storage.
                var pos = (_mDeltas[i] > 0);
                wrapper.NodeExprs[i].AddConstant(pos ? _mDeltas[i] : -_mDeltas[i]);
                wrapper.NodeExprs[i].AddTerm(pos ? 1 : -1, wrapper.StorageVariables[i]);
                // Add edges.
                for (int j = 0; j < _mN; j++)   
                {
                    if (_mEdges.EdgeExists(i, j))
                    {
                        wrapper.NodeExprs[i].AddTerm(pos ? -_mEdges.GetEdgeCost(i, j) : _mEdges.GetEdgeCost(i, j), wrapper.EdgeVariables[i, j]);
                    }
                    if (_mEdges.EdgeExists(j, i))
                    {
                        wrapper.NodeExprs[i].AddTerm(pos ? _mEdges.GetEdgeCost(j, i) : -_mEdges.GetEdgeCost(j, i), wrapper.EdgeVariables[j, i]);
                    }
                }
                wrapper.Constraints[i] = wrapper.Model.AddConstr(wrapper.NodeExprs[i], GRB.GREATER_EQUAL, 0, "node" + i);
            }
        }

        #region Setup

        /// <summary>
        /// Set variables (when edges change).
        /// </summary>
        private void SetupVariables(ModelWrapper wrapper)
        {
            for (int i = 0; i < _mN; i++)
            {
                wrapper.StorageVariables[i] = wrapper.Model.AddVar(-double.MaxValue, double.MaxValue, 0, Precision, "storage" + i);
                for (int j = 0; j < _mN; j++)
                {
                    if (!_mEdges.EdgeExists(i, j)) continue;
                    //wrapper.EdgeVariables[i, j] = wrapper.Model.AddVar(0, _mEdges.GetEdgeCapacity(i, j), 0, Precision, "edge" + i + j);
                    wrapper.EdgeVariables[i, j] = wrapper.Model.AddVar(-_mEdges.GetEdgeCapacity(i, j), _mEdges.GetEdgeCapacity(i, j), 0, Precision, "edge" + i + j);
                }
            }

            wrapper.Model.Update();
        }

        /// <summary>
        /// Set flow objective (when edges change).
        /// </summary>
        private void SetFlowObjective()
        {
            GRBQuadExpr flowObjective = 0.0;

            for (int i = 0; i < _mN; i++)
            {
                for (int j = 0; j < _mN; j++)
                {
                    if (!_mEdges.EdgeExists(i, j)) continue;
                    // Note that the SQUARED flow is minimized (to minimize the needed capacity).
                    flowObjective.AddTerm(_mEdges.GetEdgeCost(i, j), _mFlow.EdgeVariables[i, j], _mFlow.EdgeVariables[i, j]);
                }
            }

            _mFlow.Model.SetObjective(flowObjective);
        }

        /// <summary>
        /// Set balance objective (when nodes change).
        /// </summary>
        private void SetBalanceObjective()
        {
            GRBLinExpr obj = 0.0;
            foreach (var expr in _mBalance.NodeExprs) obj.Add(expr);
            _mBalance.Model.SetObjective(obj, GRB.MINIMIZE);
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

    public class ModelWrapper
    {

        public GRBVar[] StorageVariables { get; set; }
        public GRBConstr[] Constraints { get; set; }
        public GRBLinExpr[] NodeExprs { get; set; }
        public GRBVar[,] EdgeVariables { get; set; }
        public GRBModel Model { get; set; }

        public ModelWrapper(GRBEnv env, int n)
        {
            Model = new GRBModel(env);
            StorageVariables = new GRBVar[n];
            Constraints = new GRBConstr[n];
            NodeExprs = new GRBLinExpr[n];
            EdgeVariables = new GRBVar[n,n];
        }

    }
}
