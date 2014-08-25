using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    public class CooperativeExportStrategy : IExportStrategy
    {

        private double[] _mMismatches;
        private readonly ExportHelper _mHelper = new ExportHelper();

        public CooperativeExportStrategy(IDistributionStrategy distributionStrategy)
        {
            _mHelper.DistributionStrategy = distributionStrategy;
        }

        public void Bind(List<Node> nodes, double[] mismatches)
        {
            _mHelper.Bind(nodes, mismatches);
            _mMismatches = mismatches;
        }
        
        /// <summary>
        /// Balance the system utilizing the storages of all nodes.
        /// </summary>
        public BalanceResult BalanceSystem(int tick)
        {
            // Find relevant storage layer, charge all below.
            var balanceResult = _mHelper.BalanceGlobally(tick, (() => (_mMismatches.Sum() > 0) ? Response.Charge : Response.Discharge));
            // Distribute power.
            _mHelper.DistributePower(tick);

            return balanceResult;
        }

    }
}
