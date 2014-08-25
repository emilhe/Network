using System.Collections.Generic;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    public class NoExportStrategy : IExportStrategy
    {
        private readonly ExportHelper _mHelper = new ExportHelper();

        public void Bind(List<Node> nodes, double[] mismatches)
        {
            _mHelper.Bind(nodes, mismatches);
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full.
        /// </summary>
        public BalanceResult BalanceSystem(int tick)
        {
            return _mHelper.BalanceLocally(tick, i => true, true);
        }

    }
}
