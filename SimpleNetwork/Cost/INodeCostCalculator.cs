using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;

namespace BusinessLogic.Cost
{
    interface INodeCostCalculator
    {

        Dictionary<string, double> DetailedSystemCosts(NodeGenes nodeGenes);

        double SystemCost(NodeGenes nodeGenes);

    }
}
