using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;

namespace BusinessLogic.Cost
{
    public class SerialCostCalculator : ICostCalculator<NodeChromosome>
    {

        private readonly NodeCostCalculator _mCalc = new NodeCostCalculator(false);

        public void UpdateCost(NodeChromosome[] chromosomes)
        {
            foreach (var chromosome in chromosomes)
            {
                chromosome.UpdateCost(_mCalc);
            }
        }
    }
}
