using System;
using System.Linq;

namespace Optimization
{
    public class GeneticOptimizer<T> where T : IChromosome
    {

        private readonly IGeneticOptimizationStrategy<T> _mStrat;

        public GeneticOptimizer(IGeneticOptimizationStrategy<T> geneticOptimizationStrategy)
        {
            _mStrat = geneticOptimizationStrategy;
        }

        public T Optimize(IChromosome[] population)
        {
            while (!_mStrat.TerminationCondition(population))
            {
                // Do genetic stuff.
                _mStrat.Select(population);
                _mStrat.Mate(population);
                _mStrat.Mutate(population);
                // Sort by cost.
                population = population.OrderBy(item => item.Cost).ToArray();
            }

            return (T) population.First();
        }

    }
}
