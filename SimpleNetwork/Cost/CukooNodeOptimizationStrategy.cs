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

        private const int StagnationLimit = 10;

        private const double AbandonRate = 0.10;

        private double _mLastCost = double.MaxValue;
        private int _mStagnationCount;

        // Terminate when the solution becomes stable.
        public bool TerminationCondition(NodeChromosome[] nests)
        {
            if (Math.Abs(nests[0].Cost - _mLastCost) > 1e-3)
            {
                _mLastCost = nests[0].Cost;
                _mStagnationCount = 0;
                return false;
            }

            _mLastCost = nests[0].Cost;
            _mStagnationCount++;
            return _mStagnationCount == StagnationLimit;
        }

        // Create new nests using Levy flights
        public NodeChromosome[] GetNewNests(NodeChromosome[] nests, NodeChromosome best)
        {
            var newNests = new NodeChromosome[nests.Length];
            for (int i = 0; i < nests.Length; i++)
            {
                newNests[i] = GenePool.LevyFlight(nests[i], best);
            }

            return newNests;
        }

        // Abandon the worst nests = spawn new solution.
        public void AbandonNests(NodeChromosome[] nests)
        {
            // TODO: Is this correct?
            var n = nests.Length;
            var abandonCount = Math.Ceiling(n*AbandonRate);
            for (int i = 0; i < abandonCount; i++)
            {
                nests[n - i - 1] = GenePool.SpawnChromosome();
            }
        }
    }
}
