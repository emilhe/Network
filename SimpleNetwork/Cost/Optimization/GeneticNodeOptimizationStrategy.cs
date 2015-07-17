using System;
using Optimization;

namespace BusinessLogic.Cost.Optimization
{
    public class GeneticNodeOptimizationStrategy : IGeneticOptimizationStrategy<NodeChromosome>
    {

        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);

        private const int StagnationLimit = 10;
        private const int ImmortalCount = 5;

        private const double ChildFrac = 0.5;
        private const double EliteFrac = 0.15;
        private const double EliteMixFrac = 0.05;

        private double _mLastCost = double.MaxValue;
        private int _mStagnationCount;

        public bool TerminationCondition(NodeChromosome[] chromosomes, int evaluations)
        {
            return (evaluations > 25000);

            if (Math.Abs(chromosomes[0].Cost - _mLastCost) > 1e-3)
            {
                _mLastCost = chromosomes[0].Cost;
                _mStagnationCount = 0;
                return false;
            }

            _mLastCost = chromosomes[0].Cost;
            _mStagnationCount++;
            return _mStagnationCount == StagnationLimit;
        }

        public void Select(NodeChromosome[] chromosomes)
        {
            var n = chromosomes.Length;
            // Kill bad candidates (only necessary if EliteMixFrac != 0).
            for (var i = (int)Math.Ceiling(n * EliteFrac); i < n; i++) chromosomes[i] = GenePool.SpawnChromosome();
        }

        public void Mate(NodeChromosome[] chromosomes)
        {
            var n = chromosomes.Length;
            var offspring = new NodeChromosome[(int)Math.Ceiling(n * ChildFrac)];

            // Find children.
            for (int i = 0; i < (int)Math.Ceiling(n * ChildFrac); i++)
            {
                //var father = chromosomes[0];
                var fIdx = Math.Ceiling(Rnd.NextDouble()*(Math.Floor(n*(EliteFrac + EliteMixFrac))) - 1);
                var father = chromosomes[(int) fIdx];
                var mÌdx = Math.Ceiling(Rnd.NextDouble()*(Math.Floor(n*(EliteFrac + EliteMixFrac))) - 1);
                var mother = chromosomes[(int)mÌdx];
                offspring[i] = (NodeChromosome) father.Mate(mother);
            }

            // Fill in children + randoms.
            for (int i = (int)Math.Ceiling(n * EliteFrac); i < (int)Math.Ceiling(n * (ChildFrac+EliteFrac)); i++)
            {
                var offspringIdx = i - (int) Math.Ceiling(n*EliteFrac);
                chromosomes[i] = offspring[offspringIdx]; //(i < offspring.Length) ? offspring[i] : SpawnChromosome();
            }
        }

        public void Mutate(NodeChromosome[] chromosomes)
        {
            for (int i = ImmortalCount; i < chromosomes.Length; i++) chromosomes[i].Mutate();
        }

    }
}
