using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace BusinessLogic.LCOE
{
    public static class CostCalculator
    {

        private const double Rate = 4;
        private static double _mAnnualizationFactor;

        private static double AnnualizationFactor
        {
            get
            {
                if (_mAnnualizationFactor == 0) _mAnnualizationFactor = CalcAnnualizationFactor(Rate);
                return _mAnnualizationFactor;
            }
        }

        /// <summary>
        /// System LCOE taking links into consideration (slow to evaluate).
        /// </summary>
        /// <param name="model"> network model </param>
        /// <returns> LCOE </returns>
        public static double SystemCost(NetworkModel model)
        {
            return BaseCosts(model) + BaseCosts(model) + TransmissionCost(model);
        }

        /// <summary>
        /// System LCOE not taking links into consideration (fast to evaluate).
        /// </summary>
        /// <param name="model"> network model </param>
        /// <returns> LCOE </returns>
        public static double SystemCostWithoutLinks(NetworkModel model)
        {
            return BaseCosts(model) + BackupCost(model);
        }

        // Cost of transmission network.
        private static double TransmissionCost(NetworkModel model)
        {
            return 0;
        }

        // Cost of backup facilities.
        private static double BackupCost(NetworkModel model)
        {
            return 0;
        }

        // Cost of wind/solar facilities.
        private static double BaseCosts(NetworkModel model)
        {
            var windCapacity = 0.0;
            var solarCapacity = 0.0;

            foreach (var node in model.Nodes)
            {
                // Pass alpha/gamma values?
                var alpha = 0;
                var gamma = 0;
                // Weighting factor - should be a parameter..
                var weight = node.LoadTimeSeries.GetAverage()*gamma;
                // Calculate capacities.
                windCapacity += alpha*weight/CountryInfo.GetWindCf(node.CountryName);
                solarCapacity += (1 - alpha)*weight/CountryInfo.GetSolarCf(node.CountryName);
            }

            return WindCost(windCapacity) + SolarCost(solarCapacity);
        }

        private static double BackupCost(double capacity, double energy)
        {
            return capacity * (Costs.CCGT.CapExFixed * 1e6 + Costs.CCGT.OpExFixed * 1e3 * AnnualizationFactor) +
                   energy * Costs.CCGT.OpExVariable * AnnualizationFactor;
        }

        private static double WindCost(double capacity)
        {
            return capacity*(Costs.Wind.CapExFixed*1e6 + Costs.Wind.OpExFixed*1e3*AnnualizationFactor);
        }

        private static double SolarCost(double capacity)
        {
            return capacity*(Costs.Solar.CapExFixed*1e6 + Costs.Solar.OpExFixed*1e3*AnnualizationFactor);
        }

        // WTF is this?
        private static double CalcAnnualizationFactor(double rate)
        {
            if (rate == 0) return 30;
            return (1 - Math.Pow((1 + (rate/100.0)), -30))/(rate/100.0);
        }
    }
}
