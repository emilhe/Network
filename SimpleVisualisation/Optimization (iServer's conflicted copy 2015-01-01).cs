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
            var strategy = new GeneticNodeOptimizationStrategy();
            var population = new NodeChromosome[n];

            for (int i = 0; i < population.Length; i++)
            {
                population[i] = GenePool.Spawn();
                //population[i] = new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\geneticWithConstraintK=1mio.txt"));
            }

            // The (so far) best optimum.
            //for (int i = 0; i < population.Length; i++)
            //{
            //    var Genes = new NodeGenes(0.5 + 0.5 * i / population.Length, 1, 16);
            //    population[i] = new NodeChromosome(Genes, calc);
            //}

            // Find optimum.
            var optimizer = new GeneticOptimizer<NodeChromosome>(strategy, new ParallelCostCalculator(){Full = false});
            var optimum = optimizer.Optimize(population);
            optimum.Genes.ToJsonFile(@"C:\proto\onshoreVEgeneticConstraintTransK=1.txt");
        }

        public static void SimulatedAnnealing()
        {
            var optimizer = new SaOptimizer<NodeChromosome>(new ParallelCostCalculator())
            {
                // Performance CRITIAL parameters.
                Alpha = 0.9999,
                Epsilon = 0.1,
                Temperature = 5
            };

            var optimum = optimizer.Optimize(new NodeChromosome(new NodeGenes(GenePool.RndGene)));
            optimum.Genes.ToJsonFile(@"C:\proto\simulatedAnnealing.txt");
        }

    }
}
