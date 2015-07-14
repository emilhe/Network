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
            var combinedCapacity = _mStorages.Select(item => item.NominalEnergy).Sum();
            var combinedInitialCapacity = _mStorages.Select(item => item.InitialEnergy).Sum();
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

        public void Start(int ticks)
        {
            _mCompositeStorage.Start(ticks);
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

        public double InitialEnergy
        {
            get { return _mCompositeStorage.InitialEnergy; }
        }

        public double NominalEnergy
        {
            get { return _mCompositeStorage.NominalEnergy; }
        }

        public double Inject(double amount)
        {
            return _mCompositeStorage.Inject(amount);
        }

        public double InjectMax(Response response)
        {
            return _mCompositeStorage.InjectMax(response);
        }

        public double RemainingEnergy(Response response)
        {
            return _mCompositeStorage.RemainingEnergy(response);
        }

        public double AvailableEnergy(Response response)
        {
            return _mCompositeStorage.AvailableEnergy(response);
        }

        public void ResetEnergy()
        {
            _mCompositeStorage.ResetEnergy();
        }

        public double Capacity
        {
            get { return _mCompositeStorage.Capacity; }
        }

        public double ChargeLevel
        {
            get { return _mCompositeStorage.ChargeLevel; }
        }

        public void TickChanged(int tick)
        {
            _mCompositeStorage.TickChanged(tick);
        }
    }

}
