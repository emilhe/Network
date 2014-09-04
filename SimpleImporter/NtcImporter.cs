using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleImporter
{
    /// <summary>
    /// Class for parsing/importing 
    /// </summary>
    public class NtcImporter
    {

        private const string Path = @"C:\Users\xXx\Dropbox\Master Thesis\NtcMatrix.txt";

        public static void Parse()
        {
            var lines = File.ReadAllLines(Path);
            var data = new List<NtcDataRow>();
            ParseLines(lines, data);

            ProtoStore.SaveNtcData(data);

            Console.WriteLine("NTC data parsed.");
            Console.ReadLine();
        }

        private static void ParseLines(IEnumerable<string> lines, List<NtcDataRow> data)
        {
            var i = 0;
            foreach (var line in lines)
            {
                var cells = line.Split('\t');
                for (int j = 0; j < cells.Length; j++)
                {
                    data.Add(new NtcDataRow
                    {
                        CountryFrom = Countries[i],
                        CountryTo = Countries[j],
                        LinkCapacity = int.Parse(cells[j])
                    });
                }
                i++;
            }
        }

        private static readonly List<string> Countries = new List<string>
        {
            "Austria",
            "Finland",
            "Netherlands",
            "Bosnia",
            "France",
            "Norway",
            "Belgium",
            "Great Britain",
            "Poland",
            "Bulgaria",
            "Greece",
            "Portugal",
            "Switzerland",
            "Croatia",
            "Romania",
            "Czech Republic",
            "Hungary",
            "Serbia",
            "Germany",
            "Ireland",
            "Sweden",
            "Denmark",
            "Italy",
            "Slovenia",
            "Spain",
            "Luxemborg",
            "Slovakia",
            "Estonia",
            "Latvia",
            "Lithuania",
        };

    }
}
