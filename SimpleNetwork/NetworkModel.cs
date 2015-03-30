using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BusinessLogic.ExportStrategies;
using BusinessLogic.FailureStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;

namespace BusinessLogic
{
    public class NetworkModel
    {

        private IExportScheme _mExportScheme;
        private INode[] _mNodes;
        private double[] _mMismatches;

        #region Public properties (exposed by simulation core).

        public INode[] Nodes
        {
            get { return _mNodes; }
            set
            {
                _mNodes = value;
                _mMismatches = new double[_mNodes.Length];
                if(ExportScheme != null) ExportScheme.Bind(_mMismatches);
            }
        }

        public IExportScheme ExportScheme
        {
            get
            {
                return _mExportScheme;
            }
            set
            {
                _mExportScheme = value;
                _mMismatches = new double[_mNodes.Length];
                if(_mNodes != null) _mExportScheme.Bind(_mMismatches);
            }
        }

        public IFailureStrategy FailureStrategy { get; set; }

        #endregion

        #region Simulation parameters

        public double Mismatch { get; private set; }

        public double Curtailment
        {
            get { return _mNodes.Select(item => item.Curtailment).Sum(); }
        }

        public double Backup
        {
            get { return _mNodes.Select(item => item.Backup).Sum(); }
        }

        public bool Failure { get; private set; }

        #endregion

        public NetworkModel(INode[] nodes, IExportScheme exportScheme, IFailureStrategy failureStrategy = null)
        {
            if (failureStrategy == null) failureStrategy = new NoBlackoutStrategy();
            _mMismatches = new double[nodes.Length];

            Nodes = nodes;
            ExportScheme = exportScheme;
            FailureStrategy = failureStrategy;
            ExportScheme.Bind(_mMismatches);
        }

        public void Evaluate(int tick)
        {
            // Calculate mismatches.
            for (int i = 0; i < Nodes.Length; i++)
            {
                _mMismatches[i] = Nodes[i].GetDelta();
            }
            Mismatch = _mMismatches.Sum();

            // Delegate balancing to the export scheme.
            try
            {
                ExportScheme.BalanceSystem();
            }
            catch (Exception ex)
            {
                Console.WriteLine("System balancing failure.");
                throw;
            }

            // TODO: What about failure scheme?
            // FailureStrategy.Record(Failure);
        }

    }
}
