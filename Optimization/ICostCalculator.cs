using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface ICostCalculator<T> where T : ISolution
    {

        int Evaluations { get; }
        void UpdateCost(IList<T> solutions);

    }
}
