namespace Utils
{
    public static class Costs
    {

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
