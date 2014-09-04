using System.Collections.Generic;
using System.Linq;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;

namespace BusinessLogic
{
    public class NetworkModel
    {
        public List<Node> Nodes { get; private set; }
        public double Mismatch { get; private set; }
        public double Curtailment { get { return _mBalanceResult.Curtailment; } }
        public bool Failure { get { return _mBalanceResult.Failure; } }
        public IExportStrategy ExportStrategy { get; private set; }

        private readonly double[] _mMismatches;
        private BalanceResult _mBalanceResult;

        public NetworkModel(List<Node> nodes, IExportStrategy exportStrategy)
        {
            Nodes = nodes;
            ExportStrategy = exportStrategy;
            _mMismatches = new double[Nodes.Count];
            ExportStrategy.Bind(Nodes, _mMismatches);
        }

        public void Evaluate(int tick)
        {
            // Calculate mismatches.
            for (int i = 0; i < Nodes.Count; i++) _mMismatches[i] = Nodes[i].GetDelta(tick);
            Mismatch = _mMismatches.Sum();
            // Delegate balancing to the export strategy.
            _mBalanceResult = ExportStrategy.BalanceSystem(tick);
        }

    }
}
