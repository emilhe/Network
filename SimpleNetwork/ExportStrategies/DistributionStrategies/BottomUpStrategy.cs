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
            for (int index = 0; index < nodes.Count; index++)
            {
                toInject = nodes[index].Storages.Single(item => item.Efficiency.Equals(efficiency)).Inject(tick, toInject);
                // This should be true after all "transfers complete".
                mismatches[index] = 0;
            }
            // Inject any remaining mismatch in node 0; since flows are not considered, it does not matter.
            mismatches[0] = toInject;
        }

    }
}
