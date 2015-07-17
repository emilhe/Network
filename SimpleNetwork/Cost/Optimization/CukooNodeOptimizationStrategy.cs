using System;
using System.Linq;
using Optimization;
using Utils;

namespace BusinessLogic.Cost.Optimization
{
    public class CukooNodeOptimizationStrategy : ICukooOptimizationStrategy<NodeChromosome>
    {

        public Random Rnd = new Random();

        private const int StagnationLimit = 5;
        private double _mLastCost = double.MaxValue;
        private int _mStagnationCount;
        public NodeChromosome Best { get; set; }

        private int[] m_rndOrder1;
        private int[] m_rndOrder2;

        public bool TerminationCondition(NodeChromosome[] nests, int evaluations)
        {
            // Update the random ordering on 
            m_rndOrder1 = new int[nests.Length].Linspace().Shuffle(Rnd).ToArray();
            m_rndOrder2 = new int[nests.Length].Linspace().Shuffle(Rnd).ToArray();

            //if (Math.Abs(nests[0].Cost - _mLastCost) > 1e-2)
            //{
            //    _mLastCost = nests[0].Cost;
            //    _mStagnationCount = 0;
            //}

            //_mLastCost = nests[0].Cost;
            //_mStagnationCount++;
            //if (Best == null || Best.Cost > nests[0].Cost) Best = nests[0];

            //if (_mStagnationCount == StagnationLimit)
            //{
            //    for (int i = 0; i < nests.Length; i++)
            //    {
            //        nests[i] = GenePool.SpawnChromosome();
            //    }
            //    _mStagnationCount = 0;
            //}

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
            return GenePool.DoDifferentialEvolution(nests[i], nests, m_rndOrder1[i], m_rndOrder2[i]);

        }

        public NodeChromosome LevyFlight(NodeChromosome nest, NodeChromosome bestNest, double stepSize = 0)
        {
            return GenePool.DoLevyFlight(nest, bestNest, stepSize);   
        }

        //public NodeChromosome CrossOver(NodeChromosome badNest, NodeChromosome goodNest)
        //{
        //    var genes = new NodeGenes();
        //    var phi = (1 + Math.Sqrt(5))/2.0;
        //    foreach (var key in badNest.Genes.Keys.ToArray())
        //    {
        //        genes[key].Alpha = goodNest.Genes[key].Alpha + (badNest.Genes[key].Alpha - goodNest.Genes[key].Alpha)/phi;
        //        genes[key].Gamma = goodNest.Genes[key].Gamma + (badNest.Genes[key].Gamma - goodNest.Genes[key].Gamma)/phi;
        //    }
        //    var result = new NodeChromosome(genes);
        //    if(!GenePool.Renormalize(result)) throw new Exception("Unable to renormalize");
        //    return result;
        //}

    }
}
