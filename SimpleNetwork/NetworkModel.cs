﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNetwork.Interfaces
{
    public class NetworkModel
    {
        public List<Node> Nodes { get; private set; }

        private readonly int _mMaximumStorageLevel = -1;
        private readonly IDistributionStrategy _mDistributionStrategy;
        private readonly IExportStrategy _mExportStrategy;

        #region Current iteration fields

        public double Mismatch { get; private set; }
        public double Curtailment { get; private set; }
        public bool Failure { get; private set; }

        private readonly double[] _mMismatches;
        private double _mEfficiency;
        private int _mTick;

        #endregion

        public NetworkModel(List<Node> nodes, IExportStrategy exportStrategy, IDistributionStrategy distributionStrategy)
        {
            Nodes = nodes;
            _mDistributionStrategy = distributionStrategy;
            _mExportStrategy = exportStrategy;

            _mMismatches = new double[Nodes.Count];
            _mExportStrategy.Bind(Nodes, _mMismatches, _mDistributionStrategy.Tolerance);

            _mMaximumStorageLevel = Nodes.SelectMany(item => item.Storages)
                .Select(item => item.Efficiency)
                .Distinct().Count();
        }

        public void Respond(int tick)
        {
            _mTick = tick;

            CalculateMismatches();
            BalanceSystem();
            CurtailExcessEnergy();
        }

        private void BalanceSystem()
        {
            _mEfficiency = _mExportStrategy.TraverseStorageLevels(_mTick);
            if (_mEfficiency == -1) return; // TODO: This is not very logic.
            _mDistributionStrategy.DistributePower(Nodes, _mMismatches, _mEfficiency, _mTick);
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
        /// Curtail all exess energy and report any negative curtailment (success = false).
        /// </summary>
        private void CurtailExcessEnergy()
        {
            Curtailment = _mMismatches.Sum();
            Failure = Curtailment < -_mMismatches.Length * _mDistributionStrategy.Tolerance;
        }

    }
}
