using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface ISimplexOptimizationStrategy<T>
    {

        bool TerminationCondition(T[] nests, int evaluations);

        T Centroid(T[] solutions);

    }
}
