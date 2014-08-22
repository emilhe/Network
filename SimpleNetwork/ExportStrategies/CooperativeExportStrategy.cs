using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    public class CooperativeExportStrategy : IExportStrategy
    {

        private List<Node> _mNodes;
        private double[] _mMismatches;
        private double _mTolerance;
        private Response _mSystemResponse;
        private double[] _mStorageMap;
        private int _mStorageLevel;

        public void Bind(List<Node> nodes, double[] mismatches, double tolerance)
        {
            _mNodes = nodes;
            _mTolerance = tolerance;
            _mMismatches = mismatches;

            _mStorageMap =
                _mNodes.SelectMany(item => item.StorageCollection.Efficiencies())
                    .Distinct()
                    .OrderByDescending(item => item)
                    .ToArray();
        }

        /// <summary>
        /// Detmine the storage level at which the flow optimisation is to take place. Restore/drain all lower levels.
        /// </summary>
        public double TraverseStorageLevels(int tick)
        {
            _mSystemResponse = (_mMismatches.Sum() > 0) ? Response.Charge : Response.Discharge;

            // Restore lower levels if possible.
            for (_mStorageLevel = 0; _mStorageLevel < _mStorageMap.Length; _mStorageLevel++)
            {
                if (SufficientStorageAtCurrentLevel()) return _mStorageMap[_mStorageLevel];

                // Restore the lower storage level.
                for (int index = 0; index < _mNodes.Count; index++)
                {
                    _mMismatches[index] += _mNodes[index].StorageCollection.Get(_mStorageMap[_mStorageLevel])
                        .Restore(tick, _mSystemResponse);
                }
            }

            return -1;
        }

        /// <summary>
        /// Determine if sufficient storage is availble at the current level.
        /// </summary>
        /// <returns> true if there is </returns>
        private bool SufficientStorageAtCurrentLevel()
        {
            var storage =
                _mNodes.Select(item => item.StorageCollection)
                    .Select(item => item.Get(_mStorageMap[_mStorageLevel]).RemainingCapacity(_mSystemResponse))
                    .Sum();

            switch (_mSystemResponse)
            {
                case Response.Charge:
                    return storage >= (_mMismatches.Sum() + _mMismatches.Length * _mTolerance);
                case Response.Discharge:
                    // Flip sign signs; the numbers are negative.
                    return storage <= (_mMismatches.Sum() - _mMismatches.Length * _mTolerance);
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }
    }
}
