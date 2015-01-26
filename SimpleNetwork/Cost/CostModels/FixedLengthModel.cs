using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace BusinessLogic.Cost.Transmission
{
    public class FixedLengthModel : ITransmissionCostModel
    {
        public double Eval(Dictionary<string, double> transmissionMap)
        {
            return transmissionMap.Sum(link => link.Value * GetLinkCost(link.Key));
        }

        /// <summary>
        /// Get the cost of a link using fixed length (100 km) model.
        /// </summary>
        /// <param name="key"> key </param>
        /// <returns> link cost per MW </returns>
        public static double GetLinkCost(string key)
        {
            if (!Costs.LinkType.ContainsKey(key)) throw new ArgumentException("Link type not found: " + key);

            if (Costs.LinkType[key].Equals("AC")) return 100 * Costs.AcCostPerKm;
            if (Costs.LinkType[key].Equals("DC")) return 100 * Costs.DcCostPerKm + 2 * Costs.DcConverterCost;

            throw new ArgumentException("Unknown link type.");
        }
    }
}
