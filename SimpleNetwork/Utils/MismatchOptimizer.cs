using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;

namespace BusinessLogic.Utils
{
    public class MismatchOptimizer
    {

        private const char Precision = GRB.CONTINUOUS;

        #region Fields

        private readonly double[,] _mFlows;
        private readonly double[] _mNodeOptimum;

        public bool Ready { get { return NodesSet && EdgesSet; } }        
        private bool NodesSet {get { return _mDeltas != null; }}
        private bool EdgesSet { get { return _mEdges != null; } }

        private GRBConstr[] _mConstr;

        private readonly GRBEnv _mEnv;
        private readonly GRBModel _mModel;
        private GRBVar[] _mStorageVariables;
        private GRBVar[,] _mEdgeVariables;

        private EdgeSet _mEdges;
        private double[] _mDeltas;
        private readonly int _mN;

        #endregion

        #region Public methods

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="n"> problem size (number of nodes) </param>
        public MismatchOptimizer(int n)
        {
            _mEnv = new GRBEnv();
            _mEnv.Set(GRB.IntParam.LogToConsole, 0);
            _mModel = new GRBModel(_mEnv);
            _mN = n;

            _mFlows = new double[_mN, _mN];
            _mNodeOptimum = new double[_mN];
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
            SetVariables();

            if (!Ready) return;
            
            SetObjective();                
            //RefreshConstraints();

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

            // Optimize call solves the problem.
            _mModel.Optimize();

            // Make results publicly available.
            ExtractResultsFromModel();

            return _mNodeOptimum.Sum();
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
                    if (!_mEdges.Connected(i, j)) continue;

                    _mFlows[i, j] = _mEdgeVariables[i, j].Get(GRB.DoubleAttr.X);
                }
            }

            for (int i = 0; i < _mN; i++)
            {
                // Starting with initial value.
                _mNodeOptimum[i] = _mDeltas[i] + _mStorageVariables[i].Get(GRB.DoubleAttr.X);
                // Subtract/Add export/imports.
                for (int j = 0; j < _mN; j++)
                {
                    _mNodeOptimum[i] = _mNodeOptimum[i] - _mFlows[i, j] + _mFlows[j, i];
                }
            }
        }

        /// <summary>
        /// Set variables (when edges change).
        /// </summary>
        private void SetVariables()
        {
            _mStorageVariables = new GRBVar[_mN];
            _mEdgeVariables = new GRBVar[_mN, _mN];

            for (int i = 0; i < _mN; i++)
            {
                _mStorageVariables[i] = _mModel.AddVar(-int.MaxValue, int.MaxValue, 0, Precision, "storage" + i);
                for (int j = 0; j < _mN; j++)
                {
                    // TODO: IMPOSE EDGE LIMITATIONS HERE!!
                    if (!_mEdges.Connected(i, j)) continue;
                    _mEdgeVariables[i, j] = _mModel.AddVar(-int.MaxValue, int.MaxValue, 0, Precision, "edge" + i + j);
                }
            }

            _mModel.Update(); 
        }

        /// <summary>
        /// Set objective (when edges change).
        /// </summary>
        private void SetObjective()
        {
            var firstRun = (_mConstr == null);
            if(firstRun) _mConstr = new GRBConstr[_mN];

            GRBLinExpr obj = 0.0;
            for (int i = 0; i < _mN; i++)
            {
                GRBLinExpr expr = 0.0;
                // Add mismatch and storage.
                var pos = (_mDeltas[i] > 0);
                expr.AddConstant(pos ? _mDeltas[i] : -_mDeltas[i]);
                expr.AddTerm(pos ? 1 : -1, _mStorageVariables[i]);
                // Add edges.
                for (int j = 0; j < _mN; j++)
                {
                    if (_mEdges.Connected(i, j)) expr.AddTerm(pos ? -_mEdges.GetEdgeCost(i, j) : _mEdges.GetEdgeCost(i, j), _mEdgeVariables[i, j]);
                    if (_mEdges.Connected(j, i)) expr.AddTerm(pos ? _mEdges.GetEdgeCost(i, j) : -_mEdges.GetEdgeCost(i, j), _mEdgeVariables[j, i]);
                }
                if(!firstRun) _mModel.Remove(_mConstr[i]);
                _mConstr[i] = _mModel.AddConstr(expr, GRB.GREATER_EQUAL, 0, "node" + i);
                obj.Add(expr);
            }

            _mModel.SetObjective(obj, GRB.MINIMIZE);
        }

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
