using System;
using BusinessLogic.LCOE;
using Optimization;

namespace BusinessLogic.Cost
{
    class MixChromosome : IChromosome
    {
        private static readonly Random _mRnd = new Random((int)DateTime.Now.Ticks);

        private bool _mInvalidCost = true;
        private double _mCost;

        public double Cost
        {
            get 
            {
                // Lazy cost evaluation.
                if(_mInvalidCost) UpdateCost();
                return _mCost;
            }
        }

        private MixGene[] _mGenes;

        public MixGene[] Genes
        {
            get { return _mGenes; }
        } 

        public IChromosome Mate(IChromosome partner)
        {
            throw new NotImplementedException();
        }

        // Mutation source: Optimal heterogeneity of a highly renewable pan-European electricity system
        public void Mutate()
        {
            foreach (var gene in Genes)
            {
                var destiny = _mRnd.NextDouble();
                if (destiny > 0.25) continue;

                // Mutate.
                if (destiny > 0.05)
                {                  
                    gene.Alpha = 0;
                    gene.Gamma = 0;
                }
                // Reseed.                    
                else
                {
                    gene.Alpha = 0;
                    gene.Gamma = 0;
                }
            }

            _mInvalidCost = true;
        }

        private void UpdateCost()
        {
            _mCost = 0;
            _mInvalidCost = false;
        }
    }
}
