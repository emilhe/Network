using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using Gurobi;

namespace BusinessLogic.Utils
{
    class SystemOptimizer : IOptimizer
    {

        public double BalanceOptimum { get; private set; }
        public List<double> StorageOptima { get; private set; }

        private readonly CoreOptimizer _mCore;

        public SystemOptimizer(int n, int m)
        {
            _mCore = new CoreOptimizer(n, m) {OnSolveCompleted = ExtractResultsFromModel};
            StorageOptima = new List<double>(m);
        }

        private void ExtractResultsFromModel()
        {
            try
            {
                BalanceOptimum = 0;
                foreach (var expr in _mCore.Wrap.NodeExprs) BalanceOptimum += expr.Value;

                for (int level = 0; level < StorageOptima.Count; level++)
                {
                    StorageOptima[level] = 0;
                    foreach (var variable in _mCore.Wrap.Storages[level])
                        StorageOptima[level] += variable.Get(GRB.DoubleAttr.X);
                }
            }
            catch (Exception e)
            {
                _mCore.Wrap.Model.ComputeIIS();
                _mCore.Wrap.Model.Write(@"C:\Temp\balanceModel.ilp");
                throw;
            }
            finally
            {
                if (OnSolveCompleted != null) OnSolveCompleted();
            }
        }

        #region Delegation

        public Action OnSolveCompleted { private get; set; }

        public int N
        {
            get { return _mCore.N; }
        }

        public EdgeCollection Edges
        {
            get { return _mCore.Edges; }
        }

        public ModelWrapper3 Wrap
        {
            get { return _mCore.Wrap; }
        }

        public double[] Deltas
        {
            get { return _mCore.Deltas; }
        }

        public void SetEdges(EdgeCollection edges)
        {
            ((IOptimizer) _mCore).SetEdges(edges);
        }

        public void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits)
        {
            ((IOptimizer) _mCore).SetNodes(nodes, lowLimits, highLimits);
        }

        public void Solve()
        {
            ((IOptimizer) _mCore).Solve();
        }

        #endregion

    }
}
