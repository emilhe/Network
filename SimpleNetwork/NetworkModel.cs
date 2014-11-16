using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BusinessLogic.ExportStrategies;
using BusinessLogic.FailureStrategies;
using BusinessLogic.Interfaces;

namespace BusinessLogic
{
    public class NetworkModel
    {
        public List<Node> Nodes { get; private set; }
        public double Mismatch { get; private set; }

        public double Curtailment
        {
            get { return _mBalanceResult.Curtailment; }
        }

        public bool Failure
        {
            get { return FailureStrategy.Failure; }
        }

        public IFailureStrategy FailureStrategy { get; private set; }
        public IExportStrategy ExportStrategy { get; private set; }
        private BalanceResult _mBalanceResult;
        private readonly double[] _mMismatches;

        public NetworkModel(List<Node> nodes, IExportStrategy exportStrategy, IFailureStrategy failureStrategy = null)
        {
            if (failureStrategy == null) failureStrategy = new NoBlackoutStrategy();
            _mMismatches = new double[nodes.Count];

            Nodes = nodes;
            ExportStrategy = exportStrategy;
            FailureStrategy = failureStrategy;
            ExportStrategy.Bind(Nodes, _mMismatches);

        }

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
