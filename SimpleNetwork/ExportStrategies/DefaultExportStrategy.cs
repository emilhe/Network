using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork
{
    public class DefaultExportStrategy : IExportStrategy
    {
        private const double Tolerance = 1e-4;

        public List<Node> Nodes { get; private set; }

        private readonly FlowOptimizer _flowOptimizer;
        private readonly int _mMaximumStorageLevel = -1;

        #region Current iteration fields

        public double Mismatch { get; private set; }
        public double Curtailment { get; private set; }
        public bool Failure { get; private set; }

        private readonly double[] _mMismatches;
        private readonly double[] _mLoLims;
        private readonly double[] _mHiLims;
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

        public DefaultExportStrategy(List<Node> nodes, EdgeSet edges)
        {
            // Auto detect the maximum storage level.
            Nodes = nodes;
            _mMaximumStorageLevel = Nodes.SelectMany(item => item.Storages.Keys).Max();

            _flowOptimizer = new FlowOptimizer(Nodes.Count);
            _flowOptimizer.SetEdges(edges);

            _mLoLims = new double[Nodes.Count];
            _mHiLims = new double[Nodes.Count];
            _mMismatches = new double[Nodes.Count];
        }

        public void Respond(int tick)
        {
            _mTick = tick;

            CalculateMismatches();
            TraverseStorageLevels();
            OptimizeEnergyFlows();
            CurtailExcessEnergy();
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
            // Setup limits.
            var idx = 0;
            foreach (var storage in Nodes.Select(item => item.Storages[_mStorageLevel]))
            {
                _mLoLims[idx] = storage.RemainingCapacity(Response.Discharge);
                _mHiLims[idx] = storage.RemainingCapacity(Response.Charge);
                idx++;
            }
        }

        /// <summary>
        /// Optimize the energy flows and perform the optimal charges/discharges.
        /// </summary>
        private void OptimizeEnergyFlows()
        {
            if (OutOfStorage) return;

            // Determine FLOWS using Gurobi optimization.
            _flowOptimizer.SetNodes(_mMismatches, _mLoLims, _mHiLims);
            _flowOptimizer.Solve();

            // Charge based on flow optimization results.
            for (int index = 0; index < Nodes.Count; index++)
            {
                _mMismatches[index] = Nodes[index].Storages[_mStorageLevel].Inject(_mTick, _flowOptimizer.NodeOptimum[index]);
            }
        }

        /// <summary>
        /// Curtail all exess energy and report any negative curtailment (success = false).
        /// </summary>
        private void CurtailExcessEnergy()
        {
            Curtailment = _mMismatches.Sum();
            Failure = Curtailment < -_mMismatches.Length*Tolerance;
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
                    return storage < (_mMismatches.Sum() + _mMismatches.Length * Tolerance);
                case Response.Discharge:
                    // Flip sign signs; the numbers are negative.
                    return storage > (_mMismatches.Sum() - _mMismatches.Length * Tolerance);
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }

    }
}
