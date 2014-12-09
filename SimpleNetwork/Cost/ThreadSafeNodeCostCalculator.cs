using System.Collections.Generic;
using Optimization;

namespace BusinessLogic.Cost
{
    public class ThreadSafeNodeCostCalculator : INodeCostCalculator
    {

        public bool Busy { get { return _mBusy; } set { _mBusy = value; } }

        private volatile bool _mBusy;
        private readonly NodeCostCalculator _mCore = new NodeCostCalculator(false);

        public Dictionary<string, double> DetailedSystemCosts(NodeGenes nodeGenes, bool includeTransmission = false)
        {
            return _mCore.DetailedSystemCosts(nodeGenes, includeTransmission);
        }

        public double SystemCost(NodeGenes nodeGenes, bool includeTransmission = false)
        {
            return _mCore.SystemCost(nodeGenes, includeTransmission);
        }

    }
}
