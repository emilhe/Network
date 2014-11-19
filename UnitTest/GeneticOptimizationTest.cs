using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;

namespace UnitTest
{
    class GeneticOptimizationTest
    {

            
        class StringChromosome : IChromosome
        {
            
            private const string _mTarget = "Hello World!";
            private string Genes { get; set; }

            public double Cost { get; private set; }

            public StringChromosome(string genes)
            {
                Genes = genes;

                Cost = CalculateCost();;
            }

            private double CalculateCost()
            {
                var cost = 0.0;
                var idx = 0;

                foreach (var gene in Genes)
                {
                    cost = Math.Abs(Char.GetNumericValue(gene) - Char.GetNumericValue(_mTarget[idx]));
                    idx++;
                }

                return cost;
            }

            public IChromosome[] Mate(IChromosome partner)
            {
                var validPartner = partner as StringChromosome;
                if(validPartner == null) throw new ArgumentException("Invalid partner.");

                var result = new IChromosome[2];
                result[0] = new StringChromosome(Genes.Substring(0, 6) + validPartner.Genes.Substring(6));
                result[1] = new StringChromosome(validPartner.Genes.Substring(0, 6) + Genes.Substring(6));

                return result;
            }

            public void Mutate()
            {
                var rnd = new Random(DateTime.Now.Millisecond);
                var idx = rnd.Next(0, 11);
                var mutation = Genes.ToCharArray();
                var numericValue = Char.ConvertFromUtf32(((int) Char.GetNumericValue(mutation[idx])));
                mutation[idx] = Convert.ToChar(numericValue + rnd.Next(0, 100));
                Genes = new string(mutation);
            }

        }

    }
}
