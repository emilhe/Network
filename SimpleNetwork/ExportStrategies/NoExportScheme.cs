using System.Collections.Generic;
using BusinessLogic.Interfaces;

namespace BusinessLogic.ExportStrategies
{
    public class NoExportScheme : IExportScheme
    {

        private IList<INode> _mNodes;
        private double[] _mMismatches;

        public void Bind(IList<INode> nodes, double[] mismatches)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full.
        /// </summary>
        public void BalanceSystem()
        {
            for (int i = 0; i < _mNodes.Count; i++)
            {
                _mMismatches[i] = _mNodes[i].StorageCollection.Inject(_mMismatches[i]);
                _mNodes[i].Balancing.CurrentValue = _mMismatches[i];
                _mMismatches[i] = 0;
            }
        }

        #region Measurement

        public bool Measuring { get { return false; } }

        public void Start(int ticks)
        {
            // Nothing to measure.
        }

        public void Clear()
        {
            // Nothing to measure.
        }

        public void Sample(int tick)
        {
            // Nothing to measure.
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries>();
        }

        #endregion

    }
}
