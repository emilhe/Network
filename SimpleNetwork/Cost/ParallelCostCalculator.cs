using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Optimization;

namespace BusinessLogic.Cost
{

    public class ParallelCostCalculator : ICostCalculator<NodeChromosome>
    {

        //private readonly List<ThreadSafeNodeCostCalculator> _mCalcs;
        private readonly Dictionary<int, ThreadSafeNodeCostCalculator> _mCalcMap;
        private readonly ParallelOptions _mOptions;

        public ParallelCostCalculator(int maxDegreeOfParallelism = -1)
        {
            if (maxDegreeOfParallelism == -1) maxDegreeOfParallelism = Environment.ProcessorCount;
            _mOptions = new ParallelOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism};
            _mCalcMap = new Dictionary<int, ThreadSafeNodeCostCalculator>(maxDegreeOfParallelism);
            //_mCalcs = new List<ThreadSafeNodeCostCalculator>(maxDegreeOfParallelism);
            //for (int i = 0; i < maxDegreeOfParallelism; i++)
            //{
            //    _mCalcs.Add(new ThreadSafeNodeCostCalculator());
            //}
        }

        public void UpdateCost(NodeChromosome[] chromosomes)
        {
            Parallel.ForEach(chromosomes, _mOptions, chromosome =>
            {
                var id = Thread.CurrentThread.ManagedThreadId;
                if (!_mCalcMap.ContainsKey(id))
                {
                    _mCalcMap.Add(id, new ThreadSafeNodeCostCalculator());
                }
                chromosome.UpdateCost(_mCalcMap[id]);
            });
            //foreach (var calc in _mCalcs) calc.Busy = false;
        }

    }
}
