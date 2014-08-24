using System;
using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.ExportStrategies.DistributionStrategies;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    public class SelfishExportStrategy : IExportStrategy
    {

        public double Tolerance { get { return _mDistributionStrategy.Tolerance; } }

        private readonly NoExportStrategy _mNoExportExportStrategy = new NoExportStrategy();

        private readonly IDistributionStrategy _mDistributionStrategy;
        private List<Node> _mNodes;
        private Response _mSystemResponse;
        private double[] _mMismatches;
        private double[] _mStorageMap;
        private int _mStorageLevel;

        public SelfishExportStrategy(IDistributionStrategy distributionStrategy)
        {
            _mDistributionStrategy = distributionStrategy;
            _mDistributionStrategy.ShareStorage = false;
        }

        public void Bind(List<Node> nodes, double[] mismatches)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;
            ((IExportStrategy) _mNoExportExportStrategy).Bind(nodes, mismatches);

            _mStorageMap =
                _mNodes.SelectMany(item => item.StorageCollection.Efficiencies())
                    .Distinct()
                    .OrderByDescending(item => item)
                    .ToArray();
        }

        public void BalanceSystem(int tick)
        {
            _mNoExportExportStrategy.BalanceSystem(tick, i => _mMismatches[i] > 0);

            TraverseStorageLevels(tick);

            // TODO: NOT correct; flow results are wrong, fail/succes is correct; if fix is not used, gurobi fucks due to power overflow.
            if (_mStorageLevel == _mStorageMap.Length) return;

            _mDistributionStrategy.DistributePower(_mNodes, _mMismatches, _mStorageMap[_mStorageLevel], tick);
            _mNoExportExportStrategy.BalanceSystem(tick, i => _mMismatches[i] < 0);
        }

        /// <summary>
        /// Detmine the storage level at which the flow optimisation is to take place. Restore/drain all lower levels.
        /// </summary>
        private void TraverseStorageLevels(int tick)
        {
            _mSystemResponse = Response.Charge;

            // Restore lower levels if possible.
            for (_mStorageLevel = 0; _mStorageLevel < _mStorageMap.Length; _mStorageLevel++)
            {
                if (SufficientStorageAtCurrentLevel()) return;

                // Restore the lower storage level.
                for (int index = 0; index < _mNodes.Count; index++)
                {
                    if (!_mNodes[index].StorageCollection.Contains(_mStorageMap[_mStorageLevel])) continue;
                    _mMismatches[index] += _mNodes[index].StorageCollection.Get(_mStorageMap[_mStorageLevel])
                        .Restore(tick, _mSystemResponse);
                }
            }
        }

        /// <summary>
        /// Determine if sufficient storage is availble at the current level.
        /// </summary>
        /// <returns> true if there is </returns>
        private bool SufficientStorageAtCurrentLevel()
        {
            var storage =
                _mNodes.Select(item => item.StorageCollection)
                    .Where(item => item.Contains(_mStorageMap[_mStorageLevel]))
                    .Select(item => item.Get(_mStorageMap[_mStorageLevel]).RemainingCapacity(_mSystemResponse))
                    .Sum();

            switch (_mSystemResponse)
            {
                case Response.Charge:
                    return storage >= (_mMismatches.Sum() + _mMismatches.Length * _mDistributionStrategy.Tolerance);
                case Response.Discharge:
                    // Flip sign signs; the numbers are negative.
                    return storage <= (_mMismatches.Sum() - _mMismatches.Length * _mDistributionStrategy.Tolerance);
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }

    }
}
