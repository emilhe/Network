using System.Linq;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using NUnit.Framework;
using BusinessLogic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
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
            var edges = BusinessLogic.Utils.Utils.StraightLine(nodes);
            var model = new NetworkModel(nodes, new ConstrainedFlowExportStrategy(nodes, edges));
            var simulation = new SimulationCore(model);
            // Try a simulation which should work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.029;
            }
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.028;
            }
            simulation.Simulate(8760); // One year.
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
                node.Model.Gamma = 1.029;
            }
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.028;
            }
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

        [Test]
        public void CooperativeExportMinimalFlowTest()
        {
            // Basic initialization
            var nodes = ConfigurationUtils.CreateNodesWithBackup();
            var edges = BusinessLogic.Utils.Utils.StraightLine(nodes);
            var model = new NetworkModel(nodes, new CooperativeExportStrategy(new MinimalFlowStrategy(nodes, edges)));
            var simulation = new SimulationCore(model);
            // Try a simulation which should work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.029;
            }
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.65;
                node.Model.Gamma = 1.0;
            }
            simulation.Simulate(8760); // One year.
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
                node.Model.Gamma = 1.228;
            }
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.55;
                node.Model.Gamma = 1.287;
            }
            simulation.Simulate(8760); // One year.
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
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            foreach (var node in nodes)
            {
                node.Model.Alpha = 0.57;
                node.Model.Gamma = 1.534;
            }
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

    }
}
