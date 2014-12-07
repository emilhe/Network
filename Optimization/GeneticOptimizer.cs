using System;
using System.Linq;
using Utils;

namespace Optimization
{
    public class GeneticOptimizer<T> where T : IChromosome
    {

        public IParallelCostCalculator ParallelCostCalculator { get; set; }
        public int Generation { get; private set; }

        private readonly IGeneticOptimizationStrategy<T> _mStrat;

        public GeneticOptimizer(IGeneticOptimizationStrategy<T> optimizationStrategy)
        {
            _mStrat = optimizationStrategy;
        }

        public T Optimize(IChromosome[] population)
        {
            Generation = 0;
            population = OrderPopulation(population);
            var best = population.First().Cost;
            Console.WriteLine("Generation {0}, Cost = {1}", Generation, best);

            while (!_mStrat.TerminationCondition(population))
            {
                // Do genetic stuff.
                _mStrat.Select(population);
                _mStrat.Mate(population);
                _mStrat.Mutate(population);
                // Sort by cost.
                population = OrderPopulation(population);
                // Debug info.
                best = population[0].Cost;
                population[0].ToJsonFile(@"C:\proto\bestConfig.txt");
                Generation++;
                Console.WriteLine("Generation {0}, Cost = {1}", Generation, best);
            }

            return (T) population.First();
        }

        private IChromosome[] OrderPopulation(IChromosome[] unorderedPopulation)
        {
            var start = DateTime.Now;
            if (ParallelCostCalculator != null) ParallelCostCalculator.UpdateCost(unorderedPopulation);
            var result = unorderedPopulation.OrderBy(item => item.Cost).ToArray();
            var end = start.Subtract(DateTime.Now).TotalSeconds;

            return result;
        }

    }
}
