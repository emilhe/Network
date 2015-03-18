using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class SimplexOptimizer<T> where T : IVectorSolution<T>
    {

        public int Generation { get; private set; }

        private readonly ICostCalculator<T> _mCostCalculator;
        private readonly ISimplexOptimizationStrategy<T> _mStrat;

        public double Beta { get; set; }
        public double Delta { get; set; }
        public double Alpha { get; set; }
        public double Gamma { get; set; }

        public SimplexOptimizer(ISimplexOptimizationStrategy<T> optimizationStrategy, ICostCalculator<T> costCalculator)
        {
            _mStrat = optimizationStrategy;
            _mCostCalculator = costCalculator;

            Beta = 0.5;
            Delta = 0.5;
            Alpha = 1.0;
            Gamma = 2.0;
        }

        public T Optimize(T[] simplex)
        {
            simplex = OrderPopulation(simplex);
            var n = simplex.Length;
            var h = simplex[n - 1];
            var s = simplex[n - 2];
            var l = simplex[0];

            while (!_mStrat.TerminationCondition(simplex, _mCostCalculator.Evaluations))
            {
                var c = _mStrat.Centroid(simplex.Take(n - 1).ToArray());
                // Reflection.
                var xr = c.Add(c.Delta(h), Alpha);
                UpdateCost(xr);
                if (l.Cost <= xr.Cost && xr.Cost < s.Cost)
                {
                    simplex[n - 1] = xr;
                }
                // Expansion.
                else if (xr.Cost < l.Cost)
                {
                    var xe = c.Add(xr.Delta(c), Gamma);
                    UpdateCost(xe);
                    if (xe.Cost < xr.Cost) simplex[n - 1] = xe;
                    else simplex[n - 1] = xr;
                }
                // Contraction.
                else if (xr.Cost >= s.Cost)
                {
                    var best = (xr.Cost < h.Cost) ? xr : h;
                    var xc = c.Add(best.Delta(c), Beta);
                    UpdateCost(xc);
                    if (xc.Cost < best.Cost) simplex[n - 1] = xc;
                    else
                    {
                        // Shrink
                        for (int i = 0; i < n; i++)
                        {
                            simplex[i] = l.Add(simplex[i].Delta(l), Delta);
                        }
                    }
                }
                // Order according to cost.
                simplex = OrderPopulation(simplex);
                // DEBUG INFO.
                if (l.Cost > simplex[0].Cost)
                {
                    Console.WriteLine("Cost is {0} at generation {1}", simplex[0].Cost, Generation);
                }
                h = simplex[n - 1];
                s = simplex[n - 2];
                l = simplex[0];
                Generation++;
            }

            return simplex[0];
        }

        private void UpdateCost(T vertex)
        {
            if (!vertex.InvalidCost) return;

            var start = DateTime.Now;
            _mCostCalculator.UpdateCost(new List<T>{vertex});
            var end = DateTime.Now.Subtract(start).TotalSeconds;
            //Console.WriteLine("Evaluation took {0} seconds.", end);
        }

        private T[] OrderPopulation(T[] unorderedPopulation)
        {
            var start = DateTime.Now;
            _mCostCalculator.UpdateCost(unorderedPopulation.Where(item => item != null && item.InvalidCost).ToList());
            var result = unorderedPopulation.OrderBy(item => item.Cost).ToArray();
            var end = DateTime.Now.Subtract(start).TotalSeconds;
            //Console.WriteLine("Evaluation took {0} seconds.", end);

            return result;
        }

    }
}
