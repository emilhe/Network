using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using Gurobi;
using Utils;

namespace BusinessLogic.Utils
{
    class ConSyncOptimizer : IOptimizer
    {

        private readonly GRBConstr[] _mNodalBalancingConstrs;
        private readonly List<GRBConstr[]> _mNodalStorageConstrs;
        private readonly QuadFlowOptimizer _mCore;

        public ConSyncOptimizer(EdgeCollection edges, int m, double[] weights)
        {
            // Setup constraint arrays.
            _mNodalBalancingConstrs = new GRBConstr[edges.NodeCount];
            _mNodalStorageConstrs = new List<GRBConstr[]>(m);
            for (int j = 0; j < m; j++)
            {
                _mNodalStorageConstrs.Add(new GRBConstr[edges.NodeCount]);
            }
            // Setup optimizer.
            var core = new CoreOptimizer(edges, m, item => ObjectiveFactory.QuadraticBalancing(item, weights));
            _mCore = new QuadFlowOptimizer(core)
            {
                ApplyConstr = ApplyNodalConstrs,
                RemoveConstr = RemoveNodalConstrs
            };
        }

        public void ApplyNodalConstrs()
        {
            for (int i = 0; i < _mCore.Wrap.NodeExprs.Length; i++)
            {
                _mNodalBalancingConstrs[i] = _mCore.Wrap.Model.AddConstr(_mCore.Wrap.NodeExprs[i], GRB.EQUAL,
                    _mCore.Wrap.NodeExprs[i].Value, "Optimal nodal balance " + i);
            }

            for (int j = 0; j < _mCore.Wrap.Storages.Count; j++)
            {
                for (int i = 0; i < _mCore.Wrap.Storages[j].Length; i++)
                {
                    _mNodalStorageConstrs[j][i] = _mCore.Wrap.Model.AddConstr(_mCore.Wrap.Storages[j][i], GRB.EQUAL,
                        _mCore.Wrap.Storages[j][i].Get(GRB.DoubleAttr.X), "Optimal storage balance " + i + j);
                }
            }
        }

        public void RemoveNodalConstrs()
        {
            foreach (var constr in _mNodalBalancingConstrs) _mCore.Wrap.Model.Remove(constr);
            foreach (var level in _mNodalStorageConstrs)
            {
                foreach (var constr in level)
                {
                    _mCore.Wrap.Model.Remove(constr);
                }
            }
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
            ((IOptimizer)_mCore).SetNodes(nodes, lowLimits, highLimits);
        }

        public void Solve()
        {
            ((IOptimizer)_mCore).Solve();
        }

        #endregion


    }
}
