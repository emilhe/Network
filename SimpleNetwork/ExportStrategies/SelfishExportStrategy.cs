using System;
using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.ExportStrategies.DistributionStrategies;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    public class SelfishExportStrategy : IExportStrategy
    {

        private double[] _mMismatches;
        private readonly ExportHelper _mHelper = new ExportHelper();

        public SelfishExportStrategy(IDistributionStrategy distributionStrategy)
        {
            _mHelper.DistributionStrategy = distributionStrategy;
        }

        public void Bind(List<Node> nodes, double[] mismatches)
        {
            _mHelper.Bind(nodes, mismatches);
            _mMismatches = mismatches;
        }

        /// <summary>
        /// TODO: Comment shit
        /// </summary>
        public BalanceResult BalanceSystem(int tick)
        {
            _mHelper.BalanceLocally(tick, i => _mMismatches[i] > 0, false);

            _mHelper.BalanceGlobally(tick, () => Response.Charge);
            _mHelper.EqualizePower();

            return _mHelper.BalanceLocally(tick, i => _mMismatches[i] < 0, true);
        }

    }
}
