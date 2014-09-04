using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using BusinessLogic.ExportStrategies.DistributionStrategies;

namespace BusinessLogic.ExportStrategies
{
    public class CooperativeExportStrategy : IExportStrategy
    {

        private double[] _mMismatches;
        private readonly ExportHelper _mHelper = new ExportHelper();

        public CooperativeExportStrategy(IDistributionStrategy distributionStrategyStrategy)
        {
            _mHelper.DistributionStrategy = distributionStrategyStrategy;
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

        #region Measurement

        public List<ITimeSeries> CollectTimeSeries()
        {
            var result = ((IMeasureableNode) _mHelper).CollectTimeSeries();
            // Bind flow dependence.
            foreach (var ts in result) ts.Properties.Add("Flow", "Cooperative");
            return result;
        }

        public void StartMeasurement()
        {
            ((IMeasureable) _mHelper).StartMeasurement();
        }

        public void Reset()
        {
            ((IMeasureable) _mHelper).Reset();
        }

        public bool Measurering
        {
            get { return _mHelper.Measurering; }
        }

        #endregion

    }
}
