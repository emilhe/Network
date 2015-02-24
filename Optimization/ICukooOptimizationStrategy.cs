using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface ICukooOptimizationStrategy<T> where T : ISolution
    {

        // DE rate \in {0:1} where 1 means all population goes through DE in each iteration.
        double DifferentialEvolutionRate { get; }
        // LF rate \in {0:1} where 1 means all population goes through LF in each iteration.
        double LevyFlightRate { get; }
        // CO rate \in {0:1} where 1 means all population goes through CO in each iteration.
        double CrossOverRate { get; }

        // DE aggressiveness \in {0:1} where 0 means solutions are ONLY accepted when the new is better and 0 means ALWAYS accepted.
        double DifferentialEvolutionAggressiveness { get; }
        // LV aggressiveness \in {0:1} where 0 means solutions are ONLY accepted when the new is better and 0 means ALWAYS accepted.
        double LevyFlightAggressiveness { get; }

        bool TerminationCondition(T[] nests, int evaluations);

        T DifferentialEvolution(T nest, T nest1, T nest2);
        T LevyFlight(T nest, T bestNests);
        T CrossOver(T badNest, T goodNest);

    }
}
