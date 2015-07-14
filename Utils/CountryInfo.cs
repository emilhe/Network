using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Utils
{
    public class CountryInfo
    {

        #region Country code mappings

        // Source: Wiki
        private static readonly Dictionary<string, string> CountryCodeMap2 = new Dictionary<string, string>
        {
            {"SE", "Sweden"},
            {"SI", "Slovenia"},
            {"SK", "Slovakia"},
            {"RS", "Serbia"},
            {"RO", "Romania"},
            {"PT", "Portugal"},
            {"PL", "Poland"},
            {"NO", "Norway"},
            {"NL", "Netherlands"},
            {"LV", "Latvia"},
            {"LU", "Luxembourg"},
            {"LT", "Lithuania"},
            {"IT", "Italy"},
            {"IE", "Ireland"},
            {"HU", "Hungary"},
            {"HR", "Croatia"},
            {"GR", "Greece"},
            {"EL", "Greece"},
            {"GB", "Great Britain"},
            {"UK", "Great Britain"},
            {"FR", "France"},
            {"FI", "Finland"},
            {"EE", "Estonia"},
            {"ES", "Spain"},
            {"DK", "Denmark"},
            {"DE", "Germany"},
            {"CZ", "Czech Republic"},
            {"CH", "Switzerland"},
            {"BA", "Bosnia"},
            {"BG", "Bulgaria"},
            {"BE", "Belgium"},
            {"AT", "Austria"},
            {"CY", "Cyprus"},
            {"MT", "Malta"},
        };

        // Source: Wiki
        private static readonly Dictionary<string, string> CountryCodeMap3 = new Dictionary<string, string>
        {
            {"SWE", "Sweden"},
            {"SVN", "Slovenia"},
            {"SVK", "Slovakia"},
            {"SRB", "Serbia"},
            {"ROU", "Romania"},
            {"PRT", "Portugal"},
            {"POL", "Poland"},
            {"NOR", "Norway"},
            {"NLD", "Netherlands"},
            {"LVA", "Latvia"},
            {"LUX", "Luxembourg"},
            {"LTU", "Lithuania"},
            {"ITA", "Italy"},
            {"IRL", "Ireland"},
            {"HUN", "Hungary"},
            {"HRV", "Croatia"},
            {"GRC", "Greece"},
            {"GBR", "Great Britain"},
            {"FRA", "France"},
            {"FIN", "Finland"},
            {"EST", "Estonia"},
            {"ESP", "Spain"},
            {"DNK", "Denmark"},
            {"DEU", "Germany"},
            {"CZE", "Czech Republic"},
            {"CHE", "Switzerland"},
            {"BIH", "Bosnia"},
            {"BGR", "Bulgaria"},
            {"BEL", "Belgium"},
            {"AUT", "Austria"},
            {"CYP", "Cyprus"},
            {"MLT", "Malta"},
        };

        private static readonly Dictionary<string, string> CountryCodeMapName = new Dictionary<string, string>
        {
            {"Sweden", "SWE"},
            {"Slovenia", "SVN"},
            {"Slovakia", "SVK"},
            {"Serbia", "SRB"},
            {"Romania", "ROU"},
            {"Portugal", "PRT"},
            {"Poland", "POL"},
            {"Norway", "NOR"},
            {"Netherlands", "NLD"},
            {"Latvia", "LVA"},
            {"Luxembourg", "LUX"},
            {"Lithuania", "LTU"},
            {"Italy", "ITA"},
            {"Ireland", "IRL"},
            {"Hungary", "HUN"},
            {"Croatia", "HRV"},
            {"Greece", "GRC"},
            {"Great Britain", "GBR"},
            {"France", "FRA"},
            {"Finland", "FIN"},
            {"Estonia", "EST"},
            {"Spain", "ESP"},
            {"Denmark", "DNK"},
            {"Germany", "DEU"},
            {"Czech Republic", "CZE"},
            {"Switzerland", "CHE"},
            {"Bosnia", "BIH"},
            {"Bulgaria", "BGR"},
            {"Belgium", "BEL"},
            {"Austria", "AUT"},
            //{"Cyprus", "CYP"},
            //{"Malta", "MLT"},
        };

        #endregion

        //#region Capacity factor mappings: Rolando (ISET DATA)

        //// Source: Optimal heterogeneity of a highly renewable pan-European electricity system
        //private static readonly Dictionary<string, double> WindOnshoreCf = new Dictionary<string, double>
        //{
        //    {"Germany", 0.36},
        //    {"France", 0.32},
        //    {"Great Britain", 0.15},
        //    {"Italy", 0.23},
        //    {"Spain", 0.25},
        //    {"Sweden", 0.14},
        //    {"Poland", 0.15},
        //    {"Norway", 0.13},
        //    {"Netherlands", 0.15},
        //    {"Belgium", 0.14},
        //    {"Finland", 0.27},
        //    {"Czech Republic", 0.21},
        //    {"Austria", 0.19},
        //    {"Greece", 0.14},
        //    {"Romania", 0.15},
        //    {"Bulgaria", 0.15},
        //    {"Portugal", 0.22},
        //    {"Switzerland", 0.14},
        //    {"Hungary", 0.19},
        //    {"Denmark", 0.41},
        //    {"Serbia", 0.15},
        //    {"Ireland", 0.46},
        //    {"Bosnia", 0.16},
        //    {"Slovakia", 0.17},
        //    {"Croatia", 0.18},
        //    {"Lithuania", 0.30},
        //    {"Estonia", 0.28},
        //    {"Slovenia", 0.15},
        //    {"Latvia", 0.19},
        //    {"Luxembourg", 0.35},
        //};

        //// Source: Optimal heterogeneity of a highly renewable pan-European electricity system
        //private static readonly Dictionary<string, double> SolarCf = new Dictionary<string, double>
        //{
        //    {"Germany", 0.15},
        //    {"France", 0.20},
        //    {"Great Britain", 0.15},
        //    {"Italy", 0.23},
        //    {"Spain", 0.25},
        //    {"Sweden", 0.14},
        //    {"Poland", 0.15},
        //    {"Norway", 0.13},
        //    {"Netherlands", 0.15},
        //    {"Belgium", 0.14},
        //    {"Finland", 0.13},
        //    {"Czech Republic", 0.16},
        //    {"Austria", 0.17},
        //    {"Greece", 0.24},
        //    {"Romania", 0.20},
        //    {"Bulgaria", 0.22},
        //    {"Portugal", 0.23},
        //    {"Switzerland", 0.17},
        //    {"Hungary", 0.18},
        //    {"Denmark", 0.15},
        //    {"Serbia", 0.19},
        //    {"Ireland", 0.13},
        //    {"Bosnia", 0.21},
        //    {"Slovakia", 0.17},
        //    {"Croatia", 0.20},
        //    {"Lithuania", 0.14},
        //    {"Estonia", 0.13},
        //    {"Slovenia", 0.18},
        //    {"Latvia", 0.13},
        //    {"Luxembourg", 0.14},
        //};

        //#endregion

        #region Capacity factors: Extracted from VE by EMHER.

        public static readonly Dictionary<string, double> WindOnshoreCf =
            FileUtils.DictionaryFromFile<string, double>(@"C:\EmherSN\data\windCf_50pct_onshore.txt")
                .Where(item => CountryCodeMap3.ContainsKey(item.Key))
                .ToDictionary(item => GetName(item.Key), item => item.Value);

        public static readonly Dictionary<string, double> WindOffshoreCf =
            FileUtils.DictionaryFromFile<string, double>(@"C:\EmherSN\data\windCf_50pct_offshore.txt")
                .Where(item => CountryCodeMap3.ContainsKey(item.Key))
                .ToDictionary(item => GetName(item.Key), item => item.Value);

        public static readonly Dictionary<string, double> SolarCf =
            FileUtils.DictionaryFromFile<string, double>(@"C:\EmherSN\data\solarCf_50pct_onshore.txt")
                .Where(item => CountryCodeMap3.ContainsKey(item.Key))
                .ToDictionary(item => GetName(item.Key), item => item.Value);

        #endregion

        #region Mean load mappings

        // Source: 32 years of VE data.
        private static readonly Dictionary<string, double> MeanLoad =
            FileUtils.DictionaryFromFile<string, double>(@"C:\EmherSN\data\MeanLoad.txt");

        #endregion

        #region Offshore mappings

        public static Dictionary<string, double> OffshoreFrations(double scale = 0.5)
        {
            return new Dictionary<string, double>()
            {
                {"Denmark", scale},
                {"Germany", scale},
                {"Great Britain", scale},
                {"Ireland", scale},
                {"Netherlands", scale},
                {"France", scale},
                {"Belgium", scale},
                {"Norway", scale},
                {"Sweden", scale},
            };
        }

        #endregion

        #region Tradewind hydro mappings

        public static Dictionary<string, double> ReservoirCapacities = new Dictionary<string, double>
        {
            // Data source: Tradewind (total reservoir capacity in TWh, 2005)
            {"Germany", 0.30},
            {"Belgium", 0.03},
            {"Luxemborg", 0.03},
            {"France", 9.80},
            {"Switzerland", 8.60},
            {"Italy", 7.90},
            {"Austria", 3.20},
            {"Spain", 18.40},
            {"Norway", 82.00},
            {"Sweden", 28.00},
            {"Czech Republic", 0.54},
            {"Slovenia", 0},
            {"Greece", 2.40},
            {"Great Britain", 1.20},
            {"Portugal", 2.60},
            {"Croatia", 1.44},
            {"Serbia", 2.00},
            {"Romania", 4.30},
            {"Bulgaria", 0.98},
            {"Bosnia", 1.44},
            {"Slovakia", 0.63},
            {"Poland", 0.41},
            {"Finland", 5.00},
            {"Ireland", 0.24}
        };

        public static Dictionary<string, double> PumpCapacities = new Dictionary<string, double>
        {
            // Data source: Tradewind (pump capacity in GW, 2005)
            {"Germany", 3.8},
            {"Belgium", 1.3},
            {"Luxemborg", 1.1},
            {"France", 4.3},
            {"Switzerland", 1.6},
            {"Italy", 4.2},
            {"Austria", 2.9},
            {"Spain", 3.3},
            {"Norway", 0},
            {"Sweden", 0},
            {"Czech Republic", 1.1},
            {"Slovenia", 0},
            {"Greece", 0.7},
            {"Great Britain", 2.8},
            {"Portugal", 0.8},
            {"Croatia", 0},
            {"Serbia", 0},
            {"Romania", 0},
            {"Bulgaria", 0.6},
            {"Bosnia", 0},
            {"Slovakia", 0.9},
            {"Poland", 1.70},
            {"Finland", 0},
            {"Ireland", 0.30}
        };

        public static Dictionary<string, double> HydroCapacities = new Dictionary<string, double>
        {
            // Data source: Tradewind (output capacity in GW, 2005)
            {"Germany", 8.7},
            {"Belgium", 1.4},
            {"Luxemborg", 1.1},
            {"France", 25.5},
            {"Switzerland", 13.3},
            {"Italy", 21},
            {"Austria", 12},
            {"Spain", 18},
            {"Norway", 28},
            {"Sweden", 16},
            {"Czech Republic", 2.1},
            {"Slovenia", 0.9},
            {"Greece", 3},
            {"Great Britain", 4.3},
            {"Portugal", 5},
            {"Croatia", 2},
            {"Serbia", 3.5},
            {"Romania", 6},
            {"Bulgaria", 2.8},
            {"Bosnia", 2},
            {"Slovakia", 2.4},
            {"Poland", 2.23},
            {"Finland", 3},
            {"Ireland", 0.5}
        };

        public static Dictionary<string, double> Inflow = new Dictionary<string, double>
        {
            // Data source: Tradewind (inflow in TWh/year, 2005)
            {"Germany", 16.8},
            {"Belgium", 0},
            {"Luxemborg", 0},
            {"France", 55},
            {"Switzerland", 30.4},
            {"Italy", 35.5},
            {"Austria", 31.5},
            {"Spain", 24.8},
            {"Norway", 136},
            {"Sweden", 72.6},
            {"Czech Republic", 2.5},
            {"Slovenia", 3.1},
            {"Greece", 6},
            {"Great Britain", 5},
            {"Portugal", 10.6},
            {"Croatia", 6},
            {"Serbia", 11.8},
            {"Romania", 17.9},
            {"Bulgaria", 4.1},
            {"Bosnia", 6},
            {"Slovakia", 4.2},
            {"Poland", 1.7},
            {"Finland", 13.6},
            {"Ireland", 1}
        };

        #endregion

        public static List<string> GetCountries()
        {
            return CountryCodeMapName.Keys.ToList();
        } 

        /// <summary>
        /// Get the country name from the three or two letter abbreviation.
        /// </summary>
        public static string GetName(string abbrevation)
        {
            if (abbrevation.Length == 2) return CountryCodeMap2[abbrevation];
            if (abbrevation.Length == 3) return CountryCodeMap3[abbrevation];
            throw new ArgumentException("Wrong abbrev");
        }

        /// <summary>
        /// Get three letter abbreviation. Don't ever use the two letter - it's confusing.
        /// </summary>
        public static string GetAbbrev(string name)
        {
            return CountryCodeMapName[name];
        }

        /// <summary>
        /// Get three letter abbreviation. Don't ever use the two letter - it's confusing.
        /// </summary>
        public static string GetShortAbbrev(string name)
        {
            return CountryCodeMap2.Where(item => item.Value.Equals(name)).Select(item => item.Key).First();
        }

        /// <summary>
        /// Get the wind capacity factor for a country.
        /// </summary>
        public static double GetOnshoreWindCf(string name)
        {
            return WindOnshoreCf[name];
        }


        /// <summary>
        /// Get the wind capacity factor for a country.
        /// </summary>
        public static double GetOffshoreWindCf(string name)
        {
            return WindOffshoreCf[name];
        }

        /// <summary>
        /// Get the summed wind capacity factors.
        /// </summary>
        public static double GetWindCfSum()
        {
            return WindOnshoreCf.Values.Sum();
        }

        /// <summary>
        /// Get the solar capacity factor for a country.
        /// </summary>
        public static double GetSolarCf(string name)
        {
            return SolarCf[name];
        }

        /// <summary>
        /// Get the summed solar capacity factors.
        /// </summary>
        public static double GetSolarCfSum()
        {
            return SolarCf.Values.Sum();
        }

        /// <summary>
        /// Get the mean load for a country (preprocessed).
        /// </summary>
        /// <param name="country"> country name </param>
        /// <returns> mean load </returns>
        public static double GetMeanLoad(string country)
        {
            return MeanLoad[country];
        }

        /// <summary>
        /// Get the summed mean load for europe (preprocessed).
        /// </summary>
        /// <returns> mean load </returns>
        public static double GetMeanLoadSum()
        {
            return MeanLoad.Values.Sum();
        }

        /// <summary>
        /// Get the mean load for a country (preprocessed).
        /// </summary>
        /// <returns> mean load </returns>
        public static Dictionary<string, double> GetMeanLoad()
        {
            return MeanLoad;
        }
    }

    public class HydroInfo
    {
        // Reservoir capacity in TWh
        public double ReservoirCapacity { get; set; }
        // Total hydro capacity in MW
        public double Capacity { get; set; }
        // Pumped hydro capacity in MW
        public double PumpCapacity { get; set; }
        // Inflow pattern; average daily inflow in GWh
        public double[] InflowPattern { get; set; }
    }
}
