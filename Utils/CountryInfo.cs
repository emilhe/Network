using System;
using System.Collections.Generic;

namespace Utils
{
    public class CountryInfo
    {

        #region Country code mappings

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
            {"LU", "Luxemborg"},
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
            {"LUX", "Luxemborg"},
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
            {"Luxemborg", "LUX"},
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
            {"Cyprus", "CYP"},
            {"Malta", "MLT"},
        };

        #endregion

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

    }
}
