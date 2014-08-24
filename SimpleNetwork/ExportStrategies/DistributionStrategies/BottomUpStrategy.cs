using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies.DistributionStrategies
{
    public class BottomUpStrategy : IDistributionStrategy
    {
        public double Tolerance { get { return 1e-6; } }

        public bool ShareStorage { get; set; }

        public void DistributePower(List<Node> nodes, double[] mismatches, double efficiency, int tick)
        {
            // First, try equalising the power so that we only apply storage when needed.
            var toInject = EqualizePower(mismatches);
            if (!ShareStorage && toInject <= 0) return;

            // Inject remaining charge "randomly" (here we just start with node 0).
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

        private double EqualizePower(double[] mismatches)
        {
            var originalMismatch = mismatches.Sum();
            var mismatch = originalMismatch;

            for (int i = 0; i < mismatches.Length; i++)
            {
                if (originalMismatch > 0)
                {
                    if (mismatches[i] < 0) mismatches[i] = 0;
                    else if (mismatches[i] < mismatch) mismatch -= mismatches[i];
                    else
                    {
                        mismatches[i] -= mismatch;
                        mismatch = 0;
                    }
                }
                else if (originalMismatch < 0)
                {
                    if (mismatches[i] > 0) mismatches[i] = 0;
                    else if (mismatches[i] > mismatch) mismatch -= mismatches[i];
                    else
                    {
                        mismatches[i] -= mismatch;
                        mismatch = 0;
                    }
                }
            }

            return originalMismatch;
        }


        //private double EqualizePower(double[] mismatches)
        //{
        //    // Collect the power that is to be distributed.
        //    double toInject = 0;
        //    for (int i = 0; i < mismatches.Length; i++)
        //    {
        //        if (mismatches[i] <= 0) continue;
        //        toInject += mismatches[i];
        //        mismatches[i] = 0;
        //    }
        //    // Distribute it (only negative mismatches "are left").
        //    for (int i = 0; i < mismatches.Length; i++)
        //    {
        //        if (mismatches[i] >= 0) continue;
        //        if (Math.Abs(mismatches[i]) <= toInject)
        //        {
        //            toInject += mismatches[i];
        //            mismatches[i] = 0;
        //        }
        //        else
        //        {
        //            mismatches[i] += toInject;
        //            toInject = 0;
        //            break;
        //        }
        //    }
        //    return toInject;
        //}

    }
}
    