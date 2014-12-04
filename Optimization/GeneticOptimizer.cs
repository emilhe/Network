﻿using System;
using System.Linq;

namespace Optimization
{
    public class GeneticOptimizer<T> where T : IChromosome
    {

        public int Generation { get; private set; }

        private readonly IGeneticOptimizationStrategy<T> _mStrat;

        public GeneticOptimizer(IGeneticOptimizationStrategy<T> optimizationStrategy)
        {
            _mStrat = optimizationStrategy;
        }

        public T Optimize(IChromosome[] population)
        {
            Generation = 0;
            population = population.OrderBy(item => item.Cost).ToArray();
            var best = population.First().Cost;
            Console.WriteLine("Generation {0}, Cost = {1}", Generation, best);

            while (!_mStrat.TerminationCondition(population))
            {
                // Do genetic stuff.
                _mStrat.Select(population);
                _mStrat.Mate(population);
                _mStrat.Mutate(population);
                // Sort by cost.
                population = population.OrderBy(item => item.Cost).ToArray();
                // Debug info.
                best = population[0].Cost;
                Generation++;
                Console.WriteLine("Generation {0}, Cost = {1}", Generation, best);
            }

            return (T) population.First();
        }

    }
}
