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
    class OptimizationTest
    {

        [Test]
        public void Genetic()
        {
            // ReBirth population.
            var n = 100;
            var strategy = new GeneticStringOptimizationStrategy();
            var population = new HelloWorldChromosome[n];
            for (int i = 0; i < population.Length; i++) population[i] = GeneticStringOptimizationStrategy.Spawn();
            // Find optimum.
            var optimizer = new GeneticOptimizer<HelloWorldChromosome>(strategy, new HelloWorldCostCalculator());
            var optimum = optimizer.Optimize(population);
            Assert.AreEqual("Hello World!", optimum.Genes);
        }

        //[Test]
        //public void Simplex()
        //{
        //    // ReBirth population.
        //    var strategy = new SimplexStringOptimizationStrategy();
        //    var simplex = new HelloWorldChromosome[13];
        //    // Construct simplex (centered around the center value; 64).
        //    var mother = GeneticStringOptimizationStrategy.Spawn();
        //    for (int i = 0; i < simplex.Length; i++)
        //    {
        //        var charArray = mother.Genes.ToArray();
        //        if (i < charArray.Length) charArray[i] = Convert.ToChar(charArray[i] + 5);
        //        simplex[i] = new HelloWorldChromosome(new string(charArray));
        //    }
        //    // Find optimum.
        //    var optimizer = new SimplexOptimizer<HelloWorldChromosome>(strategy, new HelloWorldCostCalculator());
        //    var optimum = optimizer.Optimize(simplex);
        //    Assert.AreEqual("Hello World!", optimum.Genes);
        //}

        //#region Numbered simplex

        //[Test]
        //public void NumSimplex()
        //{
        //    // ReBirth population.
        //    var strategy = new SimplexNumOptimizationStrategy();
        //    // Construct simplex (centered around the center value; 64).
        //    var center = new[] {100.0, 100, 100, 100, 100, 100, 100, 100, 1000, 10, 100, 200};
        //    var simplex = new NumSolution[center.Length+1];
        //    for (int i = 0; i < simplex.Length; i++)
        //    {
        //        var vertex = center;
        //        if (i < center.Length) vertex[i] = vertex[i] + 5 * (i + 1);
        //        simplex[i] = new NumSolution(vertex);
        //    }
        //    // Find optimum.
        //    var optimizer = new SimplexOptimizer<NumSolution>(strategy, new NumCostCalculator());
        //    var optimum = optimizer.Optimize(simplex);
        //    Assert.AreEqual(0, optimum.Cost, 1e-5);
        //}

        //class SimplexNumOptimizationStrategy : ISimplexOptimizationStrategy<NumSolution>
        //{
        //    public bool TerminationCondition(NumSolution[] nests, int evaluations)
        //    {
        //        return nests[0].Cost < 1e-5;
        //    }

        //    public NumSolution Centroid(NumSolution[] solutions)
        //    {
        //        var n = solutions[0].Genes.Length;
        //        var genes = new double[n];
        //        for (int i = 0; i < n; i++)
        //        {
        //            genes[i] = solutions.Select(item => item.Genes[i]).Average();
        //        }
        //        return new NumSolution(genes);
        //    }
        //}

        //class NumSolution : IVectorSolution
        //{

        //    private bool _mInvalidCost = true;
        //    private double _mCost = 0;

        //    public double[] Genes { get; private set; }

        //    public NumSolution(double[] genes)
        //    {
        //        Genes = genes;
        //    }

        //    public double Cost
        //    {
        //        get { return _mCost;}
        //    }

        //    public bool InvalidCost
        //    {
        //        get { return _mInvalidCost;}
        //    }

        //    public void UpdateCost(Func<ISolution, double> eval)
        //    {
        //        _mCost = eval(this);
        //        _mInvalidCost = false;
        //    }

        //    public IVectorSolution Add(IVectorSolution partner, double weight)
        //    {
        //        var other = partner as NumSolution;
        //        if (other == null) throw new ArgumentException("Invalid partner.");

        //        var genes = new double[Genes.Length];
        //        for (int i = 0; i < Genes.Length; i++)
        //        {
        //            genes[i] = genes[i] + other.Genes[i] * weight;
        //        }
        //        return new NumSolution(genes);
        //    }

        //    public IVectorSolution Sub(IVectorSolution other)
        //    {
        //        return Add(other, -1);
        //    }
        //}

        //class NumCostCalculator : ICostCalculator<NumSolution>
        //{

        //    private int _mEvals = 0;

        //    public int Evaluations
        //    {
        //        get { return _mEvals; }
        //    }

        //    public void UpdateCost(IList<NumSolution> solutions)
        //    {
        //        _mEvals += solutions.Count;
        //        foreach (var solution in solutions)
        //        {
        //            solution.UpdateCost(CalculateCost);
        //        }
        //    }

        //    private double CalculateCost(ISolution partner)
        //    {
        //        var other = partner as NumSolution;
        //        if (other == null) throw new ArgumentException("Invalid partner.");

        //        return other.Genes.Select(item => item*item).Sum();
        //    }
        //}

        //#endregion

        //class SimplexStringOptimizationStrategy : ISimplexOptimizationStrategy<HelloWorldChromosome>
        //{

        //    public bool TerminationCondition(HelloWorldChromosome[] chromosomes, int evaluations)
        //    {
        //        if (Math.Abs(chromosomes[0].Cost) < 1e-10)
        //        {
        //            var hest = 2;
        //            return true;
        //        }
        //        return Math.Abs(chromosomes[0].Cost) < 1e-10;
        //    }

        //    public HelloWorldChromosome Centroid(HelloWorldChromosome[] solutions)
        //    {
        //        var charArray = new char[12];
        //        for (int i = 0; i < charArray.Length; i++)
        //        {
        //            charArray[i] = Convert.ToChar((int)Math.Round(solutions.Select(item => (int) item.Genes[i]).Average(item => item)));
        //        }
        //        return new HelloWorldChromosome(new string(charArray));
        //    }

        //}

        class GeneticStringOptimizationStrategy : IGeneticOptimizationStrategy<HelloWorldChromosome>
        {

            private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);
            
            private const int Min = 0;
            private const int Max = 255;

            private const int ChildCount = 75;            
            private const int EliteCount = 15;
            private const int EliteMixCount = 1;

            public bool TerminationCondition(HelloWorldChromosome[] chromosomes, int evaluations)
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

        class HelloWorldChromosome : IChromosome//, IVectorSolution
        {
            private const int _mMin = 0;
            private const int _mMax = 127;
            private static readonly Random _mRnd = new Random((int) DateTime.Now.Ticks);

            public double Cost { get; private set; }
            public bool InvalidCost { get; private set; }

            public string Genes
            {
                get { return _mGenes; }
                private set
                {
                    _mGenes = value;
                    InvalidCost = true;
                }
            }

            private string _mGenes;

            public HelloWorldChromosome(string genes)
            {
                Genes = genes;
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

            public void UpdateCost(Func<ISolution, double> eval)
            {
                Cost = eval(this);
                InvalidCost = false;
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

            //public IVectorSolution Add(IVectorSolution partner, double weight)
            //{
            //    var validPartner = partner as HelloWorldChromosome;
            //    if (validPartner == null) throw new ArgumentException("Invalid partner.");

            //    var charArray = new char[12];
            //    for (int i = 0; i < charArray.Length; i++)
            //    {
            //        var value = (int)Math.Round((int)Genes[i] + weight * validPartner.Genes[i]);
            //        if (value < _mMin) value = _mMin;
            //        if (value > _mMax) value = _mMax;
            //        charArray[i] = Convert.ToChar(value);
            //    }
            //    return new HelloWorldChromosome(new string(charArray));
            //}

            //public IVectorSolution Sub(IVectorSolution other)
            //{
            //    return Add(other, -1);
            //}
        }

        class HelloWorldCostCalculator : ICostCalculator<HelloWorldChromosome>
        {

            private const string _mTarget = "Hello World!";
            private int _mEvals = 0;

            public int Evaluations
            {
                get { return _mEvals; }
            }

            public void UpdateCost(IList<HelloWorldChromosome> solutions)
            {
                _mEvals += solutions.Count;
                foreach (var solution in solutions)
                {
                    solution.UpdateCost(CalculateCost);
                }
            }

            private double CalculateCost(ISolution solution)
            {
                var cost = 0.0;
                var idx = 0;

                foreach (var gene in (solution as HelloWorldChromosome).Genes)
                {
                    cost += Math.Abs(gene - _mTarget[idx]);
                    idx++;
                }

                return cost;
            }
        }

    }
}
