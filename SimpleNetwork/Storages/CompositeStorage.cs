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

        public void Sample(int tick)
        {
            _mCompositeStorage.Sample(tick);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return _mCompositeStorage.CollectTimeSeries();
        }

        public bool Measuring
        {
            get { return _mCompositeStorage.Measuring; }
        }

        public void Start()
        {
            _mCompositeStorage.Start();
        }

        public void Clear()
        {
            _mCompositeStorage.Clear();
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

        public double Inject(double amount)
        {
            return _mCompositeStorage.Inject(amount);
        }

        public double Restore(Response response)
        {
            return _mCompositeStorage.Restore(response);
        }

        public double RemainingCapacity(Response response)
        {
            return _mCompositeStorage.RemainingCapacity(response);
        }

        public void ResetCapacity()
        {
            _mCompositeStorage.ResetCapacity();
        }

        public double LimitIn
        {
            get { return _mCompositeStorage.LimitIn; }
        }

        public double LimitOut
        {
            get { return _mCompositeStorage.LimitOut; }
        }
    }

}
