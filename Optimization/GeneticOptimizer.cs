using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Optimization
{
    public class GeneticOptimizer<T> where T : IChromosome
    {

        public int Generation { get; private set; }
        public List<double> Steps { get; set; } 

        private readonly ICostCalculator<T> _mCostCalculator;
        private readonly IGeneticOptimizationStrategy<T> _mStrat;

        public GeneticOptimizer(IGeneticOptimizationStrategy<T> optimizationStrategy, ICostCalculator<T> costCalculator)
        {
            _mStrat = optimizationStrategy;
            _mCostCalculator = costCalculator;

            Steps = new List<double>();
        }

        public T Optimize(T[] population)
        {
            Steps.Clear();
            Generation = 0;
            population = OrderPopulation(population);
            var best = population[0].Cost;
            Console.WriteLine("Generation {0}, Cost = {1}", Generation, best);

            while (!_mStrat.TerminationCondition(population, _mCostCalculator.Evaluations))
            {
                // Do genetic stuff.
                _mStrat.Select(population);
                _mStrat.Mate(population);
                _mStrat.Mutate(population);
                // Sort by cost.
                population = OrderPopulation(population);
                // Debug info.
                best = population[0].Cost;
                Steps.Add(best);
                population[0].ToJsonFile(@"C:\proto\bestConfig.txt");
                Generation++;
                Console.WriteLine("Generation {0}, Cost = {1}", Generation, best);
            }

            return population[0];
        }

        private T[] OrderPopulation(T[] unorderedPopulation)
        {
            var start = DateTime.Now;
            _mCostCalculator.UpdateCost(unorderedPopulation);
            var result = unorderedPopulation.OrderBy(item => item.Cost).ToArray();
            var end = DateTime.Now.Subtract(start).TotalSeconds;
            Console.WriteLine("Evaluation took {0} seconds.", end);

            return result;
        }

    }
}
