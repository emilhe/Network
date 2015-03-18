using System;
using System.Runtime.CompilerServices;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using Optimization;
using Utils;

namespace Main
{
    class Optimization
    {

        public static void Genetic(int k, int n, string key = "")
        {
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
            optimum.Genes.ToJsonFile(string.Format(@"C:\proto\onshoreVEgeneticConstraintTransK={0}{1}.txt", k, key));
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
            var optimizer = new CukooOptimizer<NodeChromosome>(strategy, calc);
            // Do stuff.
            var optimum = optimizer.Optimize(population);
            optimizer.Steps.ToJsonFile(name + ".tex");
            optimum.Genes.ToJsonFile(name + "@steps.tex");
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

        public static void Simplex()
        {
            GenePool.K = 2;
            // Build simplex.
            var cord = 0.25;
            var center = new NodeVec(() => GenePool.K / 2, () => (GenePool.AlphaMax-GenePool.AlphaMin)/2 + GenePool.AlphaMin);
            var n = center.Length;
            var simplex = new NodeVec[n + 1];
            for (int i = 0; i < n+1; i++)
            {
                var vertex = center.GetVectorCopy();
                if (i < n)
                {
                    //var sign = (i < simplex.Length / 2) ? ((vertex[i] < GenePool.K / 2) ? 1 : -1) : ((vertex[i] < 0.5) ? 1 : -1);
                    vertex[i] = vertex[i] + cord; // sign*cord;
                }
                simplex[i] = new NodeVec(vertex);
            }
            // Build optimizer.
            var optimizer = new SimplexOptimizer<NodeVec>(new SimplexNodeOptimizationStrategy(),
                new NodeVecCostCalculator())
            {
                Beta = 1 + 2.0/n,
                Gamma = 0.75 - 1.0/(2*n),
                Delta = 1 - 1.0/n
            };
            // Optimize.
            var optimum = optimizer.Optimize(simplex);
            optimum.ToJsonFile(@"C:\proto\simplex.txt");
        }

    }
}
