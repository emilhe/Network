using BusinessLogic.Cost;
using Optimization;
using Utils;

namespace Main
{
    class Optimization
    {

        public static void Genetic(int k, int n)
        {
            // Adjust gene pool.
            GenePool.K = 1;
            // Setup stuff.
            var strategy = new GeneticNodeOptimizationStrategy();
            var population = new NodeChromosome[n];
            for (int i = 0; i < population.Length; i++) population[i] = GenePool.SpawnChromosome();
            var optimizer = new GeneticOptimizer<NodeChromosome>(strategy, new ParallelCostCalculator<NodeChromosome> {Full = false, Transmission = false});
            // Do stuff.
            var optimum = optimizer.Optimize(population);
            optimum.Genes.ToJsonFile(string.Format(@"C:\proto\onshoreVEgeneticConstraintTransK={0}.txt", k));
        }

        public static void Cukoo(int k, int n)
        {
            // Adjust gene pool.
            GenePool.K = 1;
            // Setup stuff.
            var strategy = new CukooNodeOptimizationStrategy();
            var population = new NodeChromosome[n];
            for (int i = 0; i < population.Length; i++) population[i] = GenePool.SpawnChromosome();
            var optimizer = new CukooOptimizer<NodeChromosome>(strategy, new ParallelCostCalculator<NodeChromosome> { Full = true, Transmission = true });
            // Do stuff.
            var optimum = optimizer.Optimize(population);
            optimum.Genes.ToJsonFile(string.Format(@"C:\proto\onshoreVE32cukooConstraintTransK={0}.txt", k));
        }

        //public static void ParticleSwarm()
        //{
        //    // ReBirth population.
        //    var n = 500;
        //    var strategy = new GeneticNodeOptimizationStrategy();
        //    var population = new NodeChromosome[n];

        //    for (int i = 0; i < population.Length; i++)
        //    {
        //        population[i] = GenePool.SpawnParticle();
        //        //population[i] = new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\geneticWithConstraintK=1mio.txt"));
        //    }

        //    // The (so far) best optimum.
        //    //for (int i = 0; i < population.Length; i++)
        //    //{
        //    //    var Genes = new NodeGenes(0.5 + 0.5 * i / population.Length, 1, 16);
        //    //    population[i] = new NodeChromosome(Genes, calc);
        //    //}

        //    //// Find optimum.
        //    //var optimizer = new PsOptimizer<NodeChromosome>(strategy, new ParallelCostCalculator() { Full = false });
        //    //var optimum = optimizer.Optimize(population);
        //    //optimum.Genes.ToJsonFile(@"C:\proto\onshoreVEgeneticConstraintTransK=3.txt");
        //}

        public static void SimulatedAnnealing()
        {
            var optimizer = new SaOptimizer<NodeChromosome>(new ParallelCostCalculator<NodeChromosome>())
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
