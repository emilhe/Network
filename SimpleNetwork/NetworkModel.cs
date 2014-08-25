using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.ExportStrategies;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork
{
    public class NetworkModel
    {
        public List<Node> Nodes { get; private set; }
        public double Mismatch { get; private set; }
        public double Curtailment { get { return _mBalanceResult.Curtailment; } }
        public bool Failure { get { return _mBalanceResult.Failure; } }

        private readonly IExportStrategy _mExportStrategy;
        private readonly double[] _mMismatches;
        private BalanceResult _mBalanceResult;

        public NetworkModel(List<Node> nodes, IExportStrategy exportStrategy)
        {
            Nodes = nodes;
            _mExportStrategy = exportStrategy;
            _mMismatches = new double[Nodes.Count];
            _mExportStrategy.Bind(Nodes, _mMismatches);
        }

        public void Respond(int tick)
        {
            // Calculate mismatches.
            for (int i = 0; i < Nodes.Count; i++) _mMismatches[i] = Nodes[i].GetDelta(tick);
            Mismatch = _mMismatches.Sum();
            // Delegate balancing to the export strategy.
            _mBalanceResult = _mExportStrategy.BalanceSystem(tick);
        }

    }
}
