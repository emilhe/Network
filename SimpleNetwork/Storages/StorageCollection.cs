﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;

namespace BusinessLogic.Storages
{
    public class StorageCollection : IEnumerable<KeyValuePair<double, IStorage>>
    {

        private readonly Dictionary<double, IStorage> _mStorageMap;

        public StorageCollection()
        {
            _mStorageMap = new Dictionary<double, IStorage>();
        }

        public void Add(IStorage storage)
        {
            if (!_mStorageMap.ContainsKey(storage.Efficiency))
            {
                _mStorageMap.Add(storage.Efficiency, storage);
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
        }

        public bool Contains(double efficiency)
        {
            return _mStorageMap.ContainsKey(efficiency);
        }

        public IStorage Get(double efficiency)
        {
            return _mStorageMap[efficiency];
        }

        //public Dictionary<double, IStorage>.ValueCollection Storages()
        //{
        //    return _mStorageMap.Values;
        //}

        //public Dictionary<double, IStorage>.KeyCollection Efficiencies()
        //{
        //    return _mStorageMap.Keys;
        //}

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