using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface ISolution
    {

        // Cost function.
        double Cost { get; }

        // Mutate according to "species specifications".
        void Mutate();

        // Mutate according to "species specifications".
        ISolution Clone();

    }
}
