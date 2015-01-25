using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Utils;
using Optimization;

namespace BusinessLogic.Cost
{

    public class ParallelCostCalculator<T> : ICostCalculator<T> where T : ISolution
    {

        private bool _mFull;
        private bool _mDirty;
        private int _mEvaluations;

        public bool Transmission { get; set; }
        public bool CacheEnabled { get; set; }
        
        public int Evaluations
        {
            get { return _mEvaluations; }
        }

        public bool Full
        {
            get { return _mFull; }
            set
            {
                if (_mFull == value) return;
                _mFull = value;
                _mDirty = true;
            }
        }

        private ConcurrentDictionary<int, NodeCostCalculator> _mCalcMap;
        private readonly ParallelOptions _mOptions;

        public ParallelCostCalculator(int maxDegreeOfParallelism = -1)
        {
            if (maxDegreeOfParallelism == -1) maxDegreeOfParallelism = Environment.ProcessorCount;
            _mOptions = new ParallelOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism};
            _mCalcMap = new ConcurrentDictionary<int, NodeCostCalculator>();
            // Initialize dummy calculator to ensure that the cache is updated.
            var dummy = new NodeCostCalculator(new ParameterEvaluator(Full) { CacheEnabled = CacheEnabled });
     
        }

        public void UpdateCost(IEnumerable<T> chromosomes)
        {        
            // If dirty, new cost calculators must be initialized.
            if (_mDirty) _mCalcMap = new ConcurrentDictionary<int, NodeCostCalculator>();
            foreach (var calculator in _mCalcMap) calculator.Value.CacheEnabled = CacheEnabled;
    
            Parallel.ForEach(chromosomes, _mOptions, chromosome =>
            {
                // Very expensive if extra thread are spawned (they are, apparently..).
                var id = Thread.CurrentThread.ManagedThreadId;
                if (!_mCalcMap.ContainsKey(id)) _mCalcMap.TryAdd(id, new NodeCostCalculator(new ParameterEvaluator(Full) { CacheEnabled = CacheEnabled }));
                // If non-nodechromosomes are used, this FUCKS UP (like, BADLY)!
                chromosome.UpdateCost(
                    solution => _mCalcMap[id].SystemCost((solution as NodeChromosome).Genes, Transmission));
            });
        }

    }
}
