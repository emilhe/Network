using System;
using System.Linq;
using Optimization;

namespace BusinessLogic.Cost.Optimization
{
    public class CukooNodeOptimizationStrategy : ICukooOptimizationStrategy<NodeChromosome>
    {

        public double DifferentialEvolutionAggressiveness
        {
            get { return 0; }
        }

        public double LevyFlightAggressiveness
        {
            get { return 0; }
        }

        public double DifferentialEvolutionRate
        {
            get { return 1; } // Best value is 1.
        }

        public double LevyFlightRate
        {
            get { return 1; } // Best value is 1.
        }

        public double CrossOverRate
        {
            get { return 0.10; } // Best value of 0:0.1.
        }

        private const int StagnationLimit = 10;
        private double _mLastCost = double.MaxValue;
        private int _mStagnationCount;

        public bool TerminationCondition(NodeChromosome[] nests, int evaluations)
        {
            return (evaluations > 25000);

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

        public NodeChromosome DifferentialEvolution(NodeChromosome[] nests, int i)
        {
            return GenePool.DoDifferentialEvolution(nests[i], nests);

        }

        public NodeChromosome LevyFlight(NodeChromosome nest, NodeChromosome bestNest)
        {
            return GenePool.DoLevyFlight(nest, bestNest);   
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
