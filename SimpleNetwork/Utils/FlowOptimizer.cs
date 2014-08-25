using System;
using System.Collections.Generic;
using Gurobi;

namespace SimpleNetwork
{
    public class FlowOptimizer : IDisposable
    {

        public double[,] Flows { get; private set; }
        public double[] NodeOptimum { get; private set; }

        private const char Precision = GRB.CONTINUOUS;

        /// <summary>
        /// If the flow precision is INTGER, an uncertainty of 0.99 is introduced to ensure termination.
        /// </summary>
        private double ConvParam
        {
            get { return Precision == GRB.INTEGER ? 0.99 : 0; }
        }

        #region Fields

        public bool Ready { get { return NodesSet && EdgesSet; } }        
        private bool NodesSet {get { return _mDeltas != null; }}
        private bool EdgesSet { get { return _mEdges != null; } }

        private readonly GRBEnv _mEnv;
        private readonly GRBModel _mModel;
        private GRBVar[,] _mVariables;
        private GRBConstr[] _mCachedConstrLoLims;
        private GRBConstr[] _mCachedConstrHiLims;

        private EdgeSet _mEdges;
        private double[] _mDeltas;
        private double[] _mLoLims;
        private double[] _mHiLims;
        private readonly int _mN;

        #endregion

        #region Public methods

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="n"> problem size (number of nodes) </param>
        public FlowOptimizer(int n)
        {
            _mEnv = new GRBEnv();
            _mEnv.Set(GRB.IntParam.LogToConsole, 0);
            _mModel = new GRBModel(_mEnv);
            _mN = n;

            Flows = new double[_mN, _mN];
            NodeOptimum = new double[_mN];
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
            SetObjective();

            if (Ready) RefreshConstraints(); 
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
            _mLoLims = lowLimits;
            _mHiLims = highLimits;

            if (Ready) RefreshConstraints();          
        }

        public void Solve()
        {
            if (!Ready) throw new ArgumentException(SolveError());

            // Optimize call solves the problem.
            _mModel.Optimize();

            // Make results publicly available.
            ExtractResultsFromModel();
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

                    Flows[i, j] = _mVariables[i, j].Get(GRB.DoubleAttr.X);
                }
            }

            for (int i = 0; i < _mN; i++)
            {
                // Starting with initial value.
                NodeOptimum[i] = _mDeltas[i];
                // Subtract/Add export/imports.
                for (int j = 0; j < _mN; j++)
                {
                    NodeOptimum[i] = NodeOptimum[i] - Flows[i, j] + Flows[j, i];
                }
            }
        }

        /// <summary>
        /// Update constraints (when model is ready).
        /// </summary>
        private void RefreshConstraints()
        {
            // Refresh constraints.
            if (_mCachedConstrLoLims != null && _mCachedConstrHiLims != null)
            {
                for (int i = 0; i < _mN; i++)
                {
                    _mCachedConstrLoLims[i].Set(GRB.DoubleAttr.RHS, (_mLoLims[i] - _mDeltas[i] - ConvParam));
                    _mCachedConstrHiLims[i].Set(GRB.DoubleAttr.RHS, (_mHiLims[i] - _mDeltas[i] + ConvParam));

                }
                _mModel.Update();
                return;
            }
            
            // Add new constraints.
            _mCachedConstrLoLims = new GRBConstr[_mN];
            _mCachedConstrHiLims = new GRBConstr[_mN];
            for (int i = 0; i < _mN; i++)
            {
                GRBLinExpr cst = 0.0;
                for (int j = 0; j < _mN; j++)
                {
                    if (_mEdges.EdgeExists(i, j)) cst.AddTerm(-_mEdges.GetEdgeCost(i, j), _mVariables[i, j]);
                    if (_mEdges.EdgeExists(j, i)) cst.AddTerm(_mEdges.GetEdgeCost(j, i), _mVariables[j, i]);
                }
                _mCachedConstrLoLims[i] = _mModel.AddConstr(cst, GRB.GREATER_EQUAL, (_mLoLims[i] - _mDeltas[i] - ConvParam), "c" + i);
                _mCachedConstrHiLims[i] = _mModel.AddConstr(cst, GRB.LESS_EQUAL, (_mHiLims[i] - _mDeltas[i] + ConvParam), "c" + i);
            }
            _mModel.Update();
        }

        /// <summary>
        /// Set variables (when edges change).
        /// </summary>
        private void SetVariables()
        {
            _mVariables = new GRBVar[_mN, _mN];
            for (int i = 0; i < _mN; i++)
            {
                for (int j = 0; j < _mN; j++)
                {
                    if (!_mEdges.EdgeExists(i, j)) continue;
                    _mVariables[i, j] = _mModel.AddVar(0, int.MaxValue, 0, Precision, "x" + i + j);
                }
            }
            _mModel.Update(); 
        }

        /// <summary>
        /// Set objective (when edges change).
        /// </summary>
        private void SetObjective()
        {
            GRBQuadExpr obj = 0.0;
            for (int i = 0; i < _mN; i++)
            {
                for (int j = 0; j < _mN; j++)
                {
                    if (!_mEdges.EdgeExists(i, j)) continue;
                    // Note that the SQUARED flow is minimized (to minimize the needed capacity).
                    obj.AddTerm(_mEdges.GetEdgeCost(i, j), _mVariables[i, j], _mVariables[i, j]);
                }
            }
            _mModel.SetObjective(obj, GRB.MINIMIZE);
            _mModel.Update(); 
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

    public class EdgeSet
    {

        public int NodeCount { get { return _mNodeCount; } }
        public int EdgeCount { get { return _mEdgeCount; } }

        private readonly Dictionary<int, double> _mEdges;
        private readonly int _mNodeCount;
        private int _mEdgeCount;

        public EdgeSet(int nodeCount)
        {
            _mNodeCount = nodeCount;
            _mEdges = new Dictionary<int, double>();
            _mEdgeCount = 0;
        }

        /// <summary>
        /// Add edge to the edge set.
        /// </summary>
        /// <param name="i"> index the nodes to connect </param>
        /// <param name="j"> index the nodes to connect </param>
        public void AddEdge(int i, int j, double cost = 1)
        {
            if (i == j) throw new ArgumentException("Cannot connect a node to itself.");

            // The connection matric is to be constructed screw symmetric; we wan't positive on top.
            _mEdges.Add(i + j * _mNodeCount, cost);
            _mEdges.Add(j + i * _mNodeCount, cost);
            _mEdgeCount++;
        }

        /// <summary>
        /// Check is an edge exists (fast since a dictionary is used).
        /// </summary>
        /// <param name="i"> index the nodes to connect </param>
        /// <param name="j"> index the nodes to connect </param>
        /// <returns> true if the nodes are connected </returns>
        public bool EdgeExists(int i, int j)
        {
            // A node cannot be connected to itself.
            if (i == j) return false;

            return _mEdges.ContainsKey(i + j * _mNodeCount);
        }

        /// <summary>
        /// Get the cost of traversing an edge.
        /// </summary>
        /// <param name="i"> index the nodes to connect </param>
        /// <param name="j"> index the nodes to connect </param>
        /// <returns> the cost </returns>
        public double GetEdgeCost(int i, int j)
        {
            if (i == j) throw new ArgumentException("A node cannot be connected to itself.");

            // The connection matrix is screw symmetic.
            return _mEdges[i + j*_mNodeCount];
        }

    }

}
