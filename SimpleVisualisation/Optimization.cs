﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.Utils;
using Optimization;
using Optimization.OldOptimization;
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
                    Transmission = true,
                };
            }
            var optimizer = new CukooOptimizer<NodeChromosome>(strategy, calc);
            // Do stuff.
            var optimum = optimizer.Optimize(population);
            //optimizer.Steps.ToJsonFile(name + ".tex");
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
            var cord = -0.1;
            var center = new NodeVec(() => 1, () => 1);
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
            (new NodeChromosome(optimum)).Genes.ToJsonFile(@"C:\proto\simplex.txt");
        }

        public static void Sequential(int k, string tag = "", ParallelNodeCostCalculator calc = null, NodeChromosome seed = null)
        {

            if(seed == null) seed = new NodeChromosome(new NodeGenes(1, 1));
            GenePool.ApplyOffshoreFraction(seed.Genes);
            GenePool.K = k;
            if (calc == null)
            {
                calc = new ParallelNodeCostCalculator()
                {
                    Full = false,
                    CacheEnabled = false,
                    Transmission = true
                };
            }
            var opt = new SequentialOptimizer(calc);
            var best = opt.Optimize(seed, 0.5, 0.005);

            Console.WriteLine("Final cost is {0} after {1} evals", best.Cost, opt.Evals);
            best.Genes.ToJsonFile(string.Format(@"C:\proto\localK={0}{1}.txt", k, tag));
        }

        private static List<string> GammaRescaling(NodeChromosome chromosome, List<string> fixedKeys)
        {
            var genes = chromosome.Genes;

            // Calculte new effective gamma (for VARAIBLE .
            var varWind = 0.0;
            var fixedWind = 0.0;
            var varSolar = 0.0;
            var fixedSolar = 0.0;
            foreach (var key in genes.Keys)
            {
                var load = CountryInfo.GetMeanLoad(key);
                var wind = genes[key].Gamma * load * genes[key].Alpha;
                var solar = genes[key].Gamma * load * (1 - genes[key].Alpha);
                if (fixedKeys.Contains(key))
                {
                    fixedWind += wind;
                    fixedSolar += solar;
                }
                else
                {
                    varWind += wind;
                    varSolar += solar;   
                }
            }
            var effGamma = (varWind + varSolar)/(CountryInfo.GetMeanLoadSum() - (fixedWind + fixedSolar));

            // Check if the new effective gamma violates the contstraints.
            var failure = false;
            foreach (var key in genes.Keys)
            {
                if(fixedKeys.Contains(key)) continue;
                var newGamma = genes[key].Gamma*chromosome.Gamma/effGamma;
                if (GenePool.GammaMin - newGamma > 1e-5)
                {
                    // Normalization NOT possible. Let's fix this value and try again.
                    genes[key].Gamma = GenePool.GammaMin;
                    fixedKeys.Add(key);
                    failure = true;

                }
                if (GenePool.GammaMax - newGamma < -1e-5)
                {
                    // Normalization NOT possible. Let's fix this value and try again.
                    genes[key].Gamma = GenePool.GammaMax;
                    fixedKeys.Add(key);
                    failure = true;
                }
            }
            // Violation; try fixing the problematic values and do another rescaling.
            if (failure) return GammaRescaling(chromosome, fixedKeys);
            
            // No violation; just do the rescaling!
            foreach (var key in chromosome.Genes.Keys)
            {
                if (fixedKeys.Contains(key)) continue;
                chromosome.Genes[key].Gamma /= effGamma;
            }

            var scale = GenePool.GammaRescaling(chromosome);

            return fixedKeys;;
        }

        class SequentialOptimizer
        {

            public double Evals { get { return _mCalc.Evaluations; } }

            private readonly ParallelNodeCostCalculator _mCalc;
            private NodeChromosome[] _mClones;
            private NodeChromosome _mBest;
            private double _mStep;

            public SequentialOptimizer(ParallelNodeCostCalculator calc)
            {
                _mCalc = calc;
            }

            public NodeChromosome Optimize(NodeChromosome seed, double stepStart, double stepEnd)
            {
                _mBest = seed;
                _mCalc.UpdateCost(new []{seed});
                _mClones = new NodeChromosome[seed.Genes.Count];
                var cost = 1000.0;
                _mStep = stepStart;
                Console.WriteLine("Cost is {0} after {1} evals", _mBest.Cost, _mCalc.Evaluations);
                if (File.Exists(@"C:\proto\seqStats.txt")) File.Move(@"C:\proto\seqStats.txt", @"C:\proto\seqStats" + DateTime.Now.ToString("dd-MM-yy hh_mm") + ".txt");
                File.Create(@"C:\proto\seqStats.txt").Close();

                while (_mStep > stepEnd)
                {
                    while (cost-_mBest.Cost > 1e-3)
                    {
                        cost = _mBest.Cost;
                        _mBest.Genes.ToJsonFile(@"C:\proto\bestSeq.txt");
                        File.AppendAllLines(@"C:\proto\seqStats.txt",
                            new[] {string.Format("{0},{1}", _mBest.Cost, _mCalc.Evaluations)});

                        // Take best possible alpha step.
                        TakeBestStep((a, b) => StepAlphaDown(a, b, _mStep), (a, b) => StepAlphaUp(a, b, _mStep));
                        Console.WriteLine("Cost is {0} after {1} evals", _mBest.Cost, _mCalc.Evaluations);

                        if (GenePool.GammaMin == GenePool.GammaMax) continue;

                        // Take best possible gamma step.
                        TakeBestStep((a, b) => StepGammaDown(a, b, _mStep), (a, b) => StepGammaUp(a, b, _mStep));
                        Console.WriteLine("Cost is {0} after {1} evals", _mBest.Cost, _mCalc.Evaluations);
                    }
                    cost = 1000.0;
                    _mStep /= 2;
                }

                return _mBest;
            }

            private void TakeBestStep(Func<NodeChromosome, string, bool> StepDown,
                Func<NodeChromosome, string, bool> StepUp)
            {
                var n = _mBest.Genes.Count;
                var labels = NodeVec.Labels;
                _mClones.Fill(() => new NodeChromosome(_mBest.Genes.Clone()));
                for (int i = 0; i < n; i++)
                {
                    if (StepDown(_mClones[i], labels[i])) continue;
                    // Step down FAILED: Set cost manually.
                    _mClones[i].Cost = _mBest.Cost;
                }
                _mCalc.UpdateCost(_mClones);
                for (int i = 0; i < n; i++)
                {
                    // Was step-down OK? If so, just proceed.
                    if (_mClones[i].Cost < _mBest.Cost) continue;
                    _mClones[i] = new NodeChromosome(_mBest.Genes.Clone());
                    if (StepUp(_mClones[i], labels[i])) continue;
                    // Step up FAILED: Set cost manually.
                    _mClones[i].Cost = _mBest.Cost;
                }
                _mCalc.UpdateCost(_mClones);
                _mBest = _mClones.OrderBy(item => item.Cost).First();
            }

            static bool StepAlphaDown(NodeChromosome chromo, string key, double delta)
            {
                var gene = chromo.Genes[key];
                if (gene.Alpha == GenePool.AlphaMin) return false;

                var newVal = gene.Alpha - delta;
                if (newVal < GenePool.AlphaMin) newVal = GenePool.AlphaMin;
                gene.Alpha = newVal;
                return true;
            }

            static bool StepAlphaUp(NodeChromosome chromo, string key, double delta)
            {
                var gene = chromo.Genes[key];
                if (gene.Alpha == GenePool.AlphaMax) return false;

                var newVal = gene.Alpha + delta;
                if (newVal > GenePool.AlphaMax) newVal = GenePool.AlphaMax;
                gene.Alpha = newVal;
                return true;
            }

            static bool StepGammaDown(NodeChromosome chromo, string key, double delta)
            {
                var gene = chromo.Genes[key];
                if (gene.Gamma == GenePool.GammaMin) return false;

                var newVal = gene.Gamma - delta;
                if (newVal < GenePool.GammaMin) newVal = GenePool.GammaMin;
                gene.Gamma = newVal;
                GammaRescaling(chromo, new List<string> { key });
                return true;
            }

            static bool StepGammaUp(NodeChromosome chromo, string key, double delta)
            {
                var gene = chromo.Genes[key];
                if (gene.Gamma == GenePool.GammaMax) return false;

                var newVal = gene.Gamma + delta;
                if (newVal > GenePool.GammaMax) newVal = GenePool.GammaMax;
                gene.Gamma = newVal;
                GammaRescaling(chromo, new List<string> { key });
                return true;
            }

        }

    }
}
