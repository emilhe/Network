using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;

namespace BusinessLogic.Cost
{
    public class CukooNodeOptimizationStrategy : ICukooOptimizationStrategy<NodeChromosome>
    {

        public double AbandonRate { get { return 0.25; } }
        private const int StagnationLimit = 10;

        private double _mLastCost = double.MaxValue;
        private int _mStagnationCount;

        // Terminate when the solution becomes stable.
        public bool TerminationCondition(NodeChromosome[] nests)
        {
            if (Math.Abs(nests[0].Cost - _mLastCost) > 1e-2)
            {
                _mLastCost = nests[0].Cost;
                _mStagnationCount = 0;
                return false;
            }

            _mLastCost = nests[0].Cost;
            _mStagnationCount++;
            return _mStagnationCount == StagnationLimit;
        }

        public NodeChromosome LevyFlight(NodeChromosome nest, NodeChromosome bestNest, double scaling = 1.0)
        {
            return GenePool.DoLevyFlight(nest, bestNest, scaling);   
        }

    }
}
