using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Utils;

namespace BusinessLogic.Interfaces
{
    interface IOptimizer
    {

        Action OnSolveCompleted { set; }

        int N { get; }
        EdgeCollection Edges { get; }
        ModelWrapper3 Wrap { get; }
        double[] Deltas { get; }

        void SetEdges(EdgeCollection edges);
        void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits);
        void Solve();

    }
}
