using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using Gurobi;
using Utils;

namespace BusinessLogic.Utils
{
    public class QuadFlowOptimizer : IOptimizer
    {

        public bool ExtractFlows { get; set; }
        public bool ExtractStorageOptima { get; set; }

        public double[,] Flows { get; private set; }
        public double[] NodeOptima { get; private set; }
        public List<double[]> StorageOptima { get; private set; }

        private readonly CoreOptimizer _mCore;
        private readonly GRBQuadExpr _mFlowObjective;

        public Action ApplyConstr;
        public Action RemoveConstr;
        public Action SetTmpTol; 

        public QuadFlowOptimizer(CoreOptimizer core)
        {
            _mCore = core;
            core.OnSolveCompleted = SolveQuadratic;
            _mFlowObjective = ObjectiveFactory.SquaredFlow(_mCore.Edges, Wrap);

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
            if(ApplyConstr != null) ApplyConstr();
            // Set new balancing objective and optimize.
            _mCore.Wrap.Model.SetObjective(_mFlowObjective);
            _mCore.Wrap.Model.Update();
            _mCore.Wrap.Model.Optimize();
            // Extract results.
            ExtractResultsFromModel();
            // Remove new constraints.
            if (RemoveConstr != null) RemoveConstr();

            if (OnSolveCompleted != null) OnSolveCompleted();
        }

        private void ExtractResultsFromModel()
        {
            var status = _mCore.Wrap.Model.Get(GRB.IntAttr.Status);
            if (status == GRB.Status.INFEASIBLE)
            {
                _mCore.Wrap.Model.ComputeIIS();
                _mCore.Wrap.Model.Write(@"C:\Temp\flowModel.ilp");
            }
            if (status == GRB.Status.NUMERIC)
            {
                // Try increasing the tolerance if possible.
                if (SetTmpTol != null)
                {
                    SetTmpTol();
                    _mCore.Wrap.Model.Optimize();
                    status = _mCore.Wrap.Model.Get(GRB.IntAttr.Status);   
                }
                // In cause of failure, no flows are assumed.
                if (status == GRB.Status.NUMERIC)
                {
                    Console.WriteLine("Gurobi had numerical difficulties. Untable to fix. No flows were assumed.");
                    NodeOptima.Fill(i => Deltas[i]);
                    Flows.Fill(0);
                    foreach (var opt in StorageOptima) opt.Fill(0);
                    Deltas.ToJsonFile(string.Format(@"C:\proto\GUROBI_PROBLEM_{0}.txt", DateTime.Now.Millisecond));
                    return;
                }
                Console.WriteLine("Gurobi had numerical difficulties. Fixed by increased tolerance.");
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
