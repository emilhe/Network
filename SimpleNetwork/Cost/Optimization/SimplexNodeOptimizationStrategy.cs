using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;

namespace BusinessLogic.Cost.Optimization
{
    public class SimplexNodeOptimizationStrategy : ISimplexOptimizationStrategy<NodeVec>
    {

        public bool TerminationCondition(NodeVec[] nests, int evaluations)
        {
            return (evaluations > 25000);
        }

        public NodeVec Centroid(NodeVec[] solutions)
        {
            var n = solutions[0].Length;
            var vector = new double[n];
            for (int i = 0; i < solutions[0].Length; i++)
            {
                vector[i] = solutions.Select(item => item[i]).Average();
            }
            return new NodeVec(vector);
        }

    }

}
