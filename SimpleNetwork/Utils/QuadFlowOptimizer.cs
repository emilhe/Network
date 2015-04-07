using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using Gurobi;

namespace BusinessLogic.Utils
{
    class QuadFlowOptimizer : IOptimizer
    {

        public bool ExtractFlows { get; set; }
        public bool ExtractStorageOptima { get; set; }

        public double[,] Flows { get; private set; }
        public double[] NodeOptima { get; private set; }
        public List<double[]> StorageOptima { get; private set; }

        private readonly CoreOptimizer _mCore;
        private readonly GRBQuadExpr _mFlowObjective;
        private readonly Action _mApplyConstr;
        private readonly Action _mRemoveConstr; 

        public QuadFlowOptimizer(CoreOptimizer core, Action applyConstr, Action removeConstr)
        {
            _mCore = core;
            core.OnSolveCompleted = SolveQuadratic;
            _mFlowObjective = ObjectiveFactory.SquaredFlow(_mCore.Edges, Wrap);
            _mRemoveConstr = removeConstr;
            _mApplyConstr = applyConstr;

            // Setup data structures.
            Flows = new double[N, N];
            NodeOptima = new double[N];
            StorageOptima = new List<double[]>();
            for (int i = 0; i < _mCore.M; i++) StorageOptima.Add(new double[N]);

            // So far, just track it all.
            ExtractFlows = true;
            ExtractStorageOptima = true;
        }

        private void SolveQuadratic()
        {
            // Add new constraints.
            _mApplyConstr();
            // Set new balancing objective and optimize.
            _mCore.Wrap.Model.SetObjective(_mFlowObjective);
            _mCore.Wrap.Model.Update();
            _mCore.Wrap.Model.Optimize();
            // Extract results.
            ExtractResultsFromModel();
            // Remove new constraints.
            _mRemoveConstr();

            if (OnSolveCompleted != null) OnSolveCompleted();
        }

        private void ExtractResultsFromModel()
        {
            int status = _mCore.Wrap.Model.Get(GRB.IntAttr.Status);
            if (status == GRB.Status.INFEASIBLE)
            {
                _mCore.Wrap.Model.ComputeIIS();
                _mCore.Wrap.Model.Write(@"C:\Temp\flowModel.ilp");
            }
            if (status == GRB.Status.NUMERIC)
            {
                throw new ArgumentException("Gurobi had numerical difficulties...");
            }
            if (status != GRB.Status.OPTIMAL && status != GRB.Status.SUBOPTIMAL)
            {
                Console.WriteLine("Optimization was stopped with status " + status);
                throw new ArgumentException("Model fucked up!");
            }

            // Extract node optima.
            for (int i = 0; i < _mCore.N; i++)
            {
                NodeOptima[i] = _mCore.Wrap.NodeExprs[i].Value;
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

                        Flows[i, j] = _mCore.Wrap.Edges[i + _mCore.N*j].Get(GRB.DoubleAttr.X);
                    }
                }
            }
        }

        public Action OnSolveCompleted { private get; set; }

        #region Delegation

        public void Solve()
        {
            ((IOptimizer)_mCore).Solve();
        }

        public int N
        {
            get { return _mCore.N; }
        }

        public int M
        {
            get { return _mCore.M; }
        }

        public EdgeCollection Edges
        {
            get { return _mCore.Edges; }
        }

        public MyModel Wrap
        {
            get { return _mCore.Wrap; }
        }

        public double[] Deltas
        {
            get { return _mCore.Mismatches; }
        }

        public void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits)
        {
            ((IOptimizer)_mCore).SetNodes(nodes, lowLimits, highLimits);
        }

        #endregion

    }

}
