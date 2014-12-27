﻿using System;
using Optimization;

namespace BusinessLogic.Cost
{
    public class NodeChromosome : IChromosome
    {
        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);
       
        private double _mCost;
        private double _mGamma = 1;
        private bool _mInvalidCost = true;

        // The OVERALL values (even though the DNA is heterogeneous).
        public double Gamma
        {
            get { return _mGamma; }
            set
            {
                _mGamma = value;
                if (Genes == null) return;
                GenePool.Renormalize(this);
            }
        }

        public double Alpha { get { return Genes.Alpha; } }

        public double Cost
        {
            get 
            {
                if(_mInvalidCost) throw new ArgumentException("Cost is not updated.");
                return _mCost;
            }
        }

        public NodeGenes Genes { get; private set; }

        public NodeChromosome()
        {
        }

        public NodeChromosome(NodeGenes genes)
        {
            Genes = genes;

            //if(GenePool.Renormalize(this)) return;
            //throw new ArgumentException("Renormalisation impossible within alpha/gamma constraints.");
        }

        public IChromosome Mate(IChromosome partner)
        {
            var validPartner = partner as NodeChromosome;
            if(validPartner == null) throw new ArgumentException("Invalid partner.");

            NodeChromosome child = null;
            var success = false;
            while (!success)
            {
                child = SpawnChild(validPartner);
                success = GenePool.Renormalize(child);
            }

            return child;
        }

        public void Mutate()
        {
            foreach (var key in Genes.Keys)
            {
                var destiny = Rnd.NextDouble();
                // 50% chance that SOMETHING happens.
                if (destiny > 0.50) continue;

                // 40% change for mutation.
                if (destiny > 0.10)
                {
                    GenePool.TryMutate(this, key);
                }
                // 10% chance for reseed.                    
                else
                {
                    GenePool.TryReSeed(this, key);
                }
            }

            _mInvalidCost = true;
        }

        public ISolution Clone()
        {
            return new NodeChromosome(Genes);
        }

        // TEST: Should in general not be invoked...
        public void UpdateCost(object calc      )
        {
            //// Should this be here?
            //Renormalize();

            // TODO: Take transmission into account.
            _mCost = ((INodeCostCalculator)calc).SystemCost(Genes, false);
            _mInvalidCost = false;
        }

        private NodeChromosome SpawnChild(NodeChromosome validPartner)
        {
            var childDna = new NodeGenes();
            foreach (var name in Genes.Keys)
            {
                var destiny = (Rnd.NextDouble() > 0.5);
                childDna[name].Gamma = destiny ? Genes[name].Gamma : validPartner.Genes[name].Gamma;
                childDna[name].Alpha = destiny ? Genes[name].Alpha : validPartner.Genes[name].Alpha;
           } 
            return new NodeChromosome(childDna);
        } 

        //private void Renormalize()
        //{
        //    //// Renormalize if necessary.
        //    //var effGamma = _mGenes.Gamma;
        //    //if (effGamma - Gamma < 1e-5) return;
        //    //// TODO: This can fuck up gamma limits (we skip this for now..).
        //    //foreach (var key in _mGenes.Keys) _mGenes[key].Gamma *= Gamma / effGamma;
        //}

    }
}