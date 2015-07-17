using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Cost.Transmission;
using Utils;

namespace BusinessLogic.Cost.CostModels
{

    public class VariableLengthModel : ITransmissionCostModel
    {

        public double Scale { get; set; }

        public VariableLengthModel()
        {
            Scale = 0.5;
            //Scale = 1;
        }

        public double Eval(Dictionary<string, double> transmissionMap)
        {
            return transmissionMap.Sum(link => link.Value*GetLinkCost(link.Key)*Scale);
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
