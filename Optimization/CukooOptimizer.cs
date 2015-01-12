using System;
using System.CodeDom;
using System.Linq;
using Utils;

namespace Optimization
{
    public class CukooOptimizer<T> where T : ISolution
    {

        public int Generation { get; private set; }

        private readonly Random _mRnd = new Random((int)DateTime.Now.Ticks);
        private readonly ICostCalculator<T> _mCostCalculator;
        private readonly ICukooOptimizationStrategy<T> _mStrat;

        public CukooOptimizer(ICukooOptimizationStrategy<T> optimizationStrategy, ICostCalculator<T> costCalculator)
        {
            _mStrat = optimizationStrategy;
            _mCostCalculator = costCalculator;
        }

        public T Optimize(T[] nests)
        {
            // Eval generation 0.
            Generation = 0;
            EvalPopulation(nests);
            nests = nests.OrderBy(item => item.Cost).ToArray();
            T bestNest = nests[0];
            Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
            // Initialize data structures.
            var n = nests.Length;
            var newEggs = new T[n];

            while (!_mStrat.TerminationCondition(nests))
            {
                Generation++;
                // Generate new eggs.
                for (int i = 0; i < n; i++)
                {
                    newEggs[i] = _mStrat.LevyFlight(nests[i], bestNest);
                }
                EvalPopulation(newEggs);
                // Drop each egg in a random nest.
                for (int i = 0; i < n; i++)
                {
                    var j = (int)Math.Round((n - 1) * _mRnd.NextDouble());
                    if (nests[j].Cost > newEggs[i].Cost) nests[j] = newEggs[i];
                }
                // Update best nest.
                nests = nests.OrderBy(item => item.Cost).ToArray();
                bestNest = nests[0];
                // Abandon bad nests.
                for (int i = 0; i < n*_mStrat.AbandonRate; i++)
                {
                    var j = (int)Math.Round((n - 1) * _mRnd.NextDouble());
                    nests[j] = _mStrat.LevyFlight(nests[j], bestNest);
                }
                EvalPopulation(nests);
                // Order & update best nest.
                nests = nests.OrderBy(item => item.Cost).ToArray();
                bestNest = nests[0];
                // Debug info.
                bestNest.ToJsonFile(@"C:\proto\bestConfig.txt");
                Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
            }

            return bestNest;
        }

        private void EvalPopulation(T[] unorderedPopulation)
        {
            var start = DateTime.Now;
            _mCostCalculator.UpdateCost(unorderedPopulation.Where(item => item.InvalidCost));
            var end = DateTime.Now.Subtract(start).TotalSeconds;
            Console.WriteLine("Evaluation took {0} seconds.", end);
        }

    }
}
