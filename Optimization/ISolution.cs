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

        // Cost function.
        bool InvalidCost { get; }

        // Update the cost. MUST be done externally.
        void UpdateCost(Func<ISolution, double> eval);

    }
}
