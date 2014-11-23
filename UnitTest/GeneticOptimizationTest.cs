using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Optimization;

namespace UnitTest
{
    [TestFixture]
    class GeneticOptimizationTest
    {

        [Test]
        public void HelloWorld()
        {
            // ReBirth population.
            var n = 100;
            var strategy = new GeneticStringOptimizationStrategy();
            var population = new IChromosome[n];
            for (int i = 0; i < population.Length; i++) population[i] = GeneticStringOptimizationStrategy.Spawn();
            // Find optimum.
            var optimizer = new GeneticOptimizer<HelloWorldChromosome>(strategy);
            var optimum = optimizer.Optimize(population);
            Assert.AreEqual("Hello World!", optimum.Genes);
        }

        class GeneticStringOptimizationStrategy : IGeneticOptimizationStrategy<HelloWorldChromosome>
        {
            private const int _mMin = 0;
            private const int _mMax = 255;
            private static readonly Random _mRnd = new Random((int)DateTime.Now.Ticks);

            private const int _mChildCount = 75;            
            private const int _mEliteCount = 15;
            private const int _mEliteMixCount = 1;

            public bool TerminationCondition(IChromosome[] chromosomes)
            {
                return Math.Abs(chromosomes[0].Cost) < 1e-10;
            }

            public void Select(IChromosome[] chromosomes)
            {
                // Kill bad candidates.
                for (int i = _mEliteCount; i < _mEliteCount + _mEliteMixCount; i++) chromosomes[i] = Spawn();
            }

            public void Mate(IChromosome[] chromosomes)
            {
                var offspring = new IChromosome[chromosomes.Length*2];

                // Find children.
                for (int i = 0; i < _mChildCount; i++)
                {
                    var father = chromosomes[(int) (_mRnd.NextDouble()*(_mEliteCount+_mEliteMixCount - 1))];
                    var mother = chromosomes[(int) (_mRnd.NextDouble()*(_mEliteCount+_mEliteMixCount - 1))];
                    offspring[i] = father.Mate(mother);
                }

                // Fill in children + randoms.
                for (int i = 0; i < _mChildCount; i++)
                {
                    chromosomes[i] = (i < offspring.Length) ? offspring[i] : Spawn();
                }
            }

            public void Mutate(IChromosome[] chromosomes)
            {
                foreach (var chromosome in chromosomes) chromosome.Mutate();
            }

            public static HelloWorldChromosome Spawn()
            {
                var charArray = new char[12];
                for (int i = 0; i < charArray.Length; i++) charArray[i] = Convert.ToChar(_mRnd.Next(_mMin, _mMax));
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

            public void Mutate()
            {
                var destiny = _mRnd.NextDouble();
                if (destiny > 0.25) return;

                var mutation = Genes.ToCharArray();
                var idx = _mRnd.Next(0, 12);
                // Mutate.
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

        }

    }
}
