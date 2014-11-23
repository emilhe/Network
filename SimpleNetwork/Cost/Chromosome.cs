using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.LCOE;

namespace BusinessLogic.Cost
{
    public class Chromosome : IList<MixGene>
    {

        public List<MixGene> Genes { get; private set; }

        public double Alpha
        {
            set
            {
                foreach (var gene in Genes)
                {
                    gene.Alpha = value;
                }
            }
        }

        public double Gamma
        {
            set
            {
                foreach (var gene in Genes)
                {
                    gene.Gamma = value;
                }
            }
        }

        public Chromosome(int count, double alpha = 0, double gamma = 0)
        {
            Genes = new List<MixGene>(count);
            if (alpha == 0 && gamma == 0) return;
            // Optionary; initialize with default values.
            for (int i = 0; i < count; i++) Genes.Add(new MixGene { Alpha = alpha, Gamma = gamma });
        }

        #region Delegation

        public IEnumerator<MixGene> GetEnumerator()
        {
            return Genes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Genes).GetEnumerator();
        }

        public void Add(MixGene item)
        {
            Genes.Add(item);
        }

        public void Clear()
        {
            Genes.Clear();
        }

        public bool Contains(MixGene item)
        {
            return Genes.Contains(item);
        }

        public void CopyTo(MixGene[] array, int arrayIndex)
        {
            Genes.CopyTo(array, arrayIndex);
        }

        public bool Remove(MixGene item)
        {
            return Genes.Remove(item);
        }

        public int Count
        {
            get { return Genes.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(MixGene item)
        {
            return Genes.IndexOf(item);
        }

        public void Insert(int index, MixGene item)
        {
            Genes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Genes.RemoveAt(index);
        }

        public MixGene this[int index]
        {
            get { return Genes[index]; }
            set { Genes[index] = value; }
        }

        #endregion

    }
}
