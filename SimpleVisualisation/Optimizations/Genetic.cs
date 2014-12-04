using BusinessLogic.Cost;
using Optimization;
using Utils;

namespace Main.Optimizations
{
    class Genetic
    {

        public static void Run()
        {
            // ReBirth population.
            var n = 50;
            var strategy = new GeneticNodeOptimizationStrategy(new CostCalculator());
            var population = new IChromosome[n];
            for (int i = 0; i < population.Length; i++) population[i] = strategy.Spawn();
            // Find optimum.
            var optimizer = new GeneticOptimizer<NodeChromosome>(strategy);
            var optimum = optimizer.Optimize(population);
            optimum.ToJsonFile(@"C:\proto\genetic.txt");
        }

    }
}
