using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.ExportStrategies.DistributionStrategies;

namespace BusinessLogic.ExportStrategies
{
    public class SelfishExportStrategy : IExportStrategy
    {

        private double[] _mMismatches;
        private readonly ExportHelper _mHelper = new ExportHelper();

        public SelfishExportStrategy(IDistributionStrategy distributionStrategyStrategy)
        {
            _mHelper.DistributionStrategy = distributionStrategyStrategy;
        }

        public void Bind(List<Node> nodes, double[] mismatches)
        {
            _mHelper.Bind(nodes, mismatches);
            _mMismatches = mismatches;
        }

        /// <summary>
        /// Balance the system; first a node takes what it can use, then it shares.
        /// </summary>
        public BalanceResult BalanceSystem(int tick)
        {
            _mHelper.BalanceLocally(tick, i => _mMismatches[i] > 0, false);

            _mHelper.BalanceGlobally(tick, () => Response.Charge);
            _mHelper.EqualizePower();

            return _mHelper.BalanceLocally(tick, i => _mMismatches[i] < 0, true);
        }

        #region Measurement

        public List<ITimeSeries> CollectTimeSeries()
        {
            return ((IMeasureableNode)_mHelper).CollectTimeSeries();
        }

        public void StartMeasurement()
        {
            ((IMeasureable)_mHelper).StartMeasurement();
        }

        public void Reset()
        {
            ((IMeasureable)_mHelper).Reset();
        }

        public bool Measurering
        {
            get { return _mHelper.Measurering; }
        }

        #endregion

    }
}
