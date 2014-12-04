using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;

namespace BusinessLogic.Cost
{
    public class SaNodeOptimizationStrategy : ISaOptimizationStrategy<NodeChromosome>
    {

        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);
        private readonly CostCalculator _mCalc;

        private const double AlphaMin = 0;
        private const double AlphaMax = 1;
        private const double GammaMin = 0;
        private const double GammaMax = 2;

        public SaNodeOptimizationStrategy(CostCalculator calc)
        {
            _mCalc = calc;
        }

        public NodeChromosome Spawn()
        {
            return new NodeChromosome(new NodeDna(RndGene), _mCalc);
        }

        private static NodeGene RndGene()
        {
            return new NodeGene
            {
                Alpha = Rnd.NextDouble() * (AlphaMax - AlphaMin) + AlphaMin,
                Gamma = Rnd.NextDouble() * (GammaMax - GammaMin) + GammaMin
            };
        }

    }
}
