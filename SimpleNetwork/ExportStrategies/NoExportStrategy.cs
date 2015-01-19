using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;

namespace BusinessLogic.ExportStrategies
{
    public class NoExportStrategy : IExportStrategy
    {
        private readonly ExportHelper _mHelper = new ExportHelper();

        public void Bind(IList<INode> nodes, double[] mismatches)
        {
            _mHelper.Bind(nodes, mismatches);
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full.
        /// </summary>
        public BalanceResult BalanceSystem()
        {
            return _mHelper.BalanceLocally(i => true, true);
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

        public void Start(int ticks)
        {
            ((IMeasureable)_mHelper).Start(ticks);
        }

        public void Clear()
        {
            ((IMeasureable)_mHelper).Clear();
        }

        public void Sample(int tick)
        {
            ((IMeasureable) _mHelper).Sample(tick);
        }

        #endregion

    }
}
