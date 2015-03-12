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
                if(ExportScheme != null) ExportScheme.Bind(_mNodes, _mMismatches);
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
                _mMismatches = new double[_mNodes.Count];
                if(_mNodes != null) _mExportScheme.Bind(_mNodes, _mMismatches);
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

        #region Construction

        // TODO: Remove HACK
        public NetworkModel(List<CountryNode> nodes, IExportScheme exportScheme,
            IFailureStrategy failureStrategy = null)
            : this(nodes.Select(item => (INode) item).ToList(), exportScheme, failureStrategy)
        {
        }

        public NetworkModel(List<INode> nodes, IExportScheme exportScheme, IFailureStrategy failureStrategy = null)
        {
            if (failureStrategy == null) failureStrategy = new NoBlackoutStrategy();
            _mMismatches = new double[nodes.Count];

            Nodes = nodes;
            ExportScheme = exportScheme;
            FailureStrategy = failureStrategy;
            ExportScheme.Bind(Nodes, _mMismatches);

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

            // Delegate balancing to the export scheme.
            ExportScheme.BalanceSystem();

            // TODO: What about failure scheme?
            // FailureStrategy.Record(Failure);
        }

    }
}
