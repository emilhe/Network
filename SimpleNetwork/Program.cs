﻿using SimpleImporter;

namespace BusinessLogic
{
    internal class Program
    {

        /// <summary>
        /// Console test entry point. Not really prette that it's here...
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            //SimpleData.CalculateMeanLoad();
            //EcnImporter.Parse();
            CsvImporter.Parse(TsSource.VE50PCT);
            //NtcImporter.Parse();
        }

    }
}
