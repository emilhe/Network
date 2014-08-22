using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork
{
    public class NoExportStrategy : IExportStrategy
    {
        private List<Node> _mNodes;
        private double[] _mMismatches;
        private double[] _mStorageMap;
        private int _mStorageLevel;

        public void Bind(List<Node> nodes, double[] mismatches, double tolerance = 0)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;

            _mStorageMap =
                _mNodes.SelectMany(item => item.Storages)
                    .Select(item => item.Efficiency)
                    .Distinct()
                    .OrderByDescending(item => item)
                    .ToArray();
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full.
        /// </summary>
        public double TraverseStorageLevels(int tick)
        {
            _mStorageLevel = 0;

            // Restore lower levels if possible.
            for (_mStorageLevel = 0; _mStorageLevel < _mStorageMap.Length; _mStorageLevel++)
            {
                // Restore the lower storage level.
                for (int index = 0; index < _mNodes.Count; index++)
                {
                    _mMismatches[index] = _mNodes[index].Storages.Single(item => item.Efficiency.Equals(_mStorageMap[_mStorageLevel])).Inject(tick, _mMismatches[index]);
                }

                if (SufficientStorageAtCurrentLevel()) return _mStorageMap[_mStorageLevel];
            }

            return -1;
        }

        /// <summary>
        /// Determine if sufficient storage is availble at the current level.
        /// </summary>
        /// <returns> false if there is </returns>
        private bool SufficientStorageAtCurrentLevel()
        {
            return _mMismatches.All(item => item == 0);
        }
    }
}
