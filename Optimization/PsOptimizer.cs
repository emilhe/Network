using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Optimization
{
    public class PsOptimizer<T> where T : IParticle
    {

        public int Generation { get; private set; }

        private readonly ICostCalculator<T> _mCostCalculator;
        private readonly IPsOptimizationStrategy<T> _mStrat;

        public PsOptimizer(IPsOptimizationStrategy<T> optimizationStrategy, ICostCalculator<T> costCalculator)
        {
            _mStrat = optimizationStrategy;
            _mCostCalculator = costCalculator;
        }

        public ISolution Optimize(T[] population)
        {
            Generation = 0;

            while (!_mStrat.TerminationCondition(population))
            {
                // Alter solutions.
                _mStrat.UpdateVelocities(population);
                _mStrat.UpdatePositions(population);
                // Update costs.
                var start = DateTime.Now;
                _mCostCalculator.UpdateCost(population);
                var end = DateTime.Now.Subtract(start).TotalSeconds;
                Console.WriteLine("Evaluation took {0} seconds.", end);
                // Update 
                _mStrat.UpdateBestPositions(population);
                // Debug info.
                _mStrat.BestSolution.ToJsonFile(@"C:\proto\bestConfig.txt");
                Generation++;
                Console.WriteLine("Generation {0}, Cost = {1}", Generation, _mStrat.BestSolution);
            }

            return _mStrat.BestSolution;
        }

    }
}
