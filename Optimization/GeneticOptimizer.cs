using System;
using System.Linq;

namespace Optimization
{
    public class GeneticOptimizer<T> where T : IChromosome
    {

        public int Generation { get; private set; }

        private readonly IGeneticOptimizationStrategy<T> _mStrat;

        public GeneticOptimizer(IGeneticOptimizationStrategy<T> geneticOptimizationStrategy)
        {
            _mStrat = geneticOptimizationStrategy;
        }

        public T Optimize(IChromosome[] population)
        {
            Generation = 0;

            while (!_mStrat.TerminationCondition(population))
            {
                // Do genetic stuff.
                _mStrat.Select(population);
                _mStrat.Mate(population);
                _mStrat.Mutate(population);
                // Sort by cost.
                population = population.OrderBy(item => item.Cost).ToArray();
                // Debug info.
                var best = population[0].Cost;
                Generation++;
            }

            return (T) population.First();
        }

    }
}
