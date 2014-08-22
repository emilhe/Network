using System.Collections.Generic;

namespace SimpleNetwork.Interfaces
{
    public interface IDistributionStrategy
    {
            
        /// <summary>
        /// Tolerance per node; e.g. GUROBI has finite precsion, hence the tolerance must be > 0 if GUROBI is used.
        /// </summary>
        double Tolerance { get; }

        /// <summary>
        /// When it is determined HOW the power should be distrubuted, call this method to it DO IT. Depending on implementation,
        /// the flow might be calculated, minimized, traced, etc.
        /// </summary>
        void DistributePower(List<Node> nodes, double[] mismatches, double efficiency, int tick);
        
    }
}
