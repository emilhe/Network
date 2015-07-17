using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleImporter;
using Utils;

namespace BusinessLogic.Cost
{
    public class NodeGenes : IDictionary<string, NodeGene>
    {

        private readonly Dictionary<string, NodeGene> _mGenes; 

        public double Alpha
        {
            set
            {
                foreach (var key in _mGenes.Keys)
                {
                    _mGenes[key].Alpha = value;
                }
            }
            get
            {
                //Console.WriteLine("This should never happen [ALPHA accessed]");
                // This is a BIT hacky..
                var wind = 0.0;
                var solar = 0.0;
                foreach (var key in _mGenes.Keys)
                {
                    var load = CountryInfo.GetMeanLoad(key);
                    wind += _mGenes[key].Gamma * load * _mGenes[key].Alpha;
                    solar += _mGenes[key].Gamma * load * (1 - _mGenes[key].Alpha);
                }
                return 1 / (1 + solar / wind);
            }
        }

        public double Gamma
        {
            set
            {
                foreach (var key in _mGenes.Keys)
                {
                    _mGenes[key].Gamma = value;
                }
            }
            get
            {
                //Console.WriteLine("This should never happen [GAMMA accessed]");
                // This is a BIT hacky..
                var wind = 0.0;
                var solar = 0.0;
                foreach (var key in _mGenes.Keys)
                {
                    var load = CountryInfo.GetMeanLoad(key);
                    wind += _mGenes[key].Gamma * load * _mGenes[key].Alpha;
                    solar += _mGenes[key].Gamma * load * (1 - _mGenes[key].Alpha);
                }
                return (wind + solar) / CountryInfo.GetMeanLoadSum(); 
            }
        }

        public NodeGenes()
        {
            _mGenes = CountryInfo.GetCountries().ToDictionary(item => item, item => new NodeGene());
        }

        public NodeGenes(double alpha, double gamma)
        {
            _mGenes = CountryInfo.GetCountries().ToDictionary(item => item, item => new NodeGene{Alpha = alpha, Gamma = gamma});
        }

        public NodeGenes(Func<NodeGene> seed)
        {
            _mGenes = CountryInfo.GetCountries().ToDictionary(item => item, item => seed());
        }

        public NodeGenes(Dictionary<string, NodeGene> genes)
        {
            _mGenes = genes;
        }

        // Get exact (deep) copy.
        public NodeGenes Clone()
        {
            var genes = new Dictionary<string, NodeGene>(_mGenes.Count);
            CloneGenes(genes, s => s);

            return new NodeGenes(genes);
        }

        // Get (deep) copy with country abbreviation strings (used for graphs).
        public NodeGenes Export()
        {
            var genes = new Dictionary<string, NodeGene>(_mGenes.Count);
            CloneGenes(genes, CountryInfo.GetShortAbbrev);

            return new NodeGenes(genes);
        }


        // Get (deep) copy with country abbreviation strings (used for graphs).
        public NodeGenes Import()
        {
            var genes = new Dictionary<string, NodeGene>(_mGenes.Count);
            CloneGenes(genes, CountryInfo.GetName);

            return new NodeGenes(genes);
        }

        private void CloneGenes(Dictionary<string, NodeGene> genes, Func<string, string> map)
        {
            foreach (var gene in _mGenes)
            {
                genes.Add(map(gene.Key), new NodeGene
                {
                    Alpha = gene.Value.Alpha,
                    Gamma = gene.Value.Gamma,
                    OffshoreFraction = gene.Value.OffshoreFraction,
                });
            }
        }

        #region Delegation 

        public IEnumerator<KeyValuePair<string, NodeGene>> GetEnumerator()
        {
            return _mGenes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _mGenes).GetEnumerator();
        }

        public void Add(KeyValuePair<string, NodeGene> item)
        {
            _mGenes.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _mGenes.Clear();
        }

        public bool Contains(KeyValuePair<string, NodeGene> item)
        {
            return _mGenes.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, NodeGene>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, NodeGene> item)
        {
            return _mGenes.Remove(item.Key);
        }

        public int Count
        {
            get { return _mGenes.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool ContainsKey(string key)
        {
            return _mGenes.ContainsKey(key);
        }

        public void Add(string key, NodeGene value)
        {
            _mGenes.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _mGenes.Remove(key);
        }

        public bool TryGetValue(string key, out NodeGene value)
        {
            return _mGenes.TryGetValue(key, out value);
        }

        public NodeGene this[string key]
        {
            get { return _mGenes[key]; }
            set
            {
                if (!_mGenes.ContainsKey(key)) key = CountryInfo.GetName(key);
                _mGenes[key] = value;
            }
        }

        public ICollection<string> Keys
        {
            get { return _mGenes.Keys; }
        }

        public ICollection<NodeGene> Values
        {
            get { return _mGenes.Values; }
        }

        #endregion

    }
}
