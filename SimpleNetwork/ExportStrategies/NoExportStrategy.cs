using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork
{
    public class NoExportStrategy : IExportStrategy
    {
        private List<Node> _mNodes;
        private double[] _mMismatches;
        private int _mMaximumStorageLevel = -1;
        private int _mStorageLevel;

        public void Bind(List<Node> nodes, double[] mismatches, double tolerance = 0)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;

            if (!_mNodes.SelectMany(item => item.Storages).Any()) return;
            _mMaximumStorageLevel = _mNodes.SelectMany(item => item.Storages.Keys).Max();
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full.
        /// </summary>
        public int TraverseStorageLevels(int tick)
        {
            _mStorageLevel = 0;

            while (_mStorageLevel <= _mMaximumStorageLevel && InsufficientStorageAtCurrentLevel())
            {
                // Charge the lower storage level.
                for (int index = 0; index < _mNodes.Count; index++)
                {
                    _mMismatches[index] = _mNodes[index].Storages[_mStorageLevel].Inject(tick, _mMismatches[index]);
                }
                // Go to the next storage level.
                _mStorageLevel++;
            }

            return _mStorageLevel;
        }

        /// <summary>
        /// Determine if sufficient storage is availble at the current level.
        /// </summary>
        /// <returns> false if there is </returns>
        private bool InsufficientStorageAtCurrentLevel()
        {
            return _mMismatches.Where(item => item != 0).Any();
        }
    }
}
