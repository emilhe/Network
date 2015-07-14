using System;
using System.Collections.Generic;

namespace Utils
{
    public static class Costs
    {

        public static bool Unsafe { get; set; }

        // Source: Optimal heterogeneity of a highly renewable pan-European electricity system + Magnus (e_lineinfo.txt).
        #region Transmission

        // Cost in €/MW/km
        public const double AcCostPerKm = 400;
        public const double DcCostPerKm = 1500;
        // Cost in €/MW
        public const double DcConverterCost = 150000;
        // Life time of links in years.
        public const double LinkLifeTime = 40; 
        // Link properties.
        public static readonly Dictionary<string, string> LinkType =
            FileUtils.DictionaryFromFile<string, string>(@"C:\EmherSN\data\LineType.txt");
        public static readonly Dictionary<string, int> LinkLength =
            FileUtils.DictionaryFromFile<string, int>(@"C:\EmherSN\data\LineLength.txt");

        public static string GetKey(string from, string to)
        {
            var key1 = string.Format("{0}-{1}", from, to);
            var key2 = string.Format("{1}-{0}", from, to);
            if (LinkLength.ContainsKey(key1)) return key1;
            if (LinkLength.ContainsKey(key2)) return key2;
            if (Unsafe) return key1;         
            throw new ArgumentException("Link not found: " + key1);
        }

        #endregion

        // Source: Rolando code.
        #region Annualization factor

        private const double Rate = 4;

        public static double AnnualizationFactor(double lifetime)
        {
            if (Rate == 0) return lifetime;
            return (1 - Math.Pow((1 + (Rate / 100.0)), -lifetime)) / (Rate / 100.0);
        }

        #endregion

        // Source: Rolando PHD thesis, table 4.1, page 109.
        #region Assets

        public static Asset OnshoreWind = new Asset
        {
            Name = "Wind – onshore",
            CapExFixed = 1.0,
            OpExFixed = 15,
            OpExVariable = 0.0,
            Lifetime = 25
        };

        public static Asset OffshoreWind = new Asset
        {
            Name = "Wind – offshore",
            CapExFixed = 2.0,
            OpExFixed = 55,
            OpExVariable = 0.0,
            Lifetime = 20
        };

        //public static Asset Wind = new Asset
        //{
        //    Name = "Wind – 50/50 mix",
        //    CapExFixed = 1.5,
        //    OpExFixed = 35,
        //    OpExVariable = 0.0
        //};

        public static Asset Solar = new Asset
        {
            Name = "Solar photovoltaic",
            CapExFixed = 0.75,
            OpExFixed = 8.5,
            OpExVariable = 0.0,
            Lifetime = 25
        };

        public static Asset CCGT = new Asset
        {
            Name = "CCGT",
            CapExFixed = 0.90,
            OpExFixed = 4.5,
            OpExVariable = 56.0,
            Lifetime = 30
        };

        public class Asset
        {
            public string Name { get; set; }
            // Euros/W
            public double CapExFixed { get; set; }
            // Euros/kW/year            
            public double OpExFixed { get; set; }
            // Euros/MWh/year                        
            public double OpExVariable { get; set; }
            // Life time in years
            public double Lifetime { get; set; }
        }

        #endregion

    }
}
