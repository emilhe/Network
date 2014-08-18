using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;

namespace SimpleImporter
{

    public class CsvImporter
    {

        /// <summary>
        /// Base direction (where the files are).
        /// </summary>
        private static string Base;

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
            {"GB", "Great Britain"},
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
        };

        #endregion

        public static void Parse(TsSource source)
        {
            if (source == TsSource.ISET) Base = @"C:\Users\xXx\Documents\ISETdata";
            if (source == TsSource.VE) Base = @"C:\Users\xXx\Documents\data";

            var files = Directory.GetFiles(Base);
            var countries = new List<string>();

            for (int index = 0; index < files.Length; index++)
            {
                var file = files[index];
                // Parse only .csv files.
                if (!file.EndsWith(".csv")) continue;

                var country = GetTsCountry(file, source);
                var name = GetTsName(file, source);
                // For now, t time series are unimportant.
                if (name.Equals("t")) continue;
                ProtoStore.SaveTimeSeries(Parse(file, country, name, source));
                if (countries.Contains(country)) continue;
                countries.Add(country);
            }

            ProtoStore.SaveCountries(countries);
        }

        private static TimeSeriesItemDal Parse(string file, string country, string name, TsSource source)
        {
            var lines = File.ReadAllLines(file);
            var ts = new TimeSeriesItemDal
            {
                Data = new List<double>(),
                Country = country,
                Source = source,
                Type = GetTsType(name)
            };

            foreach (var line in lines)
            {
                ts.Data.Add(double.Parse(line));
            }
            return ts;
        }

        private static TsType GetTsType(string name)
        {
            if (name.Equals("Gs")) return TsType.Solar;
            if (name.Equals("Gw")) return TsType.Wind;
            if (name.Equals("L")) return TsType.Load;
            throw new ArgumentException("Non valid type");
        }

        #region Specific Methods

        private static string GetTsName(string file, TsSource source)
        {
            var fI = new FileInfo(file);
            var sub = fI.Name.Replace(".csv", "");
            if(source == TsSource.VE) return sub.Substring(10, sub.Length-(10));
            if (source == TsSource.ISET) return sub.Substring(19, sub.Length - (19));
            throw new ArgumentException("Non valid type");
        }

        private static string GetTsCountry(string file, TsSource source)
        {
            var fI = new FileInfo(file);
            var sub = fI.Name.Replace(".csv", "");
            if (source == TsSource.VE) return GetName(sub.Substring(3, 3));
            if (source == TsSource.ISET) return GetName(sub.Substring(13, 2));
            throw new ArgumentException("Non valid type");
        }

        #endregion

        public static string GetName(string abbrevation)
        {
            if (abbrevation.Length == 2) return CountryCodeMap2[abbrevation];
            if (abbrevation.Length == 3) return CountryCodeMap3[abbrevation];
            throw new ArgumentException("Wrong abbrev");
        }

    }

}
