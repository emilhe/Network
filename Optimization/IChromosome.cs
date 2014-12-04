using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface IChromosome : ICost
    {

        // Mate another chromosome.
        IChromosome Mate(IChromosome partner);
        
        // Mutate according to "species specifications".
        void Mutate();

    }
}
 