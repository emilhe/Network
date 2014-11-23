using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface IGeneticOptimizationStrategy<T> where T : IChromosome
    {

        bool TerminationCondition(IChromosome[] chromosomes);

        void Select(IChromosome[] chromosomes);
        void Mate(IChromosome[] chromosomes);
        void Mutate(IChromosome[] chromosomes);

    }
}
