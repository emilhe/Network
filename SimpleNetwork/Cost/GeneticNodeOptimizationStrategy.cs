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

        private const int StagnationLimit = 15;
        private const int ImmortalCount = 5;

        private const double ChildFrac = 0.5;
        private const double EliteFrac = 0.1;
        private const double EliteMixFrac = 0.02;

        private double _mLastCost = double.MaxValue;
        private int _mStagnationCount;

        // Iterate until no progress has been made in [StagnationLimit] generations.
        public bool TerminationCondition(NodeChromosome[] chromosomes)
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

        public void Select(NodeChromosome[] chromosomes)
        {
            var n = chromosomes.Length;
            // Kill bad candidates (only necessary if EliteMixFrac != 0).
            for (var i = (int)Math.Ceiling(n * EliteFrac); i < n; i++) chromosomes[i] = GenePool.Spawn();
        }

        public void Mate(NodeChromosome[] chromosomes)
        {
            var n = chromosomes.Length;
            var offspring = new NodeChromosome[(int)Math.Ceiling(n * ChildFrac)];

            // Find children.
            for (int i = 0; i < (int)Math.Ceiling(n * ChildFrac); i++)
            {
                //var father = chromosomes[0];
                var father = chromosomes[(int)(Rnd.NextDouble() * (Math.Ceiling(n * (EliteFrac + EliteMixFrac)) - 1))];
                var mother = chromosomes[(int)(Rnd.NextDouble() * (Math.Ceiling(n * (EliteFrac + EliteMixFrac)) - 1))];
                offspring[i] = (NodeChromosome) father.Mate(mother);
            }

            // Fill in children + randoms.
            for (int i = (int)Math.Ceiling(n * EliteFrac); i < (int)Math.Ceiling(n * (ChildFrac+EliteFrac)); i++)
            {
                chromosomes[i] = offspring[i - (int)Math.Ceiling(n * EliteFrac)]; //(i < offspring.Length) ? offspring[i] : Spawn();
            }
        }

        public void Mutate(NodeChromosome[] chromosomes)
        {
            for (int i = ImmortalCount; i < chromosomes.Length; i++) chromosomes[i].Mutate();
        }

    }
}
