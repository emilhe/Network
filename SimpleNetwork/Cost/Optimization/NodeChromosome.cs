using System;
using System.Collections.Generic;
using BusinessLogic.Cost.Optimization;
using Optimization;

namespace BusinessLogic.Cost
{
    public class NodeChromosome : IChromosome
    {
        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);
       
        private double _mCost;
        private double _mGamma = 1;

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

        public bool InvalidCost { get; set; }

        public double Cost
        {
            get 
            {
                if(InvalidCost) throw new ArgumentException("Cost is not updated.");
                return _mCost;
            }
            // Should ONLY be set internally or by JSON deserializer.
            set
            {
                _mCost = value;
                InvalidCost = false;
            }
        }

        // Should ONLY be set by JSON deserializer.   
        public NodeGenes Genes { get; set; }

        public NodeChromosome()
        {
            InvalidCost = true;
        }

        public NodeChromosome(NodeGenes genes)
        {
            Genes = genes;
            InvalidCost = true;

            //if(GenePool.Renormalize(this)) return;
            //throw new ArgumentException("Renormalisation impossible within alpha/gamma constraints.");
        }

        public NodeChromosome(NodeVec vec)
        {
            var genes = new Dictionary<string, NodeGene>();
            for (int i = 0; i < NodeVec.Labels.Count; i++)
            {
                var key = NodeVec.Labels[i];
                genes.Add(key, new NodeGene
                {
                    Gamma = vec[i],
                    Alpha = vec[i + NodeVec.Labels.Count],
                });
            }
            Genes = new NodeGenes(genes);

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
                //success = GenePool.Renormalize(child);
                GenePool.RecursiveGammaRescaling(this, new List<string>());
                success = true;
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
                    Genes[key].Gamma += 0.25 * (0.5 - Rnd.NextDouble());
                    if (Genes[key].Gamma < GenePool.GammaMin) Genes[key].Gamma = GenePool.GammaMin;
                    if (Genes[key].Gamma > GenePool.GammaMax) Genes[key].Gamma = GenePool.GammaMax;
                    Genes[key].Alpha += 0.10 * (0.5 - Rnd.NextDouble());
                    if (Genes[key].Alpha < GenePool.AlphaMin) Genes[key].Alpha = GenePool.AlphaMin;
                    if (Genes[key].Alpha > GenePool.AlphaMax) Genes[key].Alpha = GenePool.AlphaMax;
                    //GenePool.TryMutate(this, key);
                }
                // 10% chance for reseed.                    
                else
                {
                    Genes[key].Gamma = Rnd.NextDouble() * (GenePool.GammaMax - GenePool.GammaMin) + GenePool.GammaMin;
                    Genes[key].Alpha = Rnd.NextDouble() * (GenePool.AlphaMax - GenePool.AlphaMin) + GenePool.AlphaMin;
                    //GenePool.TryReSeed(this, key);
                }
            }
            GenePool.RecursiveGammaRescaling(this, new List<string>());

            InvalidCost = true;
        }

        public ISolution Clone()
        {
            return new NodeChromosome(Genes.Clone());
        }

        public void UpdateCost(Func<ISolution, double> eval)
        {
            Cost = eval(this);
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

        //public IVectorSolution Add(IVectorSolution partner, double weight)
        //{
        //    var other = partner as NodeChromosome;
        //    if (other == null) throw new ArgumentException("Invalid partner.");

        //    var genes = new NodeGenes();
        //    foreach (var name in Genes.Keys)
        //    {
        //        genes[name].Gamma = Genes[name].Gamma + other.Genes[name].Gamma*weight;
        //        genes[name].Alpha = Genes[name].Alpha + other.Genes[name].Alpha*weight;
        //    }
        //    return new NodeChromosome(genes);
        //}

        //public IVectorSolution Sub(IVectorSolution other)
        //{
        //    return Add(other, -1);
        //}
    }
}
