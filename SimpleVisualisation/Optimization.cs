using System;
using System.Runtime.CompilerServices;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using Optimization;
using Optimization.OldOptimization;
using Utils;

namespace Main
{
    class Optimization
    {

        public static void Genetic(int k, int n, string key = "")
        {
            var name = string.Format(@"C:\proto\VE50gaK={0}@{1}", k, key);
            // Adjust gene pool.
            GenePool.K = k;
            // Setup stuff.
            var strategy = new GeneticNodeOptimizationStrategy();
            var population = new NodeChromosome[n];
            for (int i = 0; i < population.Length; i++) population[i] = GenePool.SpawnChromosome();
            //population[0] = new NodeChromosome(NodeGenesFactory.SpawnCfMax(1, 1, k));
            var optimizer = new GeneticOptimizer<NodeChromosome>(strategy, new ParallelNodeCostCalculator {Full = false, Transmission = false});
            // Do stuff.
            var optimum = optimizer.Optimize(population);
            optimizer.Steps.ToJsonFile(name + "@steps.txt");
            optimum.Genes.ToJsonFile(name + ".txt");
        }

        public static void Cukoo(int k, int n = 500, string key = "", NodeChromosome seed = null, ParallelNodeCostCalculator calc = null)
        {
            var name = string.Format(@"C:\proto\VE50cukooK={0}@{1}", k, key);
            // Adjust gene pool.
            GenePool.K = k;
            // Setup stuff.
            var strategy = new CukooNodeOptimizationStrategy();
            var population = new NodeChromosome[n];
            for (int i = 0; i < population.Length; i++) population[i] = GenePool.SpawnChromosome();
            if (seed != null) population[0] = seed;
            //population[1] = new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(name));
            if (calc == null)
            {
                calc = new ParallelNodeCostCalculator
                {
                    Full = false,
                    Transmission = false,
                };
            }
            var optimizer = new PureCukooOptimizer<NodeChromosome>(strategy, calc);
            // Do stuff.
            var optimum = optimizer.Optimize(population);
            optimizer.Steps.ToJsonFile(name + "@steps.txt");
            optimum.Genes.ToJsonFile(name + ".txt");
            Console.WriteLine("K = {0} ({1}) has cost {2}", k, key, optimum.Cost);
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
            var optimizer = new SaOptimizer<NodeChromosome>(new ParallelNodeCostCalculator())
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
