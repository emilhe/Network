using System.Collections.Generic;
using Optimization;

namespace BusinessLogic.Cost
{
    public class ThreadSafeNodeCostCalculator : INodeCostCalculator
    {

        public bool Busy { get { return _mBusy; } }

        private volatile bool _mBusy;
        private readonly NodeCostCalculator _mCore = new NodeCostCalculator();

        public Dictionary<string, double> DetailedSystemCosts(NodeGenes nodeGenes, bool includeTransmission = false)
        {
            _mBusy = true;
            var result =  _mCore.DetailedSystemCosts(nodeGenes, includeTransmission);
            _mBusy = false;
            return result;
        }

        public double SystemCost(NodeGenes nodeGenes, bool includeTransmission = false)
        {
            _mBusy = true;
            var result = _mCore.SystemCost(nodeGenes, includeTransmission);
            _mBusy = false;
            return result;
        }

    }
}
