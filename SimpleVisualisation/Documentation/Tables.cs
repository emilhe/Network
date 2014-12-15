using System.Collections.Generic;
using System.IO;
using System.Linq;
using LaTeX;
using NUnit.Framework;
using Utils;

namespace Main.Documentation
{
    class Tables
    {

        public static void PrintLinkInfo()
        {
            var table = new Table
            {
                Header = new[] {"From", "To", "Type", "Length [km]"},
                Caption = "Link information.",
                Label = "link-info",
                Format = "llcr",
                Rows = new List<string[]>(),
                Injection = @"\small"
            };

            foreach (var link in Costs.LinkLength.Keys.OrderBy(item => item))
            {
                var row = new[]
                {
                    link.Split('-')[0], 
                    link.Split('-')[1], 
                    Costs.LinkType[link], 
                    Costs.LinkLength[link].ToString()
                };
                table.Rows.Add(row);
            }

            File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\linkInfo.tex", table.ToTeX());
        }

        public static void PrintCapacityFactors()
        {
            var table = new Table
            {
                Header = new[] { "", @"$\nu_n^w$", @"$\nu_n^s$", "", "", @"$\nu_n^w$", @"$\nu_n^s$", "", "", @"$\nu_n^w$", @"$\nu_n^s$", "" },
                Caption = @"Capacity factors $\nu_n^w$ and $\nu_n^s$ for wind and solar photovoltaics for the European countries.",
                Label = "capacity-factors",
                Format = "lccclccclccc",
                Rows = new List<string[]>()
            };

            var idx = 0;
            foreach (var country in CountryInfo.GetCountries().OrderBy(item => item))
            {
                var col = idx/10;
                if (col == 0) table.Rows.Add(new string[12]);

                table.Rows[idx%10][col*4 + 0] = country;
                table.Rows[idx % 10][col * 4 + 1] = CountryInfo.GetWindCf(country).ToString("0.00");
                table.Rows[idx % 10][col * 4 + 2] = CountryInfo.GetSolarCf(country).ToString("0.00");
                table.Rows[idx%10][col*4 + 3] = " ";
                
                idx++;
            }

            File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\capacityFactors.tex", table.ToTeX());
        }

        public static void PrintCostInfo()
        {
            var table = new Table
            {
                Header = new[] { "Asset", @"CapEx [€/W]",@"Fixed OpEx [€/kW/year]", @"Variable OpEx [€/MWh]" },
                Caption = @"Cost assumptions for different assets.",
                Label = "cost-assumptions",
                Format = "lrrr",
                Rows = new List<string[]>()
            };

            var assets = new List<Costs.Asset>
            {
                Costs.CCGT,
                Costs.OnshoreWind,
                Costs.OffshoreWind,
                Costs.Solar,
                Costs.Wind
            };

            foreach (var asset in assets.OrderBy(item => item.Name))
            {
                table.Rows.Add(new[] { asset.Name, asset.CapExFixed.ToString("0.0"), asset.OpExFixed.ToString("0.0"), asset.OpExVariable.ToString("0.0") });
            }

            File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\costInfo.tex", table.ToTeX());
        }

    }
}
