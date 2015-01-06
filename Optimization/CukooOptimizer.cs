using System;
using System.Linq;
using Utils;

namespace Optimization
{
    public class CukooOptimizer<T> where T : ISolution
    {

        public int Generation { get; private set; }

        private readonly ICostCalculator<T> _mCostCalculator;
        private readonly ICukooOptimizationStrategy<T> _mStrat;

        public CukooOptimizer(ICukooOptimizationStrategy<T> optimizationStrategy, ICostCalculator<T> costCalculator)
        {
            _mStrat = optimizationStrategy;
            _mCostCalculator = costCalculator;
        }

        public T Optimize(T[] nests)
        {
            Generation = 0;
            nests = OrderPopulation(nests);
            var bestNest = nests[0];
            Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);

            while (!_mStrat.TerminationCondition(nests))
            {
                // Generate new solutions.
                var newNests = _mStrat.GetNewNests(nests, bestNest);
                EvalPopulation(newNests);
                // Select the best solutions.
                for (int i = 0; i < nests.Length; i++)
                {
                    if (newNests[i].Cost >= nests[i].Cost) continue;
                    nests[i] = newNests[i];
                }
                nests = nests.OrderBy(item => item.Cost).ToArray();
                // Abandon the bad nests.
                _mStrat.AbandonNests(nests);
                nests = OrderPopulation(nests);
                // Update best nest.
                bestNest = nests[0];
                // Debug info.
                bestNest.ToJsonFile(@"C:\proto\bestConfig.txt");
                Generation++;
                Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
            }

            return bestNest;
        }

        private T[] OrderPopulation(T[] unorderedPopulation)
        {
            EvalPopulation(unorderedPopulation);
            return unorderedPopulation.OrderBy(item => item.Cost).ToArray();
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
