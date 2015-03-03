using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;

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

        public void Bind(IList<INode> nodes, double[] mismatches)
        {
            _mHelper.Bind(nodes, mismatches);
            _mMismatches = mismatches;
        }
        
        /// <summary>
        /// Balance the system utilizing the storages of all nodes.
        /// </summary>
        public void BalanceSystem()
        {
            // Find relevant storage layer, charge all below.
            _mHelper.BalanceGlobally((() => (_mMismatches.Sum() > 0) ? Response.Charge : Response.Discharge));
            // Distribute power.
            _mHelper.DistributePower();
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

        public void Start(int ticks)
        {
            ((IMeasureable)_mHelper).Start(ticks);
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
