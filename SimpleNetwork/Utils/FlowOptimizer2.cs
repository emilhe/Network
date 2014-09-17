using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;

namespace BusinessLogic.Utils
{
    public class FlowOptimizer2
    {
     
        private const char Precision = GRB.CONTINUOUS;

        #region Fields

        public double[,] Flows { get; private set; }
        public double[] NodeOptimum { get; private set; }

        public bool Ready { get { return NodesSet && EdgesSet; } }        
        private bool NodesSet {get { return _mDeltas != null; }}
        private bool EdgesSet { get { return _mEdges != null; } }

        private readonly GRBEnv _mEnv;
        private readonly GRBModel _mModel;
        private GRBVar[,] _mEdgeVariables;
        private GRBQuadExpr _mFlowObjective;

        private GRBVar[] _mStorageVariables;
        private GRBConstr[] _mConstr;
        private GRBLinExpr[] _mNodeExprs;

        private EdgeSet _mEdges;
        private double[] _mDeltas;
        private readonly int _mN;

        #endregion

        public double OptimalBalance { get; private set; }

        #region Public methods

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="n"> problem size (number of nodes) </param>
        public FlowOptimizer2(int n)
        {
            _mEnv = new GRBEnv();
            _mEnv.Set(GRB.IntParam.LogToConsole, 0);
            _mModel = new GRBModel(_mEnv);
            _mN = n;

            Flows = new double[_mN, _mN];
            NodeOptimum = new double[_mN];

            _mConstr = new GRBConstr[_mN];
            _mNodeExprs = new GRBLinExpr[_mN];
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
            SetupVariables();
            SetupFlowObjective();

            if (!Ready) return;
            
            SetObjective();                

            _mModel.Update();
        }

        /// <summary>
        /// Set the network nodes.
        /// </summary>
        /// <param name="nodes"> nodes </param>
        /// <param name="lowLimits"> lower limit (discharge) </param>
        /// <param name="highLimits"> higher limit (charge) </param>
        public void SetNodes(double[] nodes, double[] lowLimits, double[] highLimits)
        {
            if (nodes.Length != _mN)
            {
                throw new ArgumentException("Dismension mismatch between nodes and FlowOptimizer.");
            }

            _mDeltas = nodes;

            if (!Ready) return;

            SetObjective();

            // Update storage boundaries.
            for (int i = 0; i < _mStorageVariables.Length; i++)
            {
                _mStorageVariables[i].Set(GRB.DoubleAttr.LB, lowLimits[i]);
                _mStorageVariables[i].Set(GRB.DoubleAttr.UB, highLimits[i]);
            }
            // Update 

            _mModel.Update();
        }

        public double Solve()
        {
            if (!Ready) throw new ArgumentException(SolveError());

            // First, solve to get optimal balance results.
            _mModel.Optimize();
            OptimalBalance = 0;
            foreach (GRBLinExpr expr in _mNodeExprs) OptimalBalance += expr.Value;
            // Next, solve to minimize flow.
            GRBLinExpr sum = 0.0;
            foreach (var expr in _mNodeExprs) sum.Add(expr);
            var bal = _mModel.AddConstr(sum, GRB.EQUAL, OptimalBalance, "Optimal balance");
            _mModel.SetObjective(_mFlowObjective);
            _mModel.Update();
            _mModel.Optimize();

            _mModel.Remove(bal);
            foreach (var constr in _mConstr) _mModel.Remove(constr);

            // Make results publicly available.
            ExtractResultsFromModel();

            return OptimalBalance;
        }

        public void Dispose()
        {
            _mEnv.Dispose(); 
        }

        #endregion

        #region Private methods

        private void ExtractResultsFromModel()
        {
            for (int i = 0; i < _mN; i++)
            {
                for (int j = 0; j < _mN; j++)
                {
                    if (!_mEdges.EdgeExists(i, j)) continue;

                    Flows[i, j] = _mEdgeVariables[i, j].Get(GRB.DoubleAttr.X);
                }
            }

            for (int i = 0; i < _mN; i++)
            {
                // Starting with initial value.
                NodeOptimum[i] = _mDeltas[i] + _mStorageVariables[i].Get(GRB.DoubleAttr.X);
                // Subtract/Add export/imports.
                for (int j = 0; j < _mN; j++)
                {
                    NodeOptimum[i] = NodeOptimum[i] - Flows[i, j] + Flows[j, i];
                }
            }
        }


        /// <summary>
        /// Set objective (when edges change).
        /// </summary>
        private void SetObjective()
        {
            GRBLinExpr obj = 0.0;
            for (int i = 0; i < _mN; i++)
            {
                _mNodeExprs[i] = 0.0;
                // Add mismatch and storage.
                var pos = (_mDeltas[i] > 0);
                _mNodeExprs[i].AddConstant(pos ? _mDeltas[i] : -_mDeltas[i]);
                _mNodeExprs[i].AddTerm(pos ? 1 : -1, _mStorageVariables[i]);
                // Add edges.
                for (int j = 0; j < _mN; j++)
                {
                    if (_mEdges.EdgeExists(i, j)) _mNodeExprs[i].AddTerm(pos ? -_mEdges.GetEdgeCost(i, j) : _mEdges.GetEdgeCost(i, j), _mEdgeVariables[i, j]);
                    if (_mEdges.EdgeExists(j, i)) _mNodeExprs[i].AddTerm(pos ? _mEdges.GetEdgeCost(i, j) : -_mEdges.GetEdgeCost(i, j), _mEdgeVariables[j, i]);
                }
                _mConstr[i] = _mModel.AddConstr(_mNodeExprs[i], GRB.GREATER_EQUAL, 0, "node" + i);
                obj.Add(_mNodeExprs[i]);
            }

            _mModel.SetObjective(obj, GRB.MINIMIZE);
        }

        #region Setup

        /// <summary>
        /// Set variables (when edges change).
        /// </summary>
        private void SetupVariables()
        {
            _mStorageVariables = new GRBVar[_mN];
            _mEdgeVariables = new GRBVar[_mN, _mN];

            for (int i = 0; i < _mN; i++)
            {
                _mStorageVariables[i] = _mModel.AddVar(-int.MaxValue, int.MaxValue, 0, Precision, "storage" + i);
                for (int j = 0; j < _mN; j++)
                {
                    // TODO: IMPOSE EDGE LIMITATIONS HERE!!
                    if (!_mEdges.EdgeExists(i, j)) continue;
                    _mEdgeVariables[i, j] = _mModel.AddVar(-int.MaxValue, int.MaxValue, 0, Precision, "edge" + i + j);
                }
            }

            _mModel.Update(); 
        }

        /// <summary>
        /// Set objective (when edges change).
        /// </summary>
        private void SetupFlowObjective()
        {
            _mFlowObjective = 0.0;
            for (int i = 0; i < _mN; i++)
            {
                for (int j = 0; j < _mN; j++)
                {
                    if (!_mEdges.EdgeExists(i, j)) continue;
                    // Note that the SQUARED flow is minimized (to minimize the needed capacity).
                    _mFlowObjective.AddTerm(_mEdges.GetEdgeCost(i, j), _mEdgeVariables[i, j], _mEdgeVariables[i, j]);
                }
            }
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
}
