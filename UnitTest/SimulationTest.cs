using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using NUnit.Framework;
using BusinessLogic;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Utils;
using SimpleImporter;

namespace UnitTest
{
    class SimulationTest
    {

        [Test]
        public void ConstrainedFlowTest()
        {
            // Basic initialization
            var nodes = ConfigurationUtils.CreateNodesWithBackup();
            var edges = Stuff.StraightLine(nodes);
            var model = new NetworkModel(nodes, new ConstrainedFlowExportStrategy(nodes, edges));
            var simulation = new SimulationCore(model);
            // Try a simulation which should work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.028;
            }
            simulation.Simulate(8766); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.027;
            }
            simulation.Simulate(8766); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

        [Test]
        public void CooperativeExportBottomUpTest()
        {
            // Basic initialization
            var nodes = ConfigurationUtils.CreateNodesWithBackup();
            var model = new NetworkModel(nodes, new CooperativeExportStrategy(new SkipFlowStrategy()));
            var simulation = new SimulationCore(model);
            // Try a simulation which should work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.028;
            }
            simulation.Simulate(Stuff.HoursInYear); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.027;
            }
            simulation.Simulate(Stuff.HoursInYear); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

        [Test]
        public void CooperativeExportMinimalFlowTest()
        {
            // Basic initialization
            var nodes = ConfigurationUtils.CreateNodesWithBackup();
            var edges = Stuff.StraightLine(nodes);
            var model = new NetworkModel(nodes, new CooperativeExportStrategy(new MinimalFlowStrategy(nodes, edges)));
            var simulation = new SimulationCore(model);
            // Try a simulation which should work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.028;
            }
            simulation.Simulate(Stuff.HoursInYear); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.027;
            }
            simulation.Simulate(Stuff.HoursInYear); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

        [Test]
        public void SelfishExportBottomUpTest()
        {
            // Basic initialization
            var nodes = ConfigurationUtils.CreateNodesWithBackup();
            var model = new NetworkModel(nodes, new SelfishExportStrategy(new SkipFlowStrategy()));
            var simulation = new SimulationCore(model);
            // Try a simulation which should work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.55;
                node.Model.Gamma = 1.328;
            }
            simulation.Simulate(Stuff.HoursInYear); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.55;
                node.Model.Gamma = 1.187;
            }
            simulation.Simulate(Stuff.HoursInYear); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

        [Test]
        public void NoExportBottomUpTest()
        {
            // Basic initialization
            var nodes = ConfigurationUtils.CreateNodesWithBackup();
            var model = new NetworkModel(nodes, new NoExportStrategy());
            var simulation = new SimulationCore(model);
            // Try a simulation which should work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.57;
                node.Model.Gamma = 1.536;
            }
            simulation.Simulate(Stuff.HoursInYear); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.57;
                node.Model.Gamma = 1.530;
            }
            simulation.Simulate(Stuff.HoursInYear); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

        [Test]
        public void VerifyISET()
        {
            var core = new FullCore(1, ConfigurationUtils.CreateNodes()); //new FullCore();
            var eval = new ParameterEvaluator(core){CacheEnabled = false};
            var calc = new NodeCostCalculator(eval);
            // Verify limiting case: free flow
            var freeFlow = calc.DetailedSystemCosts(new NodeGenes(1, 1), true);
            Assert.AreEqual(00.0000, freeFlow["Offshore wind"], 1e-3);
            Assert.AreEqual(36.8568, freeFlow["Onshore wind"], 1e-3);
            Assert.AreEqual(05.3806, freeFlow["Transmission"], 1e-3);
            Assert.AreEqual(07.1614, freeFlow["Backup"], 1e-3);
            Assert.AreEqual(00.0000, freeFlow["Solar"], 1e-3);
            Assert.AreEqual(10.3439, freeFlow["Fuel"], 1e-3);
            // Verify limiting case: no flow
            core.TcController.EdgeFuncs.Clear();
            core.TcController.EdgeFuncs.Add(string.Format("Europe edges, constrained {0}%", 0),
                list => ConfigurationUtils.GetEdges(list.Select(item => (INode)item).ToList(), "NtcMatrix", 0));
            var noFlow = calc.DetailedSystemCosts(new NodeGenes(1, 1), true);
            Assert.AreEqual(00.0000, noFlow["Offshore wind"], 1e-3);
            Assert.AreEqual(36.8568, noFlow["Onshore wind"], 1e-3);
            Assert.AreEqual(00.0000, noFlow["Transmission"], 1e-3);
            Assert.AreEqual(07.5396, noFlow["Backup"], 1e-3);
            Assert.AreEqual(00.0000, noFlow["Solar"], 1e-3);
            Assert.AreEqual(18.3313, noFlow["Fuel"], 1e-3);
        }

    }
}
