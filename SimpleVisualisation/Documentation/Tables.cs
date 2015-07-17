using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Controls;
using LaTeX;
using NUnit.Framework;
using SimpleImporter;
using Utils;

namespace Main.Documentation
{
    class Tables
    {

        public static void PrintLinkInfo(int cols)
        {
            var header = new List<String>();
            var format = new StringBuilder();
            var units = new List<String>();
            for (int j = 0; j < cols; j++)
            {
                //header.AddRange(new[] { "From", "To", "Type", "Length [km]" });
                header.AddRange(new[] { "From", "To", "Length" });
                //header.AddRange(new[] { "From-To", "Length" });
                //format.Append("llcr");
                format.Append("llr");
                units.AddRange(new[] { "", "", "[km]" });
                //units.AddRange(new[] { "", "[km]" });
            }
            var table = new Table
            {
                Header = header.ToArray(),
                Caption = "Approximated link lengths.",
                Label = "link-info",
                Format = format.ToString(),
                Rows = new List<string[]>(),
                Units = units.ToArray()
                //Injection = @"\small"
            };
            int i = 0;
            var row = new List<String>();
            foreach (var link in Costs.LinkLength.Keys.OrderBy(item => item))
            {
                if (i >= cols)
                {
                    table.Rows.Add(row.ToArray());
                    row = new List<String>();
                    i = 0;
                }
                var subRow = new[]
                {
                    CountryInfo.GetShortAbbrev(link.Split('-')[0]),// + "-" +
                    CountryInfo.GetShortAbbrev(link.Split('-')[1]),
                    //Costs.LinkType[link],
                    Costs.LinkLength[link].ToString()
                };
                row.AddRange(subRow);
                i++;
            }
            table.Rows.Add(row.ToArray());

            File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\linkInfo.tex", table.ToTeX());
        }

        public static void PrintCostInfo()
        {
            var table = new Table
            {
                Header = new[] { "Asset", @"CapEx", @"Fixed OpEx", @"Variable OpEx" },
                Units = new[] { "", @"[€/W]", @"[€/kW/year]", @"[€/MWh]" },
                Caption = @"Cost assumptions for different assets.",
                Label = "cost-assumptions",
                Format = "lrrr",
                Rows = new List<string[]>(),
            };

            var assets = new List<Costs.Asset>
            {
                Costs.CCGT,
                Costs.OnshoreWind,
                Costs.OffshoreWind,
                Costs.Solar,
            };

            foreach (var asset in assets.OrderBy(item => item.Name))
            {
                table.Rows.Add(new[] { asset.Name, asset.CapExFixed.ToString("0.0"), asset.OpExFixed.ToString("0.0"), asset.OpExVariable.ToString("0.0") });
            }

            File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\costInfo.tex", table.ToTeX());
        }

        public static void PrintReducedSolarAlphas()
        {
            var results = FileUtils.FromJsonFile<Dictionary<string, Dictionary<double, BetaWrapper>>>
                (@"C:\Users\Emil\Dropbox\BACKUP\Python\solar\solarAnalysis.txt");
            
            var table = new StringBuilder();
            foreach (var result in results)
            {
                var ks = result.Value.Keys.OrderBy(item => item).ToArray();
                var betaOpt = new double[ks.Length];
                var maxCfOpt = new double[ks.Length];
                var geneticOpt = new double[ks.Length];
                var idx = 0;
                foreach (var k in ks)
                {
                    var item = result.Value[k];
                    betaOpt[idx] = item.BetaX[Array.IndexOf(item.BetaY, item.BetaY.Min())];
                    maxCfOpt[idx] = item.MaxCfX[Array.IndexOf(item.MaxCfY, item.MaxCfY.Min())];
                    geneticOpt[idx] = item.GeneticX;
                    idx++;
                }
                // Create tabular stuff.
                var builder = new StringBuilder();
                builder.Append((double.Parse(result.Key)*100).ToString("0") + @"\%");
                for (int i = 0; i < ks.Length; i++)
                {
                    builder.Append(" & " + betaOpt[i].ToString("0.00") + " & " + maxCfOpt[i].ToString("0.00") + " & " + geneticOpt[i].ToString("0.00"));
                }
                builder.Append(@"\\");
                //foreach (var k in ks) builder.Append(" & K = " + k);
                //builder.AppendLine(@" \\ \hline");
                //builder.Append(@"$\beta$");
                //foreach (var b in betaOpt) builder.Append(" & " + b.ToString("0.##"));
                //builder.AppendLine(@" \\ ");
                //builder.Append(@"CF");
                //foreach (var c in maxCfOpt) builder.Append(" & " + c.ToString("0.##"));
                //builder.AppendLine(@" \\ ");
                //builder.Append(@"GAS");
                //foreach (var g in geneticOpt) builder.Append(" & " + g.ToString("0.##"));
                //builder.AppendLine(@" \\ \hline");
                //File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\solarScale" + result.Key + ".tex", builder.ToString());
                table.AppendLine(builder.ToString());
            }
            File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\solarScale.tex", table.ToString());
        }

        public static void PrintCapacityFactors()
        {
            PrintCapacityFactors(CountryInfo.WindOnshoreCf, CountryInfo.WindOffshoreCf, CountryInfo.SolarCf, "capacityFactors.tex");
        }

        private static void PrintCapacityFactors(Dictionary<string, double> cfOnW, Dictionary<string, double> cfOffW, Dictionary<string, double> cfS, string name)
        {
            var table = new Table
            {
                Header =
                    new[]
                    {
                        "", @"$\nu_n^w$", @"$\tilde{\nu}_n^w$", @"$\nu_n^s$", "", @"$\nu_n^w$", @"$\tilde{\nu}_n^w$", @"$\nu_n^s$", "",
                        @"$\nu_n^w$", @"$\tilde{\nu}_n^w$", @"$\nu_n^s$"
                    },
                Caption =
                    @"Capacity factors $\nu_n^w$, $\tilde{\nu}_n^w$ and  $\nu_n^s$ for onshore wind, offshore wind and solar PV for the European countries.",
                Label = "capacity-factors",
                Format = "lccclccclccc",
                Rows = new List<string[]>(),
            };

            var idx = 0;
            foreach (var country in CountryInfo.GetCountries().OrderBy(item => item))
            {
                var col = idx / 10;
                if (col == 0) table.Rows.Add(new string[12]);

                table.Rows[idx % 10][col * 4 + 0] = CountryInfo.GetShortAbbrev(country);
                table.Rows[idx % 10][col * 4 + 1] = (cfOnW[country] < 1e-3)? "-" : cfOnW[country].ToString("0.00");
                table.Rows[idx % 10][col * 4 + 2] = (cfOffW[country] < 1e-3) ? "-" : cfOffW[country].ToString("0.00");
                table.Rows[idx % 10][col * 4 + 3] = (cfS[country] < 1e-3) ? "-" : cfS[country].ToString("0.00");
                //table.Rows[idx % 10][col * 5 + 4] = " ";

                idx++;
            }

            File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\" + name, table.ToTeX());
        }

        public static void PrintBiomassData()
        {
            const int year = 2010;
            const string type = "Biomass";
            var data = ProtoStore.LoadEcnData();
            var energy = data.Where(item =>
                item.RowHeader.Equals(type) &&
                item.ColumnHeader.Equals("Gross electricity generation") &&
                item.Year.Equals(year)).ToArray();
            var capacity = data.Where(item =>
            item.RowHeader.Equals(type) &&
            item.ColumnHeader.Equals("Installed capacity") &&
            item.Year.Equals(year)).ToArray();

            var table = new Table
            {
                Header =
                    new[]
                    {
                        "", @"$\mathcal{K}^{-}$", @"Prod/y", "",@"$\mathcal{K}^{-}$", @"Prod/y", "",
                        @"$\mathcal{K}^{-}$", @"Prod/y",
                    },
                Caption =
                    @"Hest.",
                Label = "capacity-factors",
                Format = "lcclcclcc",
                Rows = new List<string[]>(),
            };

            var idx = 0;
            foreach (var country in CountryInfo.GetCountries().OrderBy(item => item))
            {
                var values = new string[2];
                var eMatch = energy.SingleOrDefault(item => item.Country.Equals(country));
                var cMatch = capacity.SingleOrDefault(item => item.Country.Equals(country));
                if (eMatch == null || cMatch == null)
                {
                    values[0] = "\\color{red} 0.00";
                    values[1] = "\\color{red} 0.00";
                }
                else
                {
                    // We have a match, let's add the backup.
                    var hourly = eMatch.Value / (365 * 24);
                    var capFuck = (hourly > 1e-2 && hourly*1000 > cMatch.Value);
                    var cap = capFuck ? hourly*2 : cMatch.Value/1000;
                    values[0] = hourly.ToString("0.00");
                    values[1] = (capFuck? "\\color{red}" : "") + cap.ToString("0.00");
                }

                var col = idx / 10;
                if (col == 0) table.Rows.Add(new string[9]);

                table.Rows[idx % 10][col * 3 + 0] = CountryInfo.GetShortAbbrev(country);
                table.Rows[idx%10][col*3 + 1] = values[1];
                table.Rows[idx%10][col*3 + 2] = values[0];

                idx++;
            }

            File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\biomass.tex", table.ToTeX());
        }

        public static void PrintHydroData()
        {
            var data = FileUtils.FromJsonFile<Dictionary<string, HydroInfo>>(@"C:\Users\Emil\Dropbox\Master Thesis\HydroDataExtended2005Kies.txt");
            var table = new Table
            {
                Header =
                    new[]
                    {
                        "", @"$\mathcal{K}^{-}$", @"$\mathcal{K}^{-}$", @"$E^S$", "Hourly inflow (mean)", "",
                        @"$\mathcal{K}^{-}$", @"$\mathcal{K}^{-}$", @"$E^S$", "Hourly inflow (mean)",
                    },
                Caption =
                    @"Hest.",
                Label = "capacity-factors",
                Format = "lcccclcccc",
                Rows = new List<string[]>(),
            };

            var idx = 0;
            foreach (var country in CountryInfo.GetCountries().OrderBy(item => item))
            {
                var col = idx / 15;
                if (col == 0) table.Rows.Add(new string[10]);
                table.Rows[idx % 15][col * 5 + 0] = CountryInfo.GetShortAbbrev(country);

                if (data.ContainsKey(country))
                {
                    table.Rows[idx%15][col*5 + 1] = (data[country].Capacity/1000).ToString("0.00");
                    table.Rows[idx%15][col*5 + 2] = (data[country].PumpCapacity/1000).ToString("0.00");
                    table.Rows[idx%15][col*5 + 3] = data[country].ReservoirCapacity.ToString("0.00");
                    table.Rows[idx%15][col*5 + 4] = (data[country].InflowPattern.Average()/24).ToString("0.00");
                }
                else
                {
                    table.Rows[idx%15][col*5 + 1] = "N/A";
                    table.Rows[idx%15][col*5 + 2] = "N/A";
                    table.Rows[idx%15][col*5 + 3] = "N/A";
                    table.Rows[idx%15][col*5 + 4] = "N/A";
                }

                idx++;
            }

            File.WriteAllText(@"C:\Users\Emil\Dropbox\Master Thesis\Tables\hydroExtended.tex", table.ToTeX());
        }

        public static void PrintOverviewTable(Dictionary<string, Dictionary<double, BetaWrapper>> blob)
        {
            var b1 = new StringBuilder();
            var b2 = new StringBuilder();
            b1.Append("LCOE & [\\euro] & ");
            b2.Append("Savings & [\\%] & ");
            // Extract info form blob.
            var data = blob["LCOE"];
            var reference = 0.0;
            for (int k = 1; k < 4; k++)
            {
                var subData = data[k];
                var beta = subData.BetaY.Min();
                var cf = subData.MaxCfY.Min();
                var gen = subData.GeneticY;
                if (k == 1) reference = beta;
                b1.Append(string.Format("{0} & {1} & {2} & ",
                    ThreeSignificantDigits(beta),
                    ThreeSignificantDigits(cf),
                    ThreeSignificantDigits(gen)));
                b2.Append(string.Format("{0} & {1} & {2} & ", 
                    RelPct(reference, beta), 
                    RelPct(reference, cf),
                    RelPct(reference, gen)));
            }
            var str1 = b1.ToString();
            str1 = str1.Substring(0, str1.Length - 2) + @"\\";
            Console.WriteLine(str1);
            var str2 = b2.ToString();
            str2 = str2.Substring(0, str2.Length - 2) + @"\\";
            Console.WriteLine(str2);
        }

        private static string RelPct(double reference, double d)
        {
            return ThreeSignificantDigits((reference - d)/reference*100);
        }

        private static string ThreeSignificantDigits(double d)
        {
            return d.ToString(d < 10 ? "0.00" : "0.0");
        }
    }
}
