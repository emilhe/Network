using System;
using System.Collections.Generic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Utils;
using MathNet.Numerics.Distributions;
using SimpleImporter;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;
using Utils;
using DenseMatrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;

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
            //CsvImporter.Parse(TsSource.VE50PCT);
            //NtcImporter.Parse();

            var nodes = new double[] { 2, -1 };
            var nodeNames = new[] { "Node1", "Node2" };
            var builder = new EdgeBuilder(nodeNames);
            builder.Connect(0, 1);
            var edges = builder.ToEdges();
            var opt = new LinearOptimizer(edges, 0);

            // Test the most basic test case.v
            opt.SetNodes(nodes, new List<double[]>(), new List<double[]>());
            opt.Solve();
        }

    }
}
