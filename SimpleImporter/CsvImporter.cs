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
            switch (source)
            {
                case TsSource.ISET:
                    Base = @"C:\data";
                    ParseOld(source);
                    break;
                case TsSource.VE:
                    Base = @"C:\data";
                    ParseOld(source);
                    break;
                case TsSource.VE50PCT:
                    Base = @"C:\Users\Emil\Desktop\REatlas-client";
                    ParseNew(source);
                    break;
                default:
                    throw new ArgumentException("Unsupported source.");
            }
            
        }

        private static void ParseNew(TsSource source)
        {
            var countries = ProtoStore.LoadCountries();

            foreach (var country in countries)
            {
                var windOffShore = Path.Combine(Base, "wind_50pct_offshore_timeseries", CountryInfo.GetAbbrev(country) + ".csv");
                var solar = Path.Combine(Base, "solar_50pct_onshore_timeseries", CountryInfo.GetAbbrev(country) + ".csv");
                var windOnShore = Path.Combine(Base, "wind_50pct_onshore_timeseries", CountryInfo.GetAbbrev(country) + ".csv");
                // Onshore wind
                var ts = ParseTsNew(windOnShore, country, TsType.OnshoreWind, source);
                ProtoStore.SaveTimeSeriesInImport(ts,
                    GenerateFileKey(ts.Properties["Name"], (TsType) byte.Parse(ts.Properties["Type"]),
                        (TsSource) byte.Parse(ts.Properties["Source"])));
                // Offshore wind
                ts = ParseTsNew(windOffShore, country, TsType.OffshoreWind, source);
                ProtoStore.SaveTimeSeriesInImport(ts,
                    GenerateFileKey(ts.Properties["Name"], (TsType)byte.Parse(ts.Properties["Type"]),
                        (TsSource)byte.Parse(ts.Properties["Source"])));
                // Solar
                ts = ParseTsNew(solar, country, TsType.Solar, source);
                ProtoStore.SaveTimeSeriesInImport(ts,
                    GenerateFileKey(ts.Properties["Name"], (TsType)byte.Parse(ts.Properties["Type"]),
                        (TsSource)byte.Parse(ts.Properties["Source"])));

            }

            // NOTE: For now, countries are NOT reread since the new data set contains too many.
        }

        private static TimeSeriesDal ParseTsNew(string file, string country, TsType type, TsSource source)
        {
            var ts = new TimeSeriesDal
            {
                Data = new List<double>(),
                Properties = new Dictionary<string, string>
                {
                    {"Name", country},
                    {"Source", ((byte)source).ToString()},
                    {"Type", ((byte)type).ToString()}
                }
            };

            if (!File.Exists(file)) return ts;

            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                if (line.Equals("nan"))
                {
                    ts.Data.Add(0);
                    continue;
                }
                ts.Data.Add(double.Parse(line));
            }

            return ts;
        }

        #region Old

        private static void ParseOld(TsSource source)
        {
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
                var ts = ParseTsOld(file, country, name, source);
                ProtoStore.SaveTimeSeriesInImport(ts,
                    GenerateFileKey(ts.Properties["Name"], (TsType)byte.Parse(ts.Properties["Type"]),
                        (TsSource)byte.Parse(ts.Properties["Source"])));
                if (countries.Contains(country)) continue;
                countries.Add(country);
            }

            ProtoStore.SaveCountries(countries);
        }

        private static TimeSeriesDal ParseTsOld(string file, string country, string name, TsSource source)
        {
            return ParseTsNew(file, country, GetTsType(name), source);
        }

        private static TsType GetTsType(string name)
        {
            if (name.Equals("Gs")) return TsType.Solar;
            if (name.Equals("Gw")) return TsType.Wind;
            if (name.Equals("L")) return TsType.Load;
            throw new ArgumentException("Non valid type");
        }

        #endregion

        public static string GenerateFileKey(string country, TsType type, TsSource source)
        {
            return string.Format("Ts-{0}-{1}-{2}", country, type, source);
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
