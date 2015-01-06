using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Utils;
using Optimization;

namespace BusinessLogic.Cost
{
    public class SerialCostCalculator : ICostCalculator<NodeChromosome>
    {

        private readonly NodeCostCalculator _mCalc = new NodeCostCalculator(new ParameterEvaluator(false) {CacheEnabled = false});

        public void UpdateCost(IEnumerable<NodeChromosome> chromosomes)
        {
            foreach (var chromosome in chromosomes)
            {
                chromosome.UpdateCost(_mCalc);
            }
        }
    }
}
