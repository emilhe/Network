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

    public class ParallelNodeCostCalculator : ICostCalculator<NodeChromosome>
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

        private List<NodeCostCalculator> _mCalcs;
        private readonly ParallelOptions _mOptions;

        public ParallelNodeCostCalculator(int maxDegreeOfParallelism = -1)
        {
            if (maxDegreeOfParallelism == -1) maxDegreeOfParallelism = Environment.ProcessorCount;
            _mOptions = new ParallelOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism};
            InitializeCalculators();
        }

        private void InitializeCalculators()
        {
            _mCalcs = new List<NodeCostCalculator>();
            for (int i = 0; i < _mOptions.MaxDegreeOfParallelism; i++)
            {
                _mCalcs.Add(new NodeCostCalculator(new ParameterEvaluator(Full) { CacheEnabled = CacheEnabled }));
            }
        }

        public void UpdateCost(IList<NodeChromosome> chromosomes)
        {
            // If dirty, new cost calculators must be initialized.
            if (_mDirty) InitializeCalculators();

            _mEvaluations += chromosomes.Count();

            Parallel.For(0, chromosomes.Count, _mOptions, i =>
            {
                chromosomes[i].UpdateCost(solution => _mCalcs[i%_mOptions.MaxDegreeOfParallelism].SystemCost((solution as NodeChromosome).Genes, Transmission));
            });
        }

        public T[] ParallelEval<T>(IList<NodeGenes> chromosomes, Func<NodeCostCalculator, NodeGenes, T> evalFunc)
        {
            // If dirty, new cost calculators must be initialized.
            if (_mDirty) InitializeCalculators();

            var result = new T[chromosomes.Count];

            Parallel.For(0, chromosomes.Count, _mOptions, i =>
            {
                result[i] = evalFunc(_mCalcs[i % _mOptions.MaxDegreeOfParallelism], chromosomes[i]);
            });

            return result;
        }

    }
}
