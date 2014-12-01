using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using NUnit.Framework;
using BusinessLogic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;

namespace UnitTest
{
     [TestFixture]
    class ExportTest
    {

        private List<CountryNode> _mNodes;

        [Test]
        public void NoExportTest()
        {
            PreareNodesExcess();
            var model = new NetworkModel(_mNodes, new NoExportStrategy());
            var simulation = new SimulationCore(model);
            simulation.Simulate(1);
            
            Assert.AreEqual(false, simulation.Output.Success);
            Assert.AreEqual(1, ReadParam(simulation, "Test0","Battery storage"));
            Assert.AreEqual(1, ReadParam(simulation, "Test0", "Hydrogen storage"));
            Assert.AreEqual(0, ReadParam(simulation, "Test1", "Battery storage"));
            Assert.AreEqual(0, ReadParam(simulation, "Test1", "Hydrogen storage"));
        }

        [Test]
        public void SelfishExportTestExcess()
        {
            PreareNodesExcess();
            var model = new NetworkModel(_mNodes, new SelfishExportStrategy(new SkipFlowStrategy()));
            var simulation = new SimulationCore(model);
            simulation.Simulate(1);

            Assert.AreEqual(true, simulation.Output.Success);
            Assert.AreEqual(1, ReadParam(simulation, "Test0", "Battery storage"), 1e-5);
            Assert.AreEqual(1, ReadParam(simulation, "Test0", "Hydrogen storage"), 1e-5);
            //Assert.AreEqual(0, ReadParam(simulation, "Test1", "Battery storage"), 1e-5);
            //Assert.AreEqual(0, ReadParam(simulation, "Test1", "Hydrogen storage"), 1e-5);
        }

        [Test]
        public void SelfishExportTestLack()
        {
            PreareNodesLack();
            var model = new NetworkModel(_mNodes, new SelfishExportStrategy(new SkipFlowStrategy()));
            var simulation = new SimulationCore(model);
            simulation.Simulate(1);

            Assert.AreEqual(false, simulation.Output.Success);
            Assert.AreEqual(2, ReadParam(simulation, "Test0", "Battery storage"), 1e-5);
            //Assert.AreEqual(1, ReadParam(simulation, "Test0", "Hydrogen storage"), 1e-5);
            Assert.AreEqual(0, ReadParam(simulation, "Test1", "Battery storage"), 1e-5);
            Assert.AreEqual(0, ReadParam(simulation, "Test1", "Hydrogen storage"), 1e-5);
        }

        [Test]
        public void CooperativeExportTest()
        {
            PreareNodesExcess();
            var model = new NetworkModel(_mNodes, new CooperativeExportStrategy(new SkipFlowStrategy()));
            var simulation = new SimulationCore(model);
            simulation.Simulate(1);

            Assert.AreEqual(true, simulation.Output.Success);
            Assert.AreEqual(1, ReadParam(simulation, "Test0", "Battery storage"));
            Assert.AreEqual(0.4, ReadParam(simulation, "Test0", "Hydrogen storage"), 1e-5);
            Assert.AreEqual(1, ReadParam(simulation, "Test1", "Battery storage"));
            Assert.AreEqual(0, ReadParam(simulation, "Test1", "Hydrogen storage"));
        }

        private void PreareNodesExcess()
        {
            var tsDumb = new DenseTimeSeries("Empty ts");
            var ts0 = new DenseTimeSeries("Ts0");
            var ts1 = new DenseTimeSeries("Ts1");
            ts0.AppendData((double)-11 / 3);
            ts1.AppendData(1);
            tsDumb.AppendData(0);
            var ba0 = new BatteryStorage(1);
            var ba1 = new BatteryStorage(1);
            var hy0 = new HydrogenStorage(1);
            var hy1 = new HydrogenStorage(1);
            var node0 = new CountryNode(new ReModel("Test0", ts0, tsDumb, tsDumb));
            var node1 = new CountryNode(new ReModel("Test1", ts1, tsDumb, tsDumb));
            node0.StorageCollection.Add(ba0);
            node0.StorageCollection.Add(hy0);
            node1.StorageCollection.Add(ba1);
            node1.StorageCollection.Add(hy1);

            _mNodes = new List<CountryNode> { node0, node1 };
        }

        private void PreareNodesLack()
        {
            var tsDumb = new DenseTimeSeries("Empty ts");
            var ts0 = new DenseTimeSeries("Ts0");
            var ts1 = new DenseTimeSeries("Ts1");
            ts0.AppendData(-1);
            ts1.AppendData(2);
            tsDumb.AppendData(0);
            var ba0 = new BatteryStorage(2, 1);
            var ba1 = new BatteryStorage(2, 1);
            var hy0 = new HydrogenStorage(1 , 1);
            var hy1 = new HydrogenStorage(1 , 1);
            var node0 = new CountryNode(new ReModel("Test0", ts0, tsDumb, tsDumb));
            var node1 = new CountryNode(new ReModel("Test1", ts1, tsDumb, tsDumb));
            node0.StorageCollection.Add(ba0);
            node0.StorageCollection.Add(hy0);
            node1.StorageCollection.Add(ba1);
            node1.StorageCollection.Add(hy1);

            _mNodes = new List<CountryNode> {node0, node1};
        }

        private double ReadParam(SimulationCore sim, string country, string type)
        {
            var ts = sim.Output.TimeSeries.Where(
                item => item.Properties.ContainsKey("Country") && item.Properties["Country"].Equals(country) &&
                item.Properties.ContainsKey("Name") && item.Properties["Name"].Equals(type));
            return ts.First().First().Value;
        }

    }
}
