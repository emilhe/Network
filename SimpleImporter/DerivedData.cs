using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace SimpleImporter
{
    public class DerivedData
    {

        private const string BaseDir = @"C:\proto\";

        public static void CalculateMeanLoad()
        {
            var dictionary = ProtoStore.LoadCountries()
                .ToDictionary(item => item,
                    country =>
                        ProtoStore.LoadTimeSeriesFromRoot(CsvImporter.GenerateFileKey(country, TsType.Load, TsSource.VE))
                            .Data.Average());
            dictionary.ToFile(@"C:\proto\MeanLoad.txt");
        }

    }
}
