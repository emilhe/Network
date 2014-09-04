using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Interfaces;

namespace BusinessLogic.ExportStrategies.DistributionStrategies
{
    public class SkipFlowStrategy : IDistributionStrategy
    {
        // Tolerance due to finite double precision.
        public double Tolerance
        {
            get { return 1e-10; }
        }

        /// <summary>
        /// Distribute the power. After distribution, all mismatches will be covered by storage.
        /// </summary>
        public void DistributePower(List<Node> nodes, double[] mismatches, double efficiency, int tick)
        {
            var toInject = mismatches.Sum();

            // Inject the charge "randomly" (here we just start with node 0).
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

        /// <summary>
        /// Equalize energy. After call, all entrances will be positive or negative.
        /// </summary>
        public void EqualizePower(double[] mismatches)
        {
            double pos = 0;
            double neg = 0;
            foreach (var mismatch in mismatches)
            {
                if (mismatch > 0) pos += mismatch;
                else neg += mismatch;
            }
            var originalMismatch = pos + neg;
            if (originalMismatch > 0) EqualizePos(mismatches, -neg);
            else EqualizeNeg(mismatches, pos);
        }

        /// <summary>
        /// Subtract the negative contributions from the positive; set all negative elements to 0.
        /// </summary>
        /// <param name="mismatches"> target </param>
        /// <param name="toSubtrack"> neg contributions to subtract </param>
        private void EqualizePos(double[] mismatches, double toSubtrack)
        {
            for (var i = 0; i < mismatches.Length; i++)
            {
                if (mismatches[i] < 0) mismatches[i] = 0;
                else if (mismatches[i] < toSubtrack)
                {
                    toSubtrack -= mismatches[i];
                    mismatches[i] = 0;
                }
                else
                {
                    mismatches[i] -= toSubtrack;
                    toSubtrack = 0;
                }
            }
        }

        /// <summary>
        /// Add the positive contributions to the negative; set all positive elements to 0.
        /// </summary>
        /// <param name="mismatches"> target </param>
        /// <param name="toAdd"> pos contributions to add </param>
        private void EqualizeNeg(double[] mismatches, double toAdd)
        {
            for (var i = 0; i < mismatches.Length; i++)
            {
                if (mismatches[i] > 0) mismatches[i] = 0;
                else if (-mismatches[i] < toAdd)
                {
                    toAdd += mismatches[i];
                    mismatches[i] = 0;
                }
                else
                {
                    mismatches[i] += toAdd;
                    toAdd = 0;
                }
            }
        }

        #region Measurement

        public void StartMeasurement()
        {
            // Nothing to measure.
        }

        public void Reset()
        {
            // Nothing to measure.
        }

        public bool Measurering
        {
            get { return false; }
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            return new List<ITimeSeries>();
        }

        #endregion

    }
}
    