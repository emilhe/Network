using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Utils;
using NUnit.Framework;
using Utils;

namespace UnitTest.Solvers
{
    [TestFixture]
    class GurobiStabilityTest
    {

        [Test]
        public void Test()
        {
            var nodes = ConfigurationUtils.CreateNodesNew();
            var edges = ConfigurationUtils.GetEuropeEdgeObject(nodes);
            var scheme = new ConLocalScheme(nodes, edges);

            var dir = @"C:\Users\Emil\Desktop\GurobiProblems\";
            foreach (var file in Directory.GetFiles(dir))
            {
                var deltas = FileUtils.FromJsonFile<double[]>(file);
                scheme.Bind(deltas);
                scheme.BalanceSystem();
                // TODO: Make assertion.
            }
        }

    }
}
