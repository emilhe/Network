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
            var deN = (int) Math.Round(nests.Length*(_mStrat.DifferentialEvolutionRate));
            var lvN = (int) Math.Round(nests.Length*(_mStrat.LevyFlightRate));
            var coN = (int) Math.Round(nests.Length*(_mStrat.CrossOverRate));
            var deForceN = (int) Math.Round(nests.Length*(1 - _mStrat.DifferentialEvolutionAggressiveness));
            var lvForceN = (int) Math.Round(nests.Length*(1 - _mStrat.LevyFlightAggressiveness));

            while (!_mStrat.TerminationCondition(nests, _mCostCalculator.Evaluations))
            {
                Generation++;

                // Generate new trail eggs by cross over.            
                for (int i = 0; i < coN; i++)
                {
                    var j = (int)Math.Round((coN - 1) * _mRnd.NextDouble());
                    // Cross over not possible.
                    if (i == j) trailEggs[i] = nests[i];
                    // Cross over possible; do it.
                    else trailEggs[i] = _mStrat.CrossOver(nests[Math.Max(i, j)], nests[Math.Min(i, j)]);
                }
                UpdateNests(nests, trailEggs, n);

                // Generate new trail eggs by Lévy flight.
                for (int i = 0; i < lvN; i++)
                {
                    trailEggs[i] = _mStrat.LevyFlight(nests[i], bestNest);
                }
                UpdateNests(nests, trailEggs, lvForceN);

                // Generate new trails eggs by differential evolution.
                for (int i = (n-deN); i < n; i++)
                {
                    trailEggs[i] = _mStrat.DifferentialEvolution(nests, i);
                }
                nests = UpdateNests(nests, trailEggs, deForceN);
                bestNest = nests[0];

                // Debug info.
                if (CacheOnDisk) bestNest.ToJsonFile(@"C:\proto\bestConfig.txt");
                if (PrintToConsole) Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
            }

            return bestNest;
        }

        private T[] UpdateNests(T[] nests, T[] trailEggs, int forceIdx)
        {
            return UpdateNests(nests, trailEggs, i => (i >= forceIdx) || nests[i].Cost > trailEggs[i].Cost);
        }

        private T[] UpdateNests(T[] nests, T[] trailEggs, Func<int, bool> condition)
        {
            var n = trailEggs.Length;
            EvalPopulation(trailEggs);
            for (int i = 0; i < n; i++)
            {
                if (trailEggs[i] == null) continue;
                if (condition(i)) nests[i] = trailEggs[i];
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
