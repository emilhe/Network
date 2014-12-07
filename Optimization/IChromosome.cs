using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface IChromosome : ISolution
    {

        // Mate another chromosome.
        IChromosome Mate(IChromosome partner);

        void UpdateCost(object costCalc);

    }
}
 