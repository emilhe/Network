using System.Collections.Generic;
using BusinessLogic.Interfaces;
using BusinessLogic.Utils;

namespace BusinessLogic.ExportStrategies
{
    public class NoExportScheme : IMeasureable, IExportScheme
    {

        private readonly INode[] _mNodes;
        private readonly StorageMap _mMap;
        private double[] _mMismatches;

        public NoExportScheme(INode[] nodes)
        {
            _mNodes = nodes;
        }

        public void Bind(double[] mismatches)
        {
            _mMismatches = mismatches;
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full.
        /// </summary>
        public void BalanceSystem()
        {
            for (int i = 0; i < _mNodes.Length; i++)
            {
                _mMismatches[i] = Inject(_mMismatches[i],_mNodes[i]);
                _mNodes[i].Balancing.CurrentValue = _mMismatches[i];
                _mMismatches[i] = 0;
            }
        }

        private double Inject(double amount, INode node)
        {
            // NOTE: Assumes storages are ordered by efficiency.
            var idx = 0;
            while (amount > 0 && idx < node.Storages.Count)
            {
                amount = node.Storages[idx].Inject(amount);
                idx++;
            }
            return amount;
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
