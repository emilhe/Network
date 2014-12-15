using System;
using System.Collections.Generic;
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
        //private static readonly Dictionary<string, double> WindCf = new Dictionary<string, double>
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

        #region Capacity factos mappsings: Magnus (VE)

        private static readonly Dictionary<string, double> WindCf =
            FileUtils.DictionaryFromFile<string, double>(@"C:\proto\CFwClean.txt");

        private static readonly Dictionary<string, double> SolarCf =
            FileUtils.DictionaryFromFile<string, double>(@"C:\proto\CFsClean.txt");

        #endregion

        #region Mean load mappings

        // Source: 32 years of VE data.
        private static readonly Dictionary<string, double> MeanLoad =
            FileUtils.DictionaryFromFile<string, double>(@"C:\proto\MeanLoad.txt");

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
        public static double GetWindCf(string name)
        {
            return WindCf[name];
        }

        /// <summary>
        /// Get the summed wind capacity factors.
        /// </summary>
        public static double GetWindCfSum()
        {
            return WindCf.Values.Sum();
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

        // TODO: Is this OK?
        public static Dictionary<string, double> GetSolarCf()
        {
            return SolarCf;
        }

        // TODO: Is this OK?
        public static Dictionary<string, double> GetWindCf()
        {
            return WindCf;
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
}
