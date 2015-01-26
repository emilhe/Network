using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace BusinessLogic.Cost.Transmission
{

    public class VariableLengthModel : ITransmissionCostModel
    {
        public double Eval(Dictionary<string, double> transmissionMap)
        {
            return transmissionMap.Sum(link => link.Value * GetLinkCost(link.Key));
        }

        /// <summary>
        /// Get the cost of a link using length dependent model.
        /// </summary>
        /// <param name="key"> key </param>
        /// <returns> link cost per MW </returns>
        public static double GetLinkCost(string key)
        {
            if (!Costs.LinkType.ContainsKey(key)) throw new ArgumentException("Link type not found: " + key);

            if (Costs.LinkType[key].Equals("AC")) return Costs.LinkLength[key] * Costs.AcCostPerKm;
            if (Costs.LinkType[key].Equals("DC")) return Costs.LinkLength[key] * Costs.DcCostPerKm + 2 * Costs.DcConverterCost;

            throw new ArgumentException("Unknown link type.");
        }
    }

}
