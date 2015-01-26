using System;
using System.Linq;
using Optimization;

namespace BusinessLogic.Cost.Optimization
{
    public class CukooNodeOptimizationStrategy : ICukooOptimizationStrategy<NodeChromosome>
    {

        public double AbandonRate { get { return 0.75; } }
        private const int StagnationLimit = 10;

        private double _mLastCost = double.MaxValue;
        private int _mStagnationCount;

        public bool TerminationCondition(NodeChromosome[] nests, int evaluations)
        {
            return (evaluations > 2500);

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

        public NodeChromosome CrossOver(NodeChromosome badNest, NodeChromosome goodNest)
        {
            var genes = new NodeGenes();
            var phi = (1 + Math.Sqrt(5))/2.0;
            foreach (var key in badNest.Genes.Keys.ToArray())
            {
                genes[key].Alpha = goodNest.Genes[key].Alpha + (badNest.Genes[key].Alpha - goodNest.Genes[key].Alpha)/phi;
                genes[key].Gamma = goodNest.Genes[key].Gamma + (badNest.Genes[key].Gamma - goodNest.Genes[key].Gamma)/phi;
            }
            var result = new NodeChromosome(genes);
            if(!GenePool.Renormalize(result)) throw new Exception("Unable to renormalize");
            return result;
        }
    }
}
