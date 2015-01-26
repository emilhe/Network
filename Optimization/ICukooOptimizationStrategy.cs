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

        double AbandonRate { get; }

        bool TerminationCondition(T[] nests, int evaluations);
        T LevyFlight(T nest, T bestNest, double scaling = 1);
        T CrossOver(T badNest, T goodNest);
    }
}
