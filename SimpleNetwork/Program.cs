﻿using System.Collections.Generic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Utils;
using SimpleImporter;

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

            //var nodes = new double[] { 2, -1 };
            //var nodeNames = new[] { "Node1", "Node2" };
            //var builder = new EdgeBuilder(nodeNames);
            //builder.Connect(0, 1);
        }

    }
}
