using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Utils;
using Optimization;

namespace BusinessLogic.Cost
{

    public class ParallelCostCalculator : ICostCalculator<NodeChromosome>
    {

        // TODO: FIX TRANSMISSION  & FULL PROPERTY
        public bool Transmission { get; set; }
        public bool Full { get; set; }

        private readonly Dictionary<int, NodeCostCalculator> _mCalcMap;
        private readonly ParallelOptions _mOptions;

        public ParallelCostCalculator(int maxDegreeOfParallelism = -1)
        {
            if (maxDegreeOfParallelism == -1) maxDegreeOfParallelism = Environment.ProcessorCount;
            _mOptions = new ParallelOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism};
            _mCalcMap = new Dictionary<int, NodeCostCalculator>(maxDegreeOfParallelism);
        }

        public void UpdateCost(NodeChromosome[] chromosomes)
        {
            Parallel.ForEach(chromosomes, _mOptions, chromosome =>
            {
                // Very expensive, of extra thread are spawned (they are, apparently..).
                var id = Thread.CurrentThread.ManagedThreadId;
                if (!_mCalcMap.ContainsKey(id)) _mCalcMap.Add(id, new NodeCostCalculator(new ParameterEvaluator(Full) { CacheEnabled = false }));
                chromosome.UpdateCost(_mCalcMap[id]);
            });
        }

    }
}
