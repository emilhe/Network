using System;
using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    public class NoExportStrategy : IExportStrategy
    {
        private List<Node> _mNodes;
        private double[] _mMismatches;
        private double[] _mStorageMap;

        public double Tolerance { get { return 0; } }

        public void Bind(List<Node> nodes, double[] mismatches)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;

            _mStorageMap =
                _mNodes.SelectMany(item => item.StorageCollection.Efficiencies())
                    .Distinct()
                    .OrderByDescending(item => item)
                    .ToArray();
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full.
        /// </summary>
        public void BalanceSystem(int tick)
        {
            BalanceSystem(tick, i => true);
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full, skip nodes not fulfilling condition.
        /// </summary>
        public void BalanceSystem(int tick, Func<int, bool> condition)
        {
            for (int i = 0; i < _mNodes.Count; i++)
            {
                foreach (double efficiency in _mStorageMap)
                {
                    if (!condition(i)) continue;
                    if (!_mNodes[i].StorageCollection.Contains(efficiency)) continue;
                    _mMismatches[i] = _mNodes[i].StorageCollection.Get(efficiency).Inject(tick, _mMismatches[i]);
                }
            }
        }

    }
}
