using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface IGeneticOptimizationStrategy<T> where T : IChromosome
    {

        bool TerminationCondition(T[] chromosomes, int evaluations);

        void Select(T[] chromosomes);
        void Mate(T[] chromosomes);
        void Mutate(T[] chromosomes);

    }
}
