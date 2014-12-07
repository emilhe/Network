using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;

namespace BusinessLogic.Cost
{
    interface INodeCostCalculator : ICostCalculator
    {

        Dictionary<string, double> DetailedSystemCosts(NodeGenes nodeGenes, bool includeTransmission = false);

        double SystemCost(NodeGenes nodeGenes, bool includeTransmission = false);

    }
}
