using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Optimization;

namespace BusinessLogic.Cost
{
    public class GeneticNodeOptimizationStrategy : IGeneticOptimizationStrategy<NodeChromosome>
    {

        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);
        private readonly CostCalculator _mCalc;

        private const int StagnationLimit = 10;

        private const double AlphaMin = 0;
        private const double AlphaMax = 1;
        private const double GammaMin = 0;
        private const double GammaMax = 2;

        private const int ImmortalCount = 1;

        private const double ChildFrac = 0.5;
        private const double EliteFrac = 0.1;
        private const double EliteMixFrac = 0.02;

        private double _mLastCost = double.MaxValue;
        private int _mStagnationCount = 0;

        public GeneticNodeOptimizationStrategy(CostCalculator calc)
        {
            _mCalc = calc;
        }

        // Iterate until no progress has been made in [StagnationLimit] generations.
        public bool TerminationCondition(IChromosome[] chromosomes)
        {
            if (Math.Abs(chromosomes[0].Cost - _mLastCost) > 1e-5)
            {
                _mLastCost = chromosomes[0].Cost;
                _mStagnationCount = 0;
                return false;
            }

            _mLastCost = chromosomes[0].Cost;
            _mStagnationCount++;
            return _mStagnationCount == StagnationLimit;
        }

        public void Select(IChromosome[] chromosomes)
        {
            var n = chromosomes.Length;
            // Kill bad candidates (only necessary if EliteMixFrac != 0).
            for (var i = (int)Math.Ceiling(n * EliteFrac); i < n; i++) chromosomes[i] = Spawn();
        }

        public void Mate(IChromosome[] chromosomes)
        {
            var n = chromosomes.Length;
            var offspring = new IChromosome[(int)Math.Ceiling(n * ChildFrac)];

            // Find children.
            for (int i = 0; i < (int)Math.Ceiling(n * ChildFrac); i++)
            {
                //var father = chromosomes[0];
                var father = chromosomes[(int)(Rnd.NextDouble() * (Math.Ceiling(n * (EliteFrac + EliteMixFrac)) - 1))];
                var mother = chromosomes[(int)(Rnd.NextDouble() * (Math.Ceiling(n * (EliteFrac + EliteMixFrac)) - 1))];
                offspring[i] = father.Mate(mother);
            }

            // Fill in children + randoms.
            for (int i = (int)Math.Ceiling(n * EliteFrac); i < (int)Math.Ceiling(n * (ChildFrac+EliteFrac)); i++)
            {
                chromosomes[i] = offspring[i - (int)Math.Ceiling(n * EliteFrac)]; //(i < offspring.Length) ? offspring[i] : Spawn();
            }
        }

        public void Mutate(IChromosome[] chromosomes)
        {
            for (int i = ImmortalCount; i < chromosomes.Length; i++) chromosomes[i].Mutate();
        }

        public IChromosome Spawn()
        {
            return new NodeChromosome(new NodeDna(RndGene), _mCalc);
        }

        #region Gene modification (here to respect limits)

        public static NodeGene RndGene()
        {
            return new NodeGene
            {
                Alpha = Rnd.NextDouble() * (AlphaMax - AlphaMin) + AlphaMin,
                Gamma = Rnd.NextDouble() * (GammaMax - GammaMin) + GammaMin
            };
        }

        public static void ReSeed(NodeGene gene)
        {
            gene.Alpha = Rnd.NextDouble()*(AlphaMax - AlphaMin) + AlphaMin;
            gene.Gamma = Rnd.NextDouble()*(GammaMax - GammaMin) + GammaMin;
        }

        public static void Mutate(NodeGene gene)
        {
            gene.Alpha = gene.Alpha + 0.05*(0.5 - Rnd.NextDouble());
            if (gene.Alpha < AlphaMin) gene.Alpha = AlphaMin;
            if (gene.Alpha > AlphaMax) gene.Alpha = AlphaMax;

            gene.Gamma = gene.Alpha + 0.05 * (0.5 - Rnd.NextDouble());
            if (gene.Gamma < GammaMin) gene.Gamma = GammaMin;
            if (gene.Gamma > GammaMax) gene.Gamma = GammaMax;
        }

        #endregion

    }
}
