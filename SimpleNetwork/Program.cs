using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using ProtoBuf;
using ProtoBuf.Meta;
using SimpleImporter;

namespace SimpleNetwork
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CsvImporter.Parse(TsSource.ISET);
        }

    }
}
