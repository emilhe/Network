using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;

namespace BusinessLogic.Cost
{

    public class ParallelCostCalculator : ICostCalculator<NodeChromosome>
    {

        private readonly List<ThreadSafeNodeCostCalculator> _mCalcs;
        private readonly ParallelOptions _mOptions;

        public ParallelCostCalculator(int maxDegreeOfParallelism = -1)
        {
            if (maxDegreeOfParallelism == -1) maxDegreeOfParallelism = Environment.ProcessorCount;
            _mOptions = new ParallelOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism};
            _mCalcs = new List<ThreadSafeNodeCostCalculator>(maxDegreeOfParallelism);
            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                _mCalcs.Add(new ThreadSafeNodeCostCalculator());
            }
        }

        public void UpdateCost(NodeChromosome[] chromosomes)
        {
            Parallel.ForEach(chromosomes, _mOptions, chromosome =>
            {
                var calc = _mCalcs.FirstOrDefault(item => !item.Busy);
                if (calc == null) throw new ArgumentException("Insufficient calculators.");

                chromosome.UpdateCost(calc);
            });
        }

    }
}
