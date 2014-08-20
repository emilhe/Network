using System;
using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.ExportStrategies.FlowStrategy;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    public class CooperativeExportStrategy : IExportStrategy
    {
        public List<Node> Nodes { get; private set; }

        private readonly int _mMaximumStorageLevel = -1;
        private readonly IFlowStrategy _mFlowStrategy;

        #region Current iteration fields

        public double Mismatch { get; private set; }
        public double Curtailment { get; private set; }
        public bool Failure { get; private set; }

        private readonly double[] _mMismatches;
        private int _mStorageLevel;
        private int _mTick;

        #endregion

        private bool OutOfStorage
        {
            get { return _mStorageLevel > _mMaximumStorageLevel; }
        }

        private Response SystemResponse
        {
            get { return (Mismatch > 0) ? Response.Charge : Response.Discharge; }
        }

        public CooperativeExportStrategy(List<Node> nodes, IFlowStrategy flowStrategy)
        {
            Nodes = nodes;
            _mFlowStrategy = flowStrategy;
            _mMismatches = new double[Nodes.Count];
            // Auto detect the maximum storage level.
            _mMaximumStorageLevel = Nodes.SelectMany(item => item.Storages.Keys).Max();
        }

        public void Respond(int tick)
        {
            _mTick = tick;

            CalculateMismatches();
            TraverseStorageLevels();
            DistributeRemainingPower();
            CurtailExcessEnergy();
        }

        private void DistributeRemainingPower()
        {
            if (OutOfStorage) return;

            _mFlowStrategy.DistributePower(Nodes, _mMismatches, _mStorageLevel, _mTick);

            //// TODO: Does this take efficiency properly into account; ANSWER: NO!?
            //var remainingCapacity = Nodes.Select(item => item.Storages[_mStorageLevel].RemainingCapacity(SystemResponse)).Sum();
            //var totalCapacity = Nodes.Select(item => item.Storages[_mStorageLevel].Capacity).Sum();
            //var meanChargeLevel = (remainingCapacity - _mMismatches.Sum()) / totalCapacity;
            //// Charge/discharge to equalize.
            //for (int index = 0; index < Nodes.Count; index++)
            //{
            //    var storage = Nodes[index].Storages[_mStorageLevel];
            //    var toInject = storage.RemainingCapacity(SystemResponse) - meanChargeLevel * storage.Capacity;
            //    Nodes[index].Storages[_mStorageLevel].Inject(_mTick, toInject);
            //    // This should be true after all "transfers complete".
            //    _mMismatches[index] = 0;
            //}
        }

        /// <summary>
        /// Determine system response; charge or discharge.
        /// </summary>
        private void CalculateMismatches()
        {
            for (int i = 0; i < Nodes.Count; i++) _mMismatches[i] = Nodes[i].GetDelta(_mTick);
            Mismatch = _mMismatches.Sum();
        }

        /// <summary>
        /// Detmine the storage level at which the flow optimisation is to take place. Restore/drain all lower levels.
        /// </summary>
        private void TraverseStorageLevels()
        {
            _mStorageLevel = 0;
            // Restore lower levels if possible.
            while (InsufficientStorageAtCurrentLevel())
            {
                // Restore the lower storage level.
                for (int index = 0; index < Nodes.Count; index++)
                {
                    _mMismatches[index] += Nodes[index].Storages[_mStorageLevel].Restore(_mTick, SystemResponse);
                }
                // Go to the next storage level.
                _mStorageLevel++;
                if (OutOfStorage) return;
            }
        }

        /// <summary>
        /// Curtail all exess energy and report any negative curtailment (success = false).
        /// </summary>
        private void CurtailExcessEnergy()
        {
            Curtailment = _mMismatches.Sum();
            Failure = Curtailment < -_mMismatches.Length * _mFlowStrategy.Tolerance;
        }

        /// <summary>
        /// Determine if sufficient storage is availble at the current level.
        /// </summary>
        /// <returns> false if there is </returns>
        private bool InsufficientStorageAtCurrentLevel()
        {
            var storage = Nodes.Select(item => item.Storages[_mStorageLevel].RemainingCapacity(SystemResponse)).Sum();
            switch (SystemResponse)
            {
                case Response.Charge:
                    return storage < (_mMismatches.Sum() + _mMismatches.Length * _mFlowStrategy.Tolerance);
                case Response.Discharge:
                    // Flip sign signs; the numbers are negative.
                    return storage > (_mMismatches.Sum() - _mMismatches.Length * _mFlowStrategy.Tolerance);
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }

    }
}
