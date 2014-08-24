using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    public class SelfishExportStrategy : IExportStrategy
    {

        private readonly CooperativeExportStrategy _mCooperativeExportStrategy = new CooperativeExportStrategy();
        private readonly NoExportStrategy _mNoExportExportStrategy = new NoExportStrategy();
        private double[] _mMismatches;

        public void Bind(List<Node> nodes, double[] mismatches, double tolerance = 0)
        {
            _mMismatches = mismatches;
            ((IExportStrategy) _mCooperativeExportStrategy).Bind(nodes, mismatches, tolerance);
            ((IExportStrategy)_mNoExportExportStrategy).Bind(nodes, mismatches, tolerance);
        }

        public double TraverseStorageLevels(int tick)
        {
            _mNoExportExportStrategy.TraverseStorageLevels(tick, i => _mMismatches[i] > 0);
            return _mCooperativeExportStrategy.TraverseStorageLevels(tick);
        }

    }
}
