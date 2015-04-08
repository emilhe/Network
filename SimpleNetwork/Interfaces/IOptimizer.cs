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

        double[] NodeOptima { get; }
        double[,] Flows { get; }
        List<double[]> StorageOptima { get; }

        void SetNodes(double[] nodes, List<double[]> lowLimits, List<double[]> highLimits);
        void Solve();

    }
}
