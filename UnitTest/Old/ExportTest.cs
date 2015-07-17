using System.Collections.Generic;
using System.Linq;
using BusinessLogic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using NUnit.Framework;

namespace UnitTest
{
     [TestFixture]
    class ExportTest
    {

         private CountryNode[] _mNodes;

        [Test]
        public void NoExportTest()
        {
            PreareNodesExcess();
            var model = new NetworkModel(_mNodes, new NoExportScheme(_mNodes));
            var simulation = new SimulationCore(model) {LogAllNodeProperties = true};
            simulation.Simulate(1);
            
            //Assert.AreEqual(false, simulation.Output.Success);
            Assert.AreEqual(1, ReadParam(simulation, "Test0","Battery storage"));
            Assert.AreEqual(1, ReadParam(simulation, "Test0", "Hydrogen storage"));
            Assert.AreEqual(0, ReadParam(simulation, "Test1", "Battery storage"));
            Assert.AreEqual(0, ReadParam(simulation, "Test1", "Hydrogen storage"));
        }

        //[Test]
        //public void SelfishExportTestExcess()
        //{
        //    PreareNodesExcess();
        //    var model = new NetworkModel(_mNodes, new SelfishExportStrategy(new SkipFlowStrategy()));
        //    var simulation = new SimulationCore(model);
        //    simulation.LogAllNodeProperties = true;
        //    simulation.Simulate(1);

        //    //Assert.AreEqual(true, simulation.Output.Success);
        //    Assert.AreEqual(1, ReadParam(simulation, "Test0", "Battery storage"), 1e-5);
        //    Assert.AreEqual(1, ReadParam(simulation, "Test0", "Hydrogen storage"), 1e-5);
        //    //Assert.AreEqual(0, ReadParam(simulation, "Test1", "Battery storage"), 1e-5);
        //    //Assert.AreEqual(0, ReadParam(simulation, "Test1", "Hydrogen storage"), 1e-5);
        //}

        //[Test]
        //public void SelfishExportTestLack()
        //{
        //    PreareNodesLack();
        //    var model = new NetworkModel(_mNodes, new SelfishExportStrategy(new SkipFlowStrategy()));
        //    var simulation = new SimulationCore(model);
        //    simulation.LogAllNodeProperties = true;
        //    simulation.Simulate(1);

        //    //Assert.AreEqual(false, simulation.Output.Success);
        //    Assert.AreEqual(2, ReadParam(simulation, "Test0", "Battery storage"), 1e-5);
        //    //Assert.AreEqual(1, ReadParam(simulation, "Test0", "Hydrogen storage"), 1e-5);
        //    Assert.AreEqual(0, ReadParam(simulation, "Test1", "Battery storage"), 1e-5);
        //    Assert.AreEqual(0, ReadParam(simulation, "Test1", "Hydrogen storage"), 1e-5);
        //}

        //[Test]
        //public void CooperativeExportTest()
        //{
        //    PreareNodesExcess();
        //    var model = new NetworkModel(_mNodes, new ConLocalScheme(_mNodes, Stuff.StraightLine(_mNodes)));
        //    var simulation = new SimulationCore(model) {LogAllNodeProperties = true};
        //    simulation.Simulate(1);

        //    //Assert.AreEqual(true, simulation.Output.Success);
        //    Assert.AreEqual(1, ReadParam(simulation, "Test0", "Battery storage"));
        //    Assert.AreEqual(0.4, ReadParam(simulation, "Test0", "Hydrogen storage"), 1e-5);
        //    Assert.AreEqual(1, ReadParam(simulation, "Test1", "Battery storage"));
        //    Assert.AreEqual(0, ReadParam(simulation, "Test1", "Hydrogen storage"));
        //}

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
            node0.Storages.Add(ba0);
            node0.Storages.Add(hy0);
            node1.Storages.Add(ba1);
            node1.Storages.Add(hy1);

            _mNodes = new[]{ node0, node1 };
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
            node0.Storages.Add(ba0);
            node0.Storages.Add(hy0);
            node1.Storages.Add(ba1);
            node1.Storages.Add(hy1);

            _mNodes = new[] {node0, node1};
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
