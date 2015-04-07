using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.Utils;
using Gurobi;

namespace BusinessLogic.ExportStrategies
{
    /// <summary>
    /// Localized flow. Use when flow is constrained and/or storage is present.
    /// </summary>
    public class ConLocalScheme: IExportScheme
    {

        private readonly IExportScheme _mCore;
        private const double BalanceWeight = 1e6;

        public ConLocalScheme(INode[] nodes, EdgeCollection edges)
        {
            var core = new CoreOptimizer(edges, nodes[0].Storages.Count, item =>
            {
                var obj = new GRBLinExpr();
                obj.MultAdd(BalanceWeight, ObjectiveFactory.LinearBalancing(item));
                return obj;
            });
            var optimizer = new QuadFlowOptimizer(core, core.ApplySystemConstr, core.RemoveSystemConstr);
            _mCore = new ConScheme(nodes, edges, optimizer);
        }

        #region Delegation

        public bool Measuring
        {
            get { return _mCore.Measuring; }
        }

        public void Start(int ticks)
        {
            _mCore.Start(ticks);
        }

        public void Clear()
        {
            _mCore.Clear();
        }

        public void Sample(int tick)
        {
            _mCore.Sample(tick);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return _mCore.CollectTimeSeries();
        }

        public void Bind(double[] mismatches)
        {
            _mCore.Bind(mismatches);
        }

        public void BalanceSystem()
        {
            _mCore.BalanceSystem();
        }

        #endregion

    }
}
