using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Gurobi;
using Utils;

namespace BusinessLogic.ExportStrategies
{
    /// <summary>
    /// Synchronized flow. Use when flow is constrained and/or storage is present.
    /// </summary>
    class ConSyncScheme : IExportScheme
    {

        private readonly IExportScheme _mCore;

        public ConSyncScheme(INode[] nodes, EdgeCollection edges)
        {
            // Corresponds to the projection vector.
            var weights = nodes.Select(node => 1.0 / CountryInfo.GetMeanLoad(node.Name)).ToArray();
            weights.Mult(1.0 / weights.Sum());

            _mCore = new ConScheme(nodes, edges, new ConSyncOptimizer(edges, nodes[0].Storages.Count, weights));
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
