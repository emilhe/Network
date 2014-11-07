using NUnit.Framework;
using BusinessLogic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
using BusinessLogic.Utils;

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
            var simulation = new Simulation(model);
            var mCtrl = new MixController(nodes);
            // Try a simulation which should work.
            mCtrl.SetMix(0.65);
            mCtrl.SetPenetration(1.029);
            mCtrl.Execute();
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            mCtrl.SetMix(0.65);
            mCtrl.SetPenetration(1.028);
            mCtrl.Execute();
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

        [Test]
        public void CooperativeExportBottomUpTest()
        {
            // Basic initialization
            var nodes = ConfigurationUtils.CreateNodesWithBackup();
            var model = new NetworkModel(nodes, new CooperativeExportStrategy(new SkipFlowStrategy()));
            var simulation = new Simulation(model);
            var mCtrl = new MixController(nodes);
            // Try a simulation which should work.
            mCtrl.SetMix(0.65);
            mCtrl.SetPenetration(1.029);
            mCtrl.Execute();
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            mCtrl.SetMix(0.65);
            mCtrl.SetPenetration(1.028);
            mCtrl.Execute();
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
            var simulation = new Simulation(model);
            var mCtrl = new MixController(nodes);
            // Try a simulation which should work.
            mCtrl.SetMix(0.65);
            mCtrl.SetPenetration(1.029);
            mCtrl.Execute();
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            mCtrl.SetMix(0.65);
            mCtrl.SetPenetration(1.0); //28
            mCtrl.Execute();
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

        [Test]
        public void SelfishExportBottomUpTest()
        {
            // Basic initialization
            var nodes = ConfigurationUtils.CreateNodesWithBackup();
            var model = new NetworkModel(nodes, new SelfishExportStrategy(new SkipFlowStrategy()));
            var simulation = new Simulation(model);
            var mCtrl = new MixController(nodes);
            // Try a simulation which should work.
            mCtrl.SetMix(0.55);
            mCtrl.SetPenetration(1.288);
            mCtrl.Execute();
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            mCtrl.SetMix(0.55);
            mCtrl.SetPenetration(1.287);
            mCtrl.Execute();
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

        [Test]
        public void NoExportBottomUpTest()
        {
            // Basic initialization
            var nodes = ConfigurationUtils.CreateNodesWithBackup();
            var model = new NetworkModel(nodes, new NoExportStrategy());
            var simulation = new Simulation(model);
            var mCtrl = new MixController(nodes);
            // Try a simulation which should work.
            mCtrl.SetMix(0.57);
            mCtrl.SetPenetration(1.536);
            mCtrl.Execute();
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(true, simulation.Output.Success);
            // Try a simulation which should NOT work.
            mCtrl.SetMix(0.57);
            mCtrl.SetPenetration(1.534);
            mCtrl.Execute();
            simulation.Simulate(8760); // One year.
            Assert.AreEqual(false, simulation.Output.Success);
        }

    }
}
