﻿using System.Collections.Generic;
using BusinessLogic.Interfaces;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

namespace BusinessLogic.Storages
{
    /// <summary>
    /// Backup (non rechargeable).
    /// </summary>
    public class BasicBackup : IStorage
    {
        private readonly BasicStorage _mCore;

        public BasicBackup(string name, double capacity)
        {
            _mCore = new BasicStorage(name, 1, capacity, capacity);
        }

        public void Sample(int tick)
        {
             _mCore.Sample(tick);
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return _mCore.CollectTimeSeries();
        }

        public bool Measuring
        {
            get { return _mCore.Measuring; }
        }

        public void Start()
        {
            ((IMeasureable) _mCore).Start();
        }

        public void Clear()
        {
            _mCore.Clear();
        }

        public string Name
        {
            get { return _mCore.Name; }
        }

        public double Efficiency
        {
            // A backup cannot be charged.
            get { return 0; }
        }

        public double InitialEnergy
        {
            get { return _mCore.InitialEnergy; }
        }

        public double NominalEnergy
        {
            get { return _mCore.NominalEnergy; }
        }

        public double Inject(double amount)
        {
            // Only negative energy (discharge) can be injected.
            return amount > 0 ? amount : ((IStorage)_mCore).Inject(amount);
        }

        public double InjectMax(Response response)
        {
            // A backup cannot be charged.
            return (response == Response.Charge) ? 0 : ((IStorage)_mCore).InjectMax(response);
        }

        public double RemainingEnergy(Response response)
        {
            // A backup cannot be charged.
            return response == Response.Charge ? 0 : ((IStorage)_mCore).RemainingEnergy(response);
        }

        public double AvailableEnergy(Response response)
        {
            // A backup cannot be charged.
            return response == Response.Charge ? 0 : ((IStorage)_mCore).AvailableEnergy(response);
        }

        public void ResetEnergy()
        {
            ((IStorage)_mCore).ResetEnergy();
        }

        public double Capacity
        {
            get { return _mCore.Capacity; }
            set { _mCore.Capacity = value; }
        }

    }
}