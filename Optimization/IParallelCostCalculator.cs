using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface IParallelCostCalculator
    {

        void UpdateCost(IChromosome[] chromosomes);

    }
}
