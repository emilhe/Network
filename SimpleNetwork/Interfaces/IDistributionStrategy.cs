﻿using System.Collections.Generic;
using BusinessLogic.Nodes;

namespace BusinessLogic.Interfaces
{
    public interface IDistributionStrategy : IMeasureable
    {
            
        /// <summary>
        /// Tolerance per node; e.g. GUROBI has finite precsion, hence the tolerance must be > 0 if GUROBI is used.
        /// </summary>
        double Tolerance { get; }

        /// <summary>
        /// Distribute power. This includes chargeing/discharge storage if necessary.
        /// </summary>
        void DistributePower(List<INode> nodes, double[] mismatches, double efficiency);

        /// <summary>
        /// Equalize power. This does NOT include charging/discharging storage.
        /// </summary>
        void EqualizePower(double[] mismatches);

    }
}