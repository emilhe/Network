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
        /// Limit the number of nodes (for test purposes).
        /// </summary>
        private const int Limit = 1000;

        /// <summary>
        /// Base direction (where the files are).
        /// </summary>
        private const string Base = @"C:\Users\xXx\Documents\data";

        public static void ResetDb()
        {
            MainAccessClient.DropDatabase();
            var client = new MainAccessClient();
            client.SaveCountryData(Parse());
        }

        private static List<CountryData> Parse()
        {
            var data = new Dictionary<string, CountryData>();
            var files = Directory.GetFiles(Base);
            for (int index = 0; index < files.Length; index++)
            {
                var file = files[index];
                // Parse only .csv files.
                if (!file.EndsWith(".csv")) continue;

                var country = GetTsCountry(file);
                var name = GetTsName(file);
                // For now, t time series are unimportant.
                if (name.Equals("t")) continue;

                var ts = Parse(file, name);
                if (data.ContainsKey(country)) data[country].TimeSeries.Add(ts);
                else
                {
                    if (data.Count == Limit) break;
                    var abbrev = GetTsCountry(file);
                    data.Add(abbrev, new CountryData { Abbreviation = abbrev, TimeSeries = new List<DenseTimeSeries> { ts } });
                }
            }

            return data.Values.ToList();
        }

        private static DenseTimeSeries Parse(string file, string name)
        {
            var lines = File.ReadAllLines(file);
            var ts = new DenseTimeSeries(name);
            foreach (var line in lines)
            {
                ts.AppendData(double.Parse(line));
            }
            return ts;
        }

        private static string GetTsName(string file)
        {
            var fI = new FileInfo(file);
            var sub = fI.Name.Replace(".csv", "");
            return sub.Substring(10, sub.Length-(10));
        }

        private static string GetTsCountry(string file)
        {
            var fI = new FileInfo(file);
            var sub = fI.Name.Replace(".csv", "");
            return sub.Substring(3, 3);
        }
    }

    public class CountryCodeMap
    {

        private static Dictionary<string, string> map = new Dictionary<string, string>
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
            {"NOR", "Norway"},
            {"LVA", "Latvia"},
            {"LUX", "Luxemborg"},
            {"LTU", "Lithuania"},
            {"ITA", "Italy"},
            {"IRL", "Ireland"},
            {"HUN", "Hungary"},
            {"HRV", "Croatia"},
            {"GRC", "Greece"},
            {"GRB", "Great Britain"},
            {"FRA", "France"},
            {"FIN", "Finland"},
            {"EST", "Estonia"},
            {"ESP", "Espania"},
            {"EST", "Spain"},
            {"DNK", "Denmark"},
            {"DEU", "Germany"},
            {"CZE", "Czech Republic"},
            {"CHE", "Switzerland"},
            {"BIH", "Bosnia"},
            {"BGR", "Bulgaria"},
            {"BEL", "Belgium"},
            {"AUT", "Austria"},
        };

        public string GetName(string abbrevation)
        {
            return map[abbrevation];
        }

    }

}
