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
        public BalanceResult BalanceSystem()
        {
            // Find relevant storage layer, charge all below.
            var balanceResult = _mHelper.BalanceGlobally((() => (_mMismatches.Sum() > 0) ? Response.Charge : Response.Discharge));
            // Distribute power.
            _mHelper.DistributePower();

            return balanceResult;
        }

        #region Measurement

        public List<ITimeSeries> CollectTimeSeries()
        {
            var result =  _mHelper.CollectTimeSeries();
            // Bind flow dependence.
            foreach (var ts in result) ts.Properties.Add("Flow", "Cooperative");
            return result;
        }

        public bool Measuring
        {
            get { return _mHelper.Measuring; }
        }

        public void Start()
        {
            ((IMeasureable) _mHelper).Start();
        }

        public void Clear()
        {
            _mHelper.Clear();
        }

        public void Sample(int tick)
        {
            ((IMeasureable) _mHelper).Sample(tick);
        }

        #endregion

    }
}
