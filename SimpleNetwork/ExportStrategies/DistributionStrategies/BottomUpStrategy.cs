using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies.DistributionStrategies
{
    public class BottomUpStrategy : IDistributionStrategy
    {
        public double Tolerance { get { return 0; } }

        public void DistributePower(List<Node> nodes, double[] mismatches, double efficiency, int tick)
        {
            // Inject remaining charge "randomly" (here we just start with node 0).
            var toInject = mismatches.Sum();
            for (int idx = 0; idx < nodes.Count; idx++)
            {
                if (!nodes[idx].StorageCollection.Contains(efficiency)) continue;
                toInject = nodes[idx].StorageCollection.Get(efficiency).Inject(tick, toInject);
            }
            // Distribute the remaining mismatches "randomly" if any.
            for (int idx = 0; idx < nodes.Count; idx++)
            {
                mismatches[idx] = 0;
                if (idx == 0) mismatches[idx] = toInject;
            }
        }

    }
}
