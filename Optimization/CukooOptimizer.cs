 using System;
using System.CodeDom;
 using System.Collections.Generic;
 using System.Linq;
using Utils;

namespace Optimization
{
    /// <summary>
    /// Adjusted by myself, inspired strongly by CS.
    /// </summary>
    public class CukooOptimizer<T> where T : ISolution
    {

        public int Generation { get; private set; }
        public bool PrintToConsole { get; set; }
        public bool CacheOnDisk { get; set; }
        public Dictionary<int, double> Steps { get; set; } 

        public double LevyFlightRate { get; set; }
        public double DifferentialEvolutionRate { get; set; }

        private readonly ICostCalculator<T> _mCostCalculator;
        private readonly ICukooOptimizationStrategy<T> _mStrat;

        public CukooOptimizer(ICukooOptimizationStrategy<T> optimizationStrategy, ICostCalculator<T> costCalculator)
        {
            _mStrat = optimizationStrategy;
            _mCostCalculator = costCalculator;

            PrintToConsole = true;
            CacheOnDisk = true;
            Steps = new Dictionary<int, double>();

            // Default values, good for most purposes.
            LevyFlightRate = 1;
            DifferentialEvolutionRate = 1;
        }

        public T Optimize(T[] nests)
        {
            Steps.Clear();
            // Eval generation 0.
            Generation = 0;
            EvalPopulation(nests);
            nests = nests.OrderBy(item => item.Cost).ToArray();
            T bestNest = nests[0];
            Steps.Add(_mCostCalculator.Evaluations, bestNest.Cost);
            if(PrintToConsole) Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
            // New trail eggs are generated in the trails eggs vector.
            var n = nests.Length;
            var trailEggs = new T[n];
            var deN = (int) Math.Round(nests.Length*(DifferentialEvolutionRate));
            var lvN = (int) Math.Round(nests.Length*(LevyFlightRate));

            while (!_mStrat.TerminationCondition(nests, _mCostCalculator.Evaluations))
            {
                Generation++;
                // Generate new trail eggs by Lévy flight.
                for (int i = 0; i < lvN; i++)
                {
                    trailEggs[i] = _mStrat.LevyFlight(nests[i], bestNest);
                }
                UpdateNests(nests, trailEggs);
                // Generate new trails eggs by differential evolution.
                for (int i = (n-deN); i < n; i++)
                {
                    trailEggs[i] = _mStrat.DifferentialEvolution(nests, i);
                }
                nests = UpdateNests(nests, trailEggs);
                bestNest = nests[0];
                Steps.Add(_mCostCalculator.Evaluations,bestNest.Cost);
                // Debug info.
                if (CacheOnDisk) bestNest.ToJsonFile(@"C:\proto\bestConfig.txt");
                if (PrintToConsole) Console.WriteLine("Generation {0}, Cost = {1}", Generation, bestNest.Cost);
            }

            return bestNest;
        }

        private T[] UpdateNests(T[] nests, T[] trailEggs)
        {
            EvalPopulation(trailEggs);
            for (int i = 0; i < trailEggs.Length; i++)
            {
                if (trailEggs[i] == null) continue;
                if (nests[i].Cost > trailEggs[i].Cost) nests[i] = trailEggs[i];
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
