 using System;
using System.CodeDom;
using System.Linq;
using Utils;

namespace Optimization
{
    public class CukooOptimizer<T> where T : ISolution
    {

        public int Generation { get; private set; }
        public bool PrintToConsole { get; set; }
        public bool CacheOnDisk { get; set; }

        private readonly Random _mRnd = new Random((int)DateTime.Now.Ticks);
        private readonly ICostCalculator<T> _mCostCalculator;
        private readonly ICukooOptimizationStrategy<T> _mStrat;

        public CukooOptimizer(ICukooOptimizationStrategy<T> optimizationStrategy, ICostCalculator<T> costCalculator)
        {
            _mStrat = optimizationStrategy;
            _mCostCalculator = costCalculator;

            PrintToConsole = true;
            CacheOnDisk = true;
        }

        public T Optimize(T[] nests)
        {
            // Eval generation 0.
            Generation = 0;
            EvalPopulation(nests);
            nests = nests.OrderBy(item => item.Cost).ToArray();
            T bestNest = nests[0];
            if(PrintToConsole) Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
            // New trail eggs are generated in the trails eggs vector.
            var n = nests.Length;
            var trailEggs = new T[n];
            var lvN = (int)Math.Round(nests.Length * (_mStrat.LevyRate));
            var coN = (int) Math.Round(nests.Length*(_mStrat.CrossOverRate));
            var deN = (int) Math.Round(nests.Length * (1-_mStrat.DifferentialEvolutionRate));      

            while (!_mStrat.TerminationCondition(nests, _mCostCalculator.Evaluations))
            {
                Generation++;
                // Generate new trail eggs by cross over.            
                for (int i = 0; i < coN; i++)
                {
                    var j = (int)Math.Round((coN - 1) * _mRnd.NextDouble());
                    // Cross over not possible; do levy flight.
                    if (i == j)
                    {
                        trailEggs[i] = _mStrat.LevyFlight(nests[i], bestNest);
                        continue;
                    }
                    // Cross over possible; do it.
                    trailEggs[i] = _mStrat.CrossOver(nests[Math.Max(i, j)], nests[Math.Min(i, j)]);
                }
                UpdateNests(nests, trailEggs);
                // Generate new trail eggs by Lévy flight.
                for (int i = 0; i < lvN; i++)
                {
                    trailEggs[i] = _mStrat.LevyFlight(nests[i], bestNest);
                }
                UpdateNests(nests, trailEggs);
                // Generate new trails eggs by differential evolution.
                //var rndOrder1 = new int[nests.Length].Linspace().Shuffle(_mRnd).ToArray();
                //var rndOrder2 = new int[nests.Length].Linspace().Shuffle(_mRnd).ToArray();
                for (int i = deN; i < n; i++)
                {
                    trailEggs[i] = _mStrat.DifferentialEvolution(nests[i], nests);
                }
                nests = UpdateNests(nests, trailEggs);
                bestNest = nests[0];
                // Debug info.
                if (CacheOnDisk) bestNest.ToJsonFile(@"C:\proto\bestConfig.txt");
                if (PrintToConsole) Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
            }

            return bestNest;
        }

        private T[] UpdateNests(T[] nests, T[] trailEggs)
        {
            return UpdateNests(nests, trailEggs, (nest, newNest) => nest.Cost > newNest.Cost);
        }

        private T[] UpdateNests(T[] nests, T[] trailEggs, Func<T, T, bool> condition)
        {
            var n = trailEggs.Length;
            EvalPopulation(trailEggs);
            for (int i = 0; i < n; i++)
            {
                if (trailEggs[i] == null) continue;
                if (condition(nests[i], trailEggs[i])) nests[i] = trailEggs[i];
            }
            return nests.OrderBy(item => item.Cost).ToArray();
        }

        private void EvalPopulation(T[] unorderedPopulation)
        {
            var start = DateTime.Now;
            _mCostCalculator.UpdateCost(unorderedPopulation.Where(item => item != null && item.InvalidCost).ToList());
            var end = DateTime.Now.Subtract(start).TotalSeconds;
            if (PrintToConsole) Console.WriteLine("Evaluation took {0} seconds.", end);
        }

    }
}
