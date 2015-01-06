using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface ICukooOptimizationStrategy<T> where T : ISolution
    {

        bool TerminationCondition(T[] nests);
        T[] GetNewNests(T[] nests, T bestNest);
        void AbandonNests(T[] nests);

    }
}
