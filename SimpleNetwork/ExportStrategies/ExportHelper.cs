using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;

namespace BusinessLogic.ExportStrategies
{
    class ExportHelper : IMeasureable
    {
        // Tolerance due to numeric roundings OR the distribution strategy chosen.
        public double Tolerance
        {
            get { return DistributionStrategy != null ? DistributionStrategy.Tolerance : 1e-10; }
        }

        /// <summary>
        /// Distribution strategy used for evaluation.
        /// </summary>
        public IDistributionStrategy DistributionStrategy { get; set; }

        private IList<INode> _mNodes;
        private Response _mSystemResponse;
        private double[] _mMismatches;

        private Dictionary<int, IStorage[]> _mStorageMap;
        private double[] _mStorageMappings;
        private int _mStorageLevel;


        public void Bind(IList<INode> nodes, double[] mismatches)
        {
            _mNodes = nodes;
            _mMismatches = mismatches;

            _mStorageMap = new Dictionary<int, IStorage[]>();
            _mStorageMappings =
                _mNodes.SelectMany(item => item.StorageCollection.Select(subItem => subItem.Key))
                    .Distinct()
                    .OrderByDescending(item => item)
                    .ToArray();
            for (int i = 0; i < _mStorageMappings.Length; i++)
            {
                _mStorageMap.Add(i, _mNodes.Select(item => item.StorageCollection)
                    .Where(item => item.Contains(_mStorageMappings[i]))
                    .Select(item => item.Get(_mStorageMappings[i])).ToArray());
            }
        }

        #region Global balancing

        /// <summary>
        /// Detmine the storage level at which the flow optimisation is to take place. Restore/drain all lower levels.
        /// </summary>
        public void BalanceGlobally(Func<Response> respFunc)
        {
            _mSystemResponse = respFunc();

            // Restore lower levels if possible.
            for (_mStorageLevel = 0; _mStorageLevel < _mStorageMappings.Length; _mStorageLevel++)
            {
                if (SufficientStorageAtCurrentLevel()) break;

                // Restore the lower storage level.
                for (int index = 0; index < _mNodes.Count; index++)
                {
                    if (!_mNodes[index].StorageCollection.Contains(_mStorageMappings[_mStorageLevel])) continue;
                    _mMismatches[index] += _mNodes[index].StorageCollection.Get(_mStorageMappings[_mStorageLevel])
                        .InjectMax(_mSystemResponse);
                }
            }
        }

        /// <summary>
        /// Determine if sufficient storage is availble at the current level.
        /// </summary>
        /// <returns> true if there is </returns>
        private bool SufficientStorageAtCurrentLevel()
        {
            var storage = _mStorageMap[_mStorageLevel].Select(item => item.AvailableEnergy(_mSystemResponse)).Sum();
            //var storage =
            //    _mNodes.Select(item => item.StorageCollection)
            //        .Where(item => item.Contains(_mStorageMappings[_mStorageLevel]))
            //        .Select(item => item.Get(_mStorageMappings[_mStorageLevel]).AvailableEnergy(_mSystemResponse))
            //        .Sum();

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
        public void BalanceLocally(Func<int, bool> condition, bool allowCurtailment)
        {
            for (int i = 0; i < _mNodes.Count; i++)
            {
                foreach (double efficiency in _mStorageMappings)
                {
                    if (!condition(i)) continue;
                    if (!allowCurtailment && efficiency == -1) continue;
                    if (!_mNodes[i].StorageCollection.Contains(efficiency)) continue;
                    _mMismatches[i] = _mNodes[i].StorageCollection.Get(efficiency).Inject(_mMismatches[i]);
                }
            }
        }

        #endregion

        #region Power distribution

        /// <summary>
        /// Distribute power. This includes chargeing/discharge storage if necessary.
        /// </summary>
        public void DistributePower()
        {
            DistributionStrategy.DistributePower(_mNodes, _mMismatches, _mStorageMappings[_mStorageLevel]);
        }

        /// <summary>
        /// Equalize power. This does NOT include charging/discharging storage.
        /// </summary>
        public void EqualizePower()
        {
            DistributionStrategy.EqualizePower(_mMismatches);            
        }

        #endregion

        #region Measurement

        public bool Measuring
        {
            get
            {
                if (DistributionStrategy == null) return false;
                return DistributionStrategy.Measuring;
            }
        }

        public void Start(int ticks)
        {
            if (DistributionStrategy == null) return;
            DistributionStrategy.Start(ticks);
        }

        public void Clear()
        {
            if (DistributionStrategy == null) return;
            DistributionStrategy.Clear();
        }

        public void Sample(int tick)
        {
            if (DistributionStrategy == null) return;
            DistributionStrategy.Sample(tick);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            if (DistributionStrategy == null) return new List<ITimeSeries>();
            return DistributionStrategy.CollectTimeSeries();
        }

        #endregion

    }

    public class BalanceResult
    {
        public double Curtailment { get; set; }
        public bool Failure { get; set; }
    }
}
