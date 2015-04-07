﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BusinessLogic.Interfaces;
using Newtonsoft.Json.Linq;
using Utils;

namespace BusinessLogic.Utils
{
    class StorageMap
    {

        private readonly INode[] _mNodes;
        private const double Delta = 1e-5;

        public int Levels { get; private set; }
        public List<double[]> LowLims { get; private set; }
        public List<double[]> HighLims { get; private set; }

        public StorageMap(INode[] nodes)
        {
            _mNodes = nodes;

            Levels = _mNodes.Select(item => item.Storages.Count()).Max();        
            LowLims = new List<double[]>();
            HighLims = new List<double[]>();
            for (int i = 0; i < Levels; i++)
            {
                LowLims.Add(new double[nodes.Length]);
                HighLims.Add(new double[nodes.Length]);
            }
        }

        /// <summary>
        /// Inject the amounts specified. If not possible, an exception is thrown.
        /// </summary>
        /// <param name="values"> List (levels) of storage injections (array = nodes) </param>
        public void Inject(List<double[]> values)
        {
            for (int i = 0; i < Levels; i++)
            {
                for (int j = 0; j < _mNodes.Length; j++)
                {
                    var remainder = _mNodes[j].Storages[i].Inject(values[i][j]);
                    if (Math.Abs(remainder) > Delta) throw new ArgumentException("Charge failure!");
                }
            }
        }

        /// <summary>
        /// Refresh limit vectors.
        /// </summary>
        public void RefreshLims()
        {
            for (int i = 0; i < Levels; i++)
            {
                for (int j = 0; j < _mNodes.Length; j++)
                {
                    LowLims[i][j] = _mNodes[j].Storages[i].AvailableEnergy(Response.Discharge);
                    HighLims[i][j] = _mNodes[j].Storages[i].AvailableEnergy(Response.Charge);
                }
            }
        }




    }
}
