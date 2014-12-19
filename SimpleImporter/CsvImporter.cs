using System;
using System.Collections.Generic;
using System.IO;
using Utils;

namespace SimpleImporter
{
    /// <summary>
    /// Class for parsing/importing time series data.
    /// </summary>
    public class CsvImporter
    {

        /// <summary>
        /// Base direction (where the files are).
        /// </summary>
        private static string Base;

        public static void Parse(TsSource source)
        {
            if (source == TsSource.ISET) Base = @"C:\data";
            if (source == TsSource.VE) Base = @"C:\data";

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
                var ts = Parse(file, country, name, source);
                ProtoStore.SaveTimeSeriesInImport(ts,
                    GenerateFileKey(ts.Properties["Name"], (TsType) byte.Parse(ts.Properties["Type"]),
                        (TsSource) byte.Parse(ts.Properties["Source"])));
                if (countries.Contains(country)) continue;
                countries.Add(country);
            }

            ProtoStore.SaveCountries(countries);
        }

        public static string GenerateFileKey(string country, TsType type, TsSource source)
        {
            return string.Format("Ts-{0}-{1}-{2}", country, type, source);
        }

        private static TimeSeriesDal Parse(string file, string country, string name, TsSource source)
        {
            var lines = File.ReadAllLines(file);
            var ts = new TimeSeriesDal
            {
                Data = new List<double>(),
                Properties = new Dictionary<string, string>
                {
                    {"Name", country},
                    {"Source", ((byte)source).ToString()},
                    {"Type", ((byte)GetTsType(name)).ToString()}
                }
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
            if (source == TsSource.VE) return CountryInfo.GetName(sub.Substring(3, 3));
            if (source == TsSource.ISET) return CountryInfo.GetName(sub.Substring(13, 2));
            throw new ArgumentException("Non valid type");
        }

        #endregion

    }

}
