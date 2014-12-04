using System;
using System.Collections.Generic;

namespace Utils
{
    public static class Costs
    {

        // Source: Optimal heterogeneity of a highly renewable pan-European electricity system + Magnus (e_lineinfo.txt).
        #region Transmission

        // Cost in €/MW/km
        private const double AcCostPerKm = 400;
        private const double DcCostPerKm = 1500;
        // Cost in €/MW
        private const double DcConverter = 150000;
        // Link properties.
        private static readonly Dictionary<string, string> LinkType =
            FileUtils.DictionaryFromFile<string, string>(@"C:\proto\LineType.txt");    
        private static readonly Dictionary<string, int> LinkLength =
            FileUtils.DictionaryFromFile<string, int>(@"C:\proto\LineLength.txt");

        /// <summary>
        /// Get the cost of a link.
        /// </summary>
        /// <param name="from"> start country </param>
        /// <param name="to"> end country </param>
        /// <param name="diffAcDc"> differentiate dc and ac links </param>
        /// <returns> link cost per MW </returns>
        public static double GetLinkCost(string from, string to, bool diffAcDc = false)
        {
            var key = GetKey(from, to);
            if (!diffAcDc) return LinkLength[key] * AcCostPerKm;

            if (!LinkType.ContainsKey(key)) throw new ArgumentException("Link type not found: " + key);
            if (LinkType[key].Equals("AC")) return LinkLength[key]*AcCostPerKm;
            if (LinkType[key].Equals("DC")) return LinkLength[key]*DcCostPerKm + 2*DcConverter;

            throw new ArgumentException("Unknown link type.");
        }

        /// <summary>
        /// Get the length of a link.
        /// </summary>
        /// <param name="from"> start country </param>
        /// <param name="to"> end country </param>
        /// <returns> link length in km </returns>
        public static double GetLinkLength(string from, string to)
        {
            return LinkLength[GetKey(from, to)];
        }

        private static string GetKey(string from, string to)
        {
            string key = null;
            var key1 = string.Format("{0}-{1}", from, to);
            var key2 = string.Format("{1}-{0}", from, to);
            if (LinkLength.ContainsKey(key1)) key = key1;
            if (LinkLength.ContainsKey(key2)) key = key2;
            if (key == null) throw new ArgumentException("Link not found: " + key1);
            return key;
        }

        #endregion


        // Source: Rolando PHD thesis, table 4.1, page 109.
        #region Assets

        public static Asset OnshoreWind = new Asset
        {
            Name = "Wind – onshore",
            CapExFixed = 1.0,
            OpExFixed = 15,
            OpExVariable = 0.0
        };

        public static Asset OffshoreWind = new Asset
        {
            Name = "Wind – offshore",
            CapExFixed = 2.0,
            OpExFixed = 55,
            OpExVariable = 0.0
        };

        public static Asset Wind = new Asset
        {
            Name = "Wind – 50/50 mix",
            CapExFixed = 1.5,
            OpExFixed = 35,
            OpExVariable = 0.0
        };

        public static Asset Solar = new Asset
        {
            Name = "Solar photovoltaic",
            CapExFixed = 1.5,
            OpExFixed = 8.5,
            OpExVariable = 0.0
        };

        public static Asset CCGT = new Asset
        {
            Name = "Solar photovoltaic",
            CapExFixed = 0.90,
            OpExFixed = 4.5,
            OpExVariable = 56.0
        };

        #endregion

        public class Asset
        {
            public string Name { get; set; }
            // Euros/W
            public double CapExFixed { get; set; }
            // Euros/kW/year            
            public double OpExFixed { get; set; }
            // Euros/MWh/year                        
            public double OpExVariable { get; set; }
        }

    }
}
