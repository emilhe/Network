using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.Utils;
using Optimization;

namespace BusinessLogic.Cost
{

    public class ParallelNodeCostCalculator : ICostCalculator<NodeChromosome>
    {

        private bool _mFull = false;
        private bool _mDirty = false;
        private int _mEvaluations;
        private ISolarCostModel _mSolarCostModel = new SolarCostModelImpl();

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

        public ISolarCostModel SolarCostModel
        {
            set
            {
                if (_mSolarCostModel == value) return;
                _mSolarCostModel = value;
                _mDirty = true;
            }
        }

        public Func<NodeCostCalculator> SpawnCostCalc { get; set; } 

        private ConcurrentDictionary<int, NodeCostCalculator> _mCalcMap;
        private readonly ParallelOptions _mOptions;

        public ParallelNodeCostCalculator(int maxDegreeOfParallelism = -1)
        {
            if (maxDegreeOfParallelism == -1) maxDegreeOfParallelism = Environment.ProcessorCount;
            _mOptions = new ParallelOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism};
            _mCalcMap = new ConcurrentDictionary<int, NodeCostCalculator>();
            SpawnCostCalc = SpawnCalc;
            // Initialize dummy calculator to ensure that the cache is updated.
            var dummy = SpawnCostCalc();
        }

        public void UpdateCost(IList<NodeChromosome> chromosomes)
        {
            Update();
            _mEvaluations += chromosomes.Count();

            Parallel.ForEach(chromosomes, _mOptions, chromosome =>
            {
                // Very expensive if extra thread are spawned (they are, apparently..).
                var id = Thread.CurrentThread.ManagedThreadId;
                if (!_mCalcMap.ContainsKey(id)) _mCalcMap.TryAdd(id, SpawnCostCalc());
                chromosome.UpdateCost(solution => _mCalcMap[id].SystemCost((solution as NodeChromosome).Genes));
            });
        }

        public T[] ParallelEval<T>(IList<NodeGenes> chromosomes, Func<NodeCostCalculator, NodeGenes, T> evalFunc)
        {
            Update();
            var result = new T[chromosomes.Count()];

            Parallel.For(0, result.Length, _mOptions, i =>
            {
                // Very expensive if extra thread are spawned (they are, apparently..).
                var id = Thread.CurrentThread.ManagedThreadId;
                if (!_mCalcMap.ContainsKey(id)) _mCalcMap.TryAdd(id, SpawnCostCalc());
                result[i] = evalFunc(_mCalcMap[id], chromosomes[i]);
            });

            return result;
        }

        public void ResetEvals()
        {
            _mEvaluations = 0;
        }

        private void Update()
        {
            // If dirty, new cost calculators must be initialized.
            if (_mDirty) _mCalcMap = new ConcurrentDictionary<int, NodeCostCalculator>();
            foreach (var calculator in _mCalcMap) calculator.Value.CacheEnabled = CacheEnabled;
        }

        private NodeCostCalculator SpawnCalc()
        {
            return new NodeCostCalculator(new ParameterEvaluator(Full) { CacheEnabled = CacheEnabled }) { SolarCostModel = _mSolarCostModel };
        }

    }
}
