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

        private const string Path = @"C:\Users\Emil\Dropbox\Master Thesis\NtcMatrix.txt";

        public static void Parse()
        {
            var lines = File.ReadAllLines(Path);
            var data = new List<LinkDataRow>();
            ParseLines(lines, data);

            ProtoStore.SaveLinkData(data, "NtcMatrix");

            Console.WriteLine("NTC data parsed.");
            Console.ReadLine();
        }

        private static void ParseLines(IEnumerable<string> lines, List<LinkDataRow> data)
        {
            var i = 0;
            foreach (var line in lines)
            {
                var cells = line.Split('\t');
                for (int j = 0; j < cells.Length; j++)
                {
                    data.Add(new LinkDataRow
                    {
                        From = Countries[i],
                        To = Countries[j],
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
            "Luxembourg",
            "Slovakia",
            "Estonia",
            "Latvia",
            "Lithuania",
        };

    }
}
