using BusinessLogic.Cost;
using Optimization;
using Utils;

namespace Main
{
    class Optimization
    {

        public static void Genetic()
        {
            // ReBirth population.
            var n = 500;
            var calc = new CostCalculator();
            var strategy = new GeneticNodeOptimizationStrategy(calc);
            var population = new IChromosome[n];

            for (int i = 0; i < population.Length; i++) population[i] = strategy.Spawn();

            // The (so far) best optimum.
            //for (int i = 0; i < population.Length; i++)
            //{
            //    var dna = new NodeDna(0.5 + 0.5 * i / population.Length, 1, 16);
            //    population[i] = new NodeChromosome(dna, calc);
            //}

            // Find optimum.
            var optimizer = new GeneticOptimizer<NodeChromosome>(strategy);
            var optimum = optimizer.Optimize(population);
            optimum.Dna.ToJsonFile(@"C:\proto\genetic.txt");
        }

        public static void SimulatedAnnealing()
        {
            var calc = new CostCalculator();
            var strategy = new SaNodeOptimizationStrategy(calc);
            var optimizer = new SaOptimizer<NodeChromosome>(strategy)
            {
                // Performance CRITIAL parameters.
                Alpha = 0.999,
                Epsilon = 0.001,
                Temperature = 100
            };

            var optimum = optimizer.Optimize(strategy.Spawn());
            optimum.Dna.ToJsonFile(@"C:\proto\simulatedAnnealing.txt");
        }

    }
}
