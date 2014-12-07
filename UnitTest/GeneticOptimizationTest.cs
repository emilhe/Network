using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Optimization;
using Utils;

namespace UnitTest
{
    [TestFixture]
    class GeneticOptimizationTest
    {

        [Test]
        public void HelloWorld()
        {
            //// ReBirth population.
            //var n = 100;
            //var strategy = new GeneticStringOptimizationStrategy();
            //var population = new IChromosome[n];
            //for (int i = 0; i < population.Length; i++) population[i] = GeneticStringOptimizationStrategy.Spawn();
            //// Find optimum.
            //var optimizer = new GeneticOptimizer<HelloWorldChromosome>(strategy);
            //var optimum = optimizer.Optimize(population);
            //Assert.AreEqual("Hello World!", optimum.Genes);
        }

        class GeneticStringOptimizationStrategy : IGeneticOptimizationStrategy<HelloWorldChromosome>
        {

            private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);
            
            private const int Min = 0;
            private const int Max = 255;

            private const int ChildCount = 75;            
            private const int EliteCount = 15;
            private const int EliteMixCount = 1;

            public bool TerminationCondition(HelloWorldChromosome[] chromosomes)
            {
                return Math.Abs(chromosomes[0].Cost) < 1e-10;
            }

            public void Select(HelloWorldChromosome[] chromosomes)
            {
                // Kill bad candidates.
                for (int i = EliteCount; i < EliteCount + EliteMixCount; i++) chromosomes[i] = Spawn();
            }

            public void Mate(HelloWorldChromosome[] chromosomes)
            {
                var offspring = new HelloWorldChromosome[chromosomes.Length * 2];

                // Find children.
                for (int i = 0; i < ChildCount; i++)
                {
                    var father = chromosomes[(int) (Rnd.NextDouble()*(EliteCount+EliteMixCount - 1))];
                    var mother = chromosomes[(int) (Rnd.NextDouble()*(EliteCount+EliteMixCount - 1))];
                    offspring[i] = (HelloWorldChromosome) father.Mate(mother);
                }

                // Fill in children + randoms.
                for (int i = 0; i < ChildCount; i++)
                {
                    chromosomes[i] = (i < offspring.Length) ? offspring[i] : Spawn();
                }
            }

            public void Mutate(HelloWorldChromosome[] chromosomes)
            {
                foreach (var chromosome in chromosomes) chromosome.Mutate();
            }

            public static HelloWorldChromosome Spawn()
            {
                var charArray = new char[12];
                for (int i = 0; i < charArray.Length; i++) charArray[i] = Convert.ToChar(Rnd.Next(Min, Max));
                return new HelloWorldChromosome(new string(charArray));
            }

        }

        class HelloWorldChromosome : IChromosome
        {
            private const int _mMin = 0;
            private const int _mMax = 255;
            private static readonly Random _mRnd = new Random((int) DateTime.Now.Ticks);

            public double Cost { get; private set; }

            public string Genes
            {
                get { return _mGenes; }
                private set
                {
                    _mGenes = value;
                    Cost = CalculateCost();
                }
            }

            private const string _mTarget = "Hello World!";
            private string _mGenes;

            public HelloWorldChromosome(string genes)
            {
                Genes = genes;
            }

            private double CalculateCost()
            {
                var cost = 0.0;
                var idx = 0;

                foreach (var gene in Genes)
                {
                    cost += Math.Abs(gene - _mTarget[idx]);
                    idx++;
                }

                return cost;
            }

            public IChromosome Mate(IChromosome partner)
            {
                var validPartner = partner as HelloWorldChromosome;
                if (validPartner == null) throw new ArgumentException("Invalid partner.");

                var pivot = _mRnd.Next(0, 12);
                return (_mRnd.NextDouble() > 0.5)
                    ? new HelloWorldChromosome(Genes.Substring(0, pivot) + validPartner.Genes.Substring(pivot))
                    : new HelloWorldChromosome(validPartner.Genes.Substring(0, pivot) + Genes.Substring(pivot));
            }

            public void UpdateCost(object costCalc)
            {
                CalculateCost();
            }

            public void Mutate()
            {
                var destiny = _mRnd.NextDouble();
                if (destiny > 0.25) return;

                var mutation = Genes.ToCharArray();
                var idx = _mRnd.Next(0, 12);
                // TryMutate.
                if (destiny > 0.05)
                {
                    mutation[idx] = Convert.ToChar(Math.Max(_mMin, (mutation[idx] + _mRnd.Next(-1, 1))%_mMax));
                }
                // Reseed.                    
                else
                {
                    mutation[idx] = Convert.ToChar(_mRnd.Next(_mMin, _mMax));
                }

                Genes = new string(mutation);
            }

            public ISolution Clone()
            {
                throw new NotImplementedException();
            }
        }

    }
}
