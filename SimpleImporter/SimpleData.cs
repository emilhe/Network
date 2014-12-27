using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace SimpleImporter
{
    public class SimpleData
    {

        private const string BaseDir = @"C:\proto\";

        public static void CalculateMeanLoad()
        {
            var dictionary = ProtoStore.LoadCountries()
                .ToDictionary(item => item,
                    country =>
                        ProtoStore.LoadTimeSeriesFromImport(CsvImporter.GenerateFileKey(country, TsType.Load, TsSource.VE))
                            .Data.Average());
            dictionary.ToFile(@"C:\proto\MeanLoad.txt");
        }

        public static void ParseLinkCosts()
        {
            const string path = @"C:\Users\Emil\Dropbox\Master Thesis\e_lineinfo.txt";

            // Parse data.
            var lines = File.ReadAllLines(path);
            var type = new Dictionary<string, string>();
            var length = new Dictionary<string, int>();

            foreach (var line in lines)
            {
                var cells = line.Split('\t');
                // Construct key.
                var from = CountryInfo.GetName(cells[0].Split(' ')[0]);
                var to = CountryInfo.GetName(cells[0].Split(' ')[2]);
                var key = string.Format("{0}-{1}", from, to);
                // Save info.
                type.Add(key, cells[1]);
                length.Add(key, int.Parse(cells[2]));
            }
            
            type.ToFile(@"C:\proto\LineType.txt");
            length.ToFile(@"C:\proto\LineLength.txt");
        }
    }
}
