using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;

namespace BusinessLogic.Storages
{

    public class CompositeStorage : IStorage
    {
        private readonly List<IStorage> _mStorages;
        private IStorage _mCompositeStorage;

        public CompositeStorage(IStorage master)
        {
            _mStorages = new List<IStorage> { master };
            MergeStorages();
        }

        public void AddStorage(IStorage storage)
        {
            if (!storage.Efficiency.Equals(_mStorages[0].Efficiency))
            {
                throw new ArgumentException("Composite storage is ONLY to be used for storages of the same efficiency.");
            }

            _mStorages.Add(storage);
            MergeStorages();
        }

        private void MergeStorages()
        {
            var combinedCapacity = _mStorages.Select(item => item.Capacity).Sum();
            var combinedInitialCapacity = _mStorages.Select(item => item.InitialCapacity).Sum();
            _mCompositeStorage = (_mStorages[0].Efficiency > 0) ? (IStorage)
                new BasicStorage(Name, _mStorages[0].Efficiency, combinedCapacity, combinedInitialCapacity) :
                new BasicBackup(Name, combinedCapacity);
        }

        public bool Measurering
        {
            get { return _mCompositeStorage.Measurering; }
        }

        public ITimeSeries TimeSeries
        {
            get { return _mCompositeStorage.TimeSeries; }
        }

        public void StartMeasurement()
        {
            _mCompositeStorage.StartMeasurement();
        }

        public void Reset()
        {
            _mCompositeStorage.Reset();
        }

        public string Name
        {
            get
            {
                var compositeName = string.Empty;
                for (int i = 0; i < _mStorages.Count; i++)
                {
                    compositeName += _mStorages[i].Name;
                    if (i < _mStorages.Count - 1) compositeName += " + ";
                }
                return compositeName;
            }
        }

        public double Efficiency
        {
            get { return _mCompositeStorage.Efficiency; }
        }

        public double InitialCapacity
        {
            get { return _mCompositeStorage.InitialCapacity; }
        }

        public double Capacity
        {
            get { return _mCompositeStorage.Capacity; }
        }

        public double Inject(int tick, double amount)
        {
            return _mCompositeStorage.Inject(tick, amount);
        }

        public double Restore(int tick, Response response)
        {
            return _mCompositeStorage.Restore(tick, response);
        }

        public double RemainingCapacity(Response response)
        {
            return _mCompositeStorage.RemainingCapacity(response);
        }

        public void ResetCapacity()
        {
            _mCompositeStorage.ResetCapacity();
        }
    }

}
