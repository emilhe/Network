using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.ExportStrategies;
using BusinessLogic.FailureStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.Utils;
using NUnit.Framework;
using SimpleImporter;
using Utils;

namespace UnitTest
{

    [TestFixture]
    class Playground
    {

        [Test]
        public void PlayGround()
        {
            var genes = NodeGenesFactory.SpawnCfMax(1,1,1);
            var nodes = ConfigurationUtils.CreateNodes(TsSource.ISET, 0);
            // Do stuff..
            var uncSync = new MockSimulationController
            {
                Scheme = new UncSyncScheme(nodes, ConfigurationUtils.GetEuropeEdgeObject(nodes),Stuff.DeltaWeights(nodes, genes)),
                Nodes = nodes,
                Years = 1
            };
            var simpleCore = new SimpleCore(uncSync, 1, nodes);
            var eval = new ParameterEvaluator(simpleCore);
            var cost = (new NodeCostCalculator(eval)).DetailedSystemCosts(genes);
            Console.WriteLine("System cost is " + cost.Select(item => item.Value).Sum());
        }

        private Dictionary<string, double> GetDetailedSystemCosts(SimulationController ctrl, CountryNode[] nodes)
        {
            var simpleCore = new SimpleCore(ctrl, 1, nodes);
            var eval = new ParameterEvaluator(simpleCore);
            return (new NodeCostCalculator(eval)).DetailedSystemCosts(new NodeGenes(1, 1));
        }

    }

    class MockSimulationController : ISimulationController
    {
        public bool CacheEnabled
        {
            get { return false; }
            set {  }
        }

        public bool InvalidateCache
        {
            get { return false; }
            set { }
        }

        public IExportScheme Scheme { get; set; }
        public CountryNode[] Nodes { get; set; }
        public int Years { get; set; }

        public List<SimulationOutput> EvaluateTs(NodeGenes genes)
        {
            var model = new NetworkModel(Nodes, Scheme);
            var simulation = new SimulationCore(model)
            {
                LogAllNodeProperties = false,
                LogFlows = true,
            };
            var watch = new Stopwatch();
            watch.Start();
            foreach (var node in Nodes)
            {
                node.Model.Gamma = genes[node.Name].Gamma;
                node.Model.Alpha = genes[node.Name].Alpha;
                node.Model.OffshoreFraction = genes[node.Name].OffshoreFraction;
            }
            simulation.Simulate(Stuff.HoursInYear * Years);

            return new List<SimulationOutput>{simulation.Output};
        }

        public List<SimulationOutput> EvaluateTs(double penetration, double mixing)
        {
            throw new NotImplementedException();
        }
    }

}
