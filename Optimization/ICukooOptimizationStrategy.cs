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

        double DifferentialEvolutionRate { get; }
        double CrossOverRate { get; }
        double LevyRate { get; }

        bool TerminationCondition(T[] nests, int evaluations);

        T DifferentialEvolution(T nest, T nest1, T nest2);
        T LevyFlight(T nest, T bestNests);
        T CrossOver(T badNest, T goodNest);

    }
}
