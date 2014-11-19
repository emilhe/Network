using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class GeneticOptimizer
    {

        public Func<bool> TerminationCondition { get; set; }

        public int ReuseCount { get; set; }
        public int KillPercentage { get; set; }

        public GeneticOptimizer(Func<bool> terminationCondition)
        {
            TerminationCondition = terminationCondition;

            ReuseCount = 12;
        }

        public IChromosome Optimize(IChromosome[] population)
        {

            while (!TerminationCondition())
            {
                // SELECT: Spawn new chromosomes, but keep the X best of the old ones.
                for (int i = ReuseCount; i < population.Length; i++)
                {
                    population[i] = population[0].Spawn();
                }
                // MATE: Mate layouts based
                var orderedPopulation = population.OrderBy(item => item.Cost);
                var idx = 0;
                foreach (var chromosome in orderedPopulation)
                {
                    // Do mating.
                    if (idx < ReuseCount)
                    {
                        
                    }

                }
            }

            return null;
        }

    }
}
