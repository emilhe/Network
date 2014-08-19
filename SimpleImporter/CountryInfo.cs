using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleImporter
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

        #endregion

        public static string GetName(string abbrevation)
        {
            if (abbrevation.Length == 2) return CountryCodeMap2[abbrevation];
            if (abbrevation.Length == 3) return CountryCodeMap3[abbrevation];
            throw new ArgumentException("Wrong abbrev");
        }

    }
}
