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

        private IExportStrategy _mExportStrategy;
        private BalanceResult _mBalanceResult;
        private IList<INode> _mNodes;
        private double[] _mMismatches;

        #region Public properties (exposed by simulation core).

        public IList<INode> Nodes
        {
            get { return _mNodes; }
            set
            {
                _mNodes = value;
                _mMismatches = new double[_mNodes.Count];
                if(ExportStrategy != null) ExportStrategy.Bind(_mNodes, _mMismatches);
            }
        }

        public IExportStrategy ExportStrategy
        {
            get
            {
                return _mExportStrategy;
            }
            set
            {
                _mExportStrategy = value;
                _mMismatches = new double[_mNodes.Count];
                if(_mNodes != null) _mExportStrategy.Bind(_mNodes, _mMismatches);
            }
        }

        public IFailureStrategy FailureStrategy { get; set; }

        #endregion

        #region Simulation parameters

        public double Mismatch { get; private set; }

        public double Curtailment
        {
            get { return _mBalanceResult.Curtailment; }
        }

        public bool Failure
        {
            get { return FailureStrategy.Failure; }
        }

        #endregion

        #region Construction

        // TODO: Remove HACK
        public NetworkModel(List<CountryNode> nodes, IExportStrategy exportStrategy,
            IFailureStrategy failureStrategy = null)
            : this(nodes.Select(item => (INode) item).ToList(), exportStrategy, failureStrategy)
        {
        }

        public NetworkModel(List<INode> nodes, IExportStrategy exportStrategy, IFailureStrategy failureStrategy = null)
        {
            if (failureStrategy == null) failureStrategy = new NoBlackoutStrategy();
            _mMismatches = new double[nodes.Count];

            Nodes = nodes;
            ExportStrategy = exportStrategy;
            FailureStrategy = failureStrategy;
            ExportStrategy.Bind(Nodes, _mMismatches);

        }

        #endregion

        public void Evaluate(int tick)
        {
            // Calculate mismatches.
            for (int i = 0; i < Nodes.Count; i++)
            {
                _mMismatches[i] = Nodes[i].GetDelta();
            }
            Mismatch = _mMismatches.Sum();

            // Delegate balancing to the export strategy.
            _mBalanceResult = ExportStrategy.BalanceSystem();
            FailureStrategy.Record(_mBalanceResult);
        }

    }
}
