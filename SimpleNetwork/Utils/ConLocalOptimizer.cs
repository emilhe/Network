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
    public class ConLocalOptimizer : IOptimizer
    {

        private const double TmpTol = 1;

        private GRBConstr _mSystemBalancingConstr;
        private readonly List<GRBConstr> _mSystemStorageConstr;
        private readonly QuadFlowOptimizer _mCore;

        public ConLocalOptimizer(EdgeCollection edges, int m)
        {
            var core = new CoreOptimizer(edges, m, ObjectiveFactory.LinearBalancing);
            _mCore = new QuadFlowOptimizer(core)
            {
                ApplyConstr = ApplySystemConstr,
                RemoveConstr = RemoveSystemConstr,  
                SetTmpTol = SetTempTolerance
            };
            _mSystemStorageConstr = new List<GRBConstr>(m);
        }

        private void ApplySystemConstr()
        {
            GRBLinExpr sum = 0.0;
            foreach (var dummy in _mCore.Wrap.NodeExprsDummies)
            {
                sum.Add(dummy);
            }
            _mSystemBalancingConstr = _mCore.Wrap.Model.AddConstr(sum, GRB.LESS_EQUAL, sum.Value, "Optimal balance");

            _mSystemStorageConstr.Clear();
            for (int j = 0; j < _mCore.Wrap.Storages.Count; j++)
            {
                sum = 0.0;
                foreach (var dummy in _mCore.Wrap.StorageDummies[j])
                {
                    sum.Add(dummy);
                }
                _mSystemStorageConstr.Add(_mCore.Wrap.Model.AddConstr(sum, GRB.LESS_EQUAL, sum.Value,
                    "Optimal storage balance " + j));
            }
        }

        private void RemoveSystemConstr()
        {
            _mCore.Wrap.Model.Remove(_mSystemBalancingConstr);
            foreach (var constr in _mSystemStorageConstr) _mCore.Wrap.Model.Remove(constr);
        }

        public void SetTempTolerance()
        {
            var rhs = _mSystemBalancingConstr.Get(GRB.DoubleAttr.RHS) + TmpTol;
            _mCore.Wrap.Model.Remove(_mSystemBalancingConstr);
            GRBLinExpr sum = 0.0;
            foreach (var dummy in _mCore.Wrap.NodeExprsDummies) sum.Add(dummy);
            _mSystemBalancingConstr = _mCore.Wrap.Model.AddConstr(sum, GRB.LESS_EQUAL, rhs, "Optimal balance");
            _mCore.Wrap.Model.Update();
        }

        #region Delegation

        public double[] NodeOptima
        {
            get { return _mCore.NodeOptima; }
        }

        public double[,] Flows
        {
            get { return _mCore.Flows; }
        }

        public List<double[]> StorageOptima
        {
            get { return _mCore.StorageOptima; }
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
