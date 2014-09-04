using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace SimpleImporter
{
    /// <summary>
    /// Class for parsing/importing 
    /// </summary>
    public class EcnImporter
    {
        /// <summary>
        /// Absolute path the file in which the information is contained.
        /// </summary>
        private const string Path = @"C:\Users\xXx\Dropbox\Master Thesis\e10069_database.csv";
        private const string Norway = @"C:\Users\xXx\Dropbox\Master Thesis\norway.csv";

        public static void Parse()
        {
            var lines = File.ReadAllLines(Path);
            var data = new List<EcnDataRow>();
            ParseLines(lines, data);

            // Norway is NOT included in the DB file, thus appended here.
            lines = File.ReadAllLines(Norway);
            ParseLines(lines, data);

            ProtoStore.SaveEcnData(data);

            Console.WriteLine("ECN data parsed.");
            Console.ReadLine();
        }

        private static void ParseLines(IEnumerable<string> lines, List<EcnDataRow> data)
        {
            var idx = 0;
            foreach (var line in lines)
            {
                idx++;
                if (idx < 46) continue;
                var cells = line.Split(',');
                // Try parsing year and value into numbers.
                double val;
                int year;
                int.TryParse(cells[4], out year);
                double.TryParse(cells[6], out val);
                // Wrap all data in a data row.
                data.Add(new EcnDataRow
                {
                    Country = CountryInfo.GetName(cells[0]),
                    RowHeader = cells[2],
                    ColumnHeader = cells[3],
                    Year = year,
                    Unit = cells[5],
                    Value = val,
                });
            }
        }


    }
}
