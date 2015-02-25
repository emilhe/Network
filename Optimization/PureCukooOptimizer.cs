using System;
using System.Linq;
using Utils;

namespace Optimization
{
    namespace OldOptimization
    {
        /// <summary>
        /// Designed by myself, inspired by MCS.
        /// </summary>
        public class PureCukooOptimizer<T> where T : ISolution
        {

            public int Generation { get; private set; }
            public bool PrintToConsole { get; set; }
            public bool CacheOnDisk { get; set; }

            public double AbandonRate { get; set; }

            private readonly Random _mRnd = new Random((int)DateTime.Now.Ticks);
            private readonly ICostCalculator<T> _mCostCalculator;
            private readonly ICukooOptimizationStrategy<T> _mStrat;

            public PureCukooOptimizer(ICukooOptimizationStrategy<T> optimizationStrategy, ICostCalculator<T> costCalculator)
            {
                _mStrat = optimizationStrategy;
                _mCostCalculator = costCalculator;

                PrintToConsole = true;
                CacheOnDisk = true;

                // Default value, good for most purposes.
                AbandonRate = 0.75;
            }

            public T Optimize(T[] nests)
            {
                // Eval generation 0.
                Generation = 0;
                EvalPopulation(nests);
                nests = nests.OrderBy(item => item.Cost).ToArray();
                T bestNest = nests[0];
                if (PrintToConsole) Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
                // Initialize data structures.
                var n = nests.Length;
                var eliteN = (int)Math.Round(nests.Length * (1 - AbandonRate));
                var trailEggs = new T[eliteN];

                while (!_mStrat.TerminationCondition(nests, _mCostCalculator.Evaluations))
                {
                    Generation++;
                    // Abandon bad nests.
                    for (int i = eliteN; i < n; i++)
                    {
                        nests[i] = _mStrat.LevyFlight(nests[i], bestNest);
                    }
                    // Generate trail eggs using good nests.
                    for (int i = 0; i < eliteN; i++)
                    {
                        trailEggs[i] = _mStrat.LevyFlight(nests[i], bestNest);
                    }
                    // Eval the new solutions.
                    EvalPopulation(trailEggs);
                    EvalPopulation(nests);
                    // Drop new eggs randomly.
                    for (int i = 0; i < eliteN; i++)
                    {
                        var j = (int)Math.Round((n - 1) * _mRnd.NextDouble());
                        if (nests[j].Cost > trailEggs[i].Cost) nests[j] = trailEggs[i];
                    }
                    // Update best nest.
                    nests = nests.OrderBy(item => item.Cost).ToArray();
                    bestNest = nests[0];
                    // Debug info.
                    if (CacheOnDisk) bestNest.ToJsonFile(@"C:\proto\bestConfig.txt");
                    if (PrintToConsole) Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
                }

                return bestNest;
            }

            private void EvalPopulation(T[] unorderedPopulation)
            {
                var start = DateTime.Now;
                _mCostCalculator.UpdateCost(unorderedPopulation.Where(item => item.InvalidCost).ToList());
                var end = DateTime.Now.Subtract(start).TotalSeconds;
                if(PrintToConsole) Console.WriteLine("Evaluation took {0} seconds.", end);
            }

        }
    }
}
