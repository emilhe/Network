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
        private int _mMaximumStorageLevel = -1;
        private int _mStorageLevel;

        public void Bind(List<Node> nodes, double[] mismatches, double tolerance)
        {
            _mNodes = nodes;
            _mTolerance = tolerance;
            _mMismatches = mismatches;

            if (!_mNodes.SelectMany(item => item.Storages.Keys).Any()) return;
            _mMaximumStorageLevel = _mNodes.SelectMany(item => item.Storages.Keys).Max();
        }

        /// <summary>
        /// Detmine the storage level at which the flow optimisation is to take place. Restore/drain all lower levels.
        /// </summary>
        public int TraverseStorageLevels(int tick)
        {
            _mStorageLevel = 0;
            _mSystemResponse = (_mMismatches.Sum() > 0) ? Response.Charge : Response.Discharge;

            // Restore lower levels if possible.
            while (_mStorageLevel <= _mMaximumStorageLevel && InsufficientStorageAtCurrentLevel())
            {
                // Restore the lower storage level.
                for (int index = 0; index < _mNodes.Count; index++)
                {
                    _mMismatches[index] += _mNodes[index].Storages[_mStorageLevel].Restore(tick, _mSystemResponse);
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
            var storage = _mNodes.Select(item => item.Storages[_mStorageLevel].RemainingCapacity(_mSystemResponse)).Sum();
            switch (_mSystemResponse)
            {
                case Response.Charge:
                    return storage < (_mMismatches.Sum() + _mMismatches.Length * _mTolerance);
                case Response.Discharge:
                    // Flip sign signs; the numbers are negative.
                    return storage > (_mMismatches.Sum() - _mMismatches.Length * _mTolerance);
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }
    }
}
