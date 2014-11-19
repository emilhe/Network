using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface IChromosome
    {
        // Properties.
        double Cost { get; }

        // Actions.
        IChromosome[] Mate(IChromosome partner);
        void Mutate();

        // Should be static?
        IChromosome Spawn();
    }
}
 