using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;

namespace BusinessLogic.Storages
{
    public class StorageCollection : IEnumerable<KeyValuePair<double, IStorage>>
    {

        private readonly Dictionary<double, IStorage> _mStorageMap;
        private IOrderedEnumerable<double> _orderedKeys;

        public StorageCollection()
        {
            _mStorageMap = new Dictionary<double, IStorage>();
        }

        public void Add(IStorage storage)
        {
            if (!_mStorageMap.ContainsKey(storage.Efficiency))
            {
                _mStorageMap.Add(storage.Efficiency, storage);
                _orderedKeys = _mStorageMap.Keys.OrderByDescending(item => item);
                return;
            }

            // More storages with the same efficiency; convert to composite storages.
            var composite = _mStorageMap[storage.Efficiency] as CompositeStorage;
            if (composite == null)
            {
                composite = new CompositeStorage(_mStorageMap[storage.Efficiency]);
                _mStorageMap[storage.Efficiency] = composite;
            }
            composite.AddStorage(storage);

            _orderedKeys = _mStorageMap.Keys.OrderByDescending(item => item);
        }

        public bool Contains(double efficiency)
        {
            return _mStorageMap.ContainsKey(efficiency);
        }

        public IStorage Get(double efficiency)
        {
            return _mStorageMap[efficiency];
        }

        /// <summary>
        /// Injects an amount of energy (positive or negative) into the most efficient storage available.
        /// </summary>
        /// <param name="amount"> amount of energy </param>
        /// <returns> remaining energy </returns>
        public double Inject(double amount)
        {
            var remaining = amount;
            foreach (var key in _orderedKeys)
            {
                remaining = _mStorageMap[key].Inject(amount);
                if (Math.Abs(remaining) < 1e-5) break;
            }
            return remaining;
        }

        public IEnumerator<KeyValuePair<double, IStorage>> GetEnumerator()
        {
            return _mStorageMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _mStorageMap).GetEnumerator();
        }
    }
}
