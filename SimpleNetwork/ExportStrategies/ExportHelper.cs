using System;
using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    class ExportHelper
    {
        // Tolerance due to numeric roundings OR the distribution strategy chosen.
        public double Tolerance
        {
            get { return DistributionStrategy != null ? DistributionStrategy.Tolerance : 1e-10; }
        }

        private List<Node> _mNodes;
        private Response _mSystemResponse;
        private double[] _mMismatches;
        private double[] _mStorageMap;
        private int _mStorageLevel;

        public IDistributionStrategy DistributionStrategy { get; set; }

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

        #region Global balancing

        /// <summary>
        /// Detmine the storage level at which the flow optimisation is to take place. Restore/drain all lower levels.
        /// </summary>
        public BalanceResult BalanceGlobally(int tick, Func<Response> respFunc)
        {
            var result = new BalanceResult();
            _mSystemResponse = respFunc();

            // Restore lower levels if possible.
            for (_mStorageLevel = 0; _mStorageLevel < _mStorageMap.Length; _mStorageLevel++)
            {
                if (SufficientStorageAtCurrentLevel()) break;

                // Restore the lower storage level.
                for (int index = 0; index < _mNodes.Count; index++)
                {
                    if (!_mNodes[index].StorageCollection.Contains(_mStorageMap[_mStorageLevel])) continue;
                    _mMismatches[index] += _mNodes[index].StorageCollection.Get(_mStorageMap[_mStorageLevel])
                        .Restore(tick, _mSystemResponse);
                }
            }

            // Calculate curtailment.
            result.Curtailment = 0.0;
            if (_mStorageMap[_mStorageLevel] == -1) result.Curtailment = _mMismatches.Sum();
            result.Failure = (result.Curtailment < 0);

            return result;
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
                    return storage >= (_mMismatches.Sum() + _mMismatches.Length * DistributionStrategy.Tolerance);
                case Response.Discharge:
                    // Flip sign signs; the numbers are negative.
                    return storage <= (_mMismatches.Sum() - _mMismatches.Length * DistributionStrategy.Tolerance);
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }

        #endregion

        #region Local balancing

        /// <summary>
        /// Charge all nodes individually until all energy is used or the storage is full, skip nodes not fulfilling condition.
        /// </summary>
        public BalanceResult BalanceLocally(int tick, Func<int, bool> condition, bool allowCurtailment)
        {
            var result = new BalanceResult {Curtailment = 0, Failure = false};

            for (int i = 0; i < _mNodes.Count; i++)
            {
                foreach (double efficiency in _mStorageMap)
                {
                    if (!condition(i)) continue;
                    if (!allowCurtailment && efficiency == -1) continue;
                    if (!_mNodes[i].StorageCollection.Contains(efficiency)) continue;
                    // Log curtailment and record failures on negative curtailment.
                    if (efficiency == -1)
                    {
                        if (_mMismatches[i] < -Tolerance) result.Failure = true;
                        result.Curtailment += _mMismatches[i];
                    }
                    _mMismatches[i] = _mNodes[i].StorageCollection.Get(efficiency).Inject(tick, _mMismatches[i]);
                }
            }

            return result;
        }

        #endregion

        #region Power distribution

        /// <summary>
        /// Distribute power. This includes chargeing/discharge storage if necessary.
        /// </summary>
        public void DistributePower(int tick)
        {
            DistributionStrategy.DistributePower(_mNodes, _mMismatches, _mStorageMap[_mStorageLevel], tick);
        }

        /// <summary>
        /// Equalize power. This does NOT include charging/discharging storage.
        /// </summary>
        public void EqualizePower()
        {
            DistributionStrategy.EqualizePower(_mMismatches);            
        }

        #endregion

    }

    public class BalanceResult
    {
        public double Curtailment { get; set; }
        public bool Failure { get; set; }
    }
}
