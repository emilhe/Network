using System.Collections.Generic;
using BusinessLogic.Interfaces;

namespace BusinessLogic.ExportStrategies
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
