using System;
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
                _mNodes.SelectMany(item => item.StorageCollection.Efficiencies())
                    .Distinct()
                    .OrderByDescending(item => item)
                    .ToArray();
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full.
        /// </summary>
        public double TraverseStorageLevels(int tick)
        {
            return TraverseStorageLevels(tick, i => true);
        }

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full. A condition on when to skip nodes can be supplied.
        /// </summary>
        public double TraverseStorageLevels(int tick, Func<int, bool> condition)
        {
            _mStorageLevel = 0;

            // Charge lower levels if possible.
            for (_mStorageLevel = 0; _mStorageLevel < _mStorageMap.Length; _mStorageLevel++)
            {
                for (int index = 0; index < _mNodes.Count; index++)
                {
                    if (!_mNodes[index].StorageCollection.Contains(_mStorageMap[_mStorageLevel])) continue;
                    if(!condition(index)) continue;
                    _mMismatches[index] = _mNodes[index].StorageCollection.Get(_mStorageMap[_mStorageLevel]).Inject(tick, _mMismatches[index]);
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
