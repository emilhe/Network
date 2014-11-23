using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;

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

        public void Bind(List<INode> nodes, double[] mismatches)
        {
            _mHelper.Bind(nodes, mismatches);
            _mMismatches = mismatches;
        }

        /// <summary>
        /// Balance the system; first a node takes what it can use, then it shares.
        /// </summary>
        public BalanceResult BalanceSystem()
        {
            _mHelper.BalanceLocally(i => _mMismatches[i] > 0, false);

            _mHelper.BalanceGlobally(() => Response.Charge);
            _mHelper.EqualizePower();

            return _mHelper.BalanceLocally(i => _mMismatches[i] < 0, true);
        }

        #region Measurement

        public List<ITimeSeries> CollectTimeSeries()
        {
            return _mHelper.CollectTimeSeries();
        }

        public bool Measuring
        {
            get { return _mHelper.Measuring; }
        }

        public void Start()
        {
            _mHelper.Start();
        }

        public void Clear()
        {
            _mHelper.Clear();
        }

        public void Sample(int tick)
        {
            _mHelper.Sample(tick);
        }

        #endregion

    }
}
