using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;
using Utils;

namespace BusinessLogic.Nodes
{

    public class CountryNode : INode
    {

        public string Name { get { return Model.Name; } }
        public string Abbreviation { get { return CountryInfo.GetAbbrev(Name); } }

        public ReModel Model { get; set; }
        public Measureable Balancing { get; set; }
        public IList<IGenerator> Generators { get; set; }
        public IList<IStorage> Storages { get; set; }

        private double _mLoad { get; set; }

        public CountryNode(ReModel model)
        {
            Model = model;

            Balancing = new Measureable("Balancing");
            Balancing.Properties.Add("Country", Name);

            Generators = Model.GetGenerators();
            Storages = new SortedStorages();
 
        }

        public double GetDelta()
        {
            return Generators.Sum(generator => generator.Production) - _mLoad;
        }

        #region Measurement

        public bool Measuring { get; private set; }

        public List<ITimeSeries> CollectTimeSeries()
        {
            var result = new List<ITimeSeries>();
            foreach (var generator in Generators.Where(item => item.Measuring))
            {
                result.AddRange(generator.CollectTimeSeries());
            }
            foreach (var storage in Storages.Where(item => item.Measuring))
            {
                result.AddRange(storage.CollectTimeSeries());
            }
            // Bind country dependence.
            foreach (var ts in result) ts.Properties.Add("Country", Name);
            return result;
        }

        public double CurrentValue
        {
            get { return Balancing.CurrentValue; }
        }

        public double Curtailment
        {
            get { return Math.Max(0, Balancing.CurrentValue); }
        }

        public double Backup
        {
            get { return Math.Max(0, -Balancing.CurrentValue); }
        }

        public void Start(int ticks)
        {
            foreach (var generator in Generators) generator.Start(ticks);
            foreach (var storage in Storages) storage.Start(ticks);
            Measuring = true;
        }

        public void Clear()
        {
            foreach (var generator in Generators) generator.Clear();
            foreach (var storage in Storages) storage.Clear();
            Measuring = false;
        }

        public void Sample(int tick)
        {
            foreach (var generator in Generators) generator.Sample(tick);
            foreach (var storage in Storages) storage.Sample(tick);
        }

        #endregion

        public void TickChanged(int tick)
        {
            _mLoad = Model.LoadTimeSeries.GetValue(tick);
            foreach (var generator in Generators) generator.TickChanged(tick);
            foreach (var storage in Storages) storage.TickChanged(tick);
        }

        class SortedStorages : IList<IStorage>
        {
            private List<IStorage> _mCore = new List<IStorage>();

            void UpdateOrder()
            {
                _mCore = _mCore.OrderByDescending(item => item.Efficiency).ToList();
            }

            #region Delegation

            public IEnumerator<IStorage> GetEnumerator()
            {
                return _mCore.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable) _mCore).GetEnumerator();
            }

            public void Add(IStorage item)
            {
                _mCore.Add(item);
                UpdateOrder();
            }

            public void Clear()
            {
                _mCore.Clear();
            }

            public bool Contains(IStorage item)
            {
                return _mCore.Contains(item);
            }

            public void CopyTo(IStorage[] array, int arrayIndex)
            {
                _mCore.CopyTo(array, arrayIndex);
            }

            public bool Remove(IStorage item)
            {
                var removed = _mCore.Remove(item);
                UpdateOrder();
                return removed;
            }

            public int Count
            {
                get { return _mCore.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public int IndexOf(IStorage item)
            {
                return _mCore.IndexOf(item);
            }

            public void Insert(int index, IStorage item)
            {
                _mCore.Insert(index, item);
                UpdateOrder();
            }

            public void RemoveAt(int index)
            {
                _mCore.RemoveAt(index);
                UpdateOrder();
            }

            public IStorage this[int index]
            {
                get { return _mCore[index]; }
                set { _mCore[index] = value; }
            }

            #endregion

        }

    }

}
