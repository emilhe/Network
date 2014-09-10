using System.Collections.Generic;
using System.Linq;
using BusinessLogic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
using BusinessLogic.Generators;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;
using NUnit.Framework;
using Utils.Statistics;

namespace UnitTest
{
    class SimulationFlowTest
    {

        [Test]
        public void TwoNodeTest()
        {
            // Setup stuff.        
            var tsOneLoad = new DenseTimeSeries("Node 1 LoadTs");
            var tsTwoLoad = new DenseTimeSeries("Node 2 LoadTs");
            var tsOneGen = new DenseTimeSeries("Node 1 GenTs");
            var tsTwoGen = new DenseTimeSeries("Node 2 GenTs");
            var genOne = new TimeSeriesGenerator("Node 1 Gen", tsOneGen);
            var genTwo = new TimeSeriesGenerator("Node 2 Gen", tsTwoGen);
            var node1 = new Node("Denmark", tsOneLoad);
            var node2 = new Node("Germany", tsTwoLoad);
            node1.Generators.Add(genOne);
            node2.Generators.Add(genTwo);
            var nodes = new List<Node> { node1, node2 };
            var edges = BusinessLogic.Utils.Utils.StraightLine(nodes);

            // Create fake data: No flow needed.
            tsOneLoad.AppendData(5);
            tsOneLoad.AppendData(5);
            tsTwoLoad.AppendData(5);
            tsTwoLoad.AppendData(5);
            tsOneGen.AppendData(5);
            tsOneGen.AppendData(5);
            tsTwoGen.AppendData(5);
            tsTwoGen.AppendData(5);
            // Run model.
            RunModel(nodes, edges, 0, 2);

            // Create fake data: 1 flow is needed.
            tsOneLoad.AppendData(5);
            tsOneLoad.AppendData(5);
            tsTwoLoad.AppendData(5);
            tsTwoLoad.AppendData(5);
            tsOneGen.AppendData(6);
            tsOneGen.AppendData(4);
            tsTwoGen.AppendData(4);
            tsTwoGen.AppendData(6);
            // Run model.
            RunModel(nodes, edges, 1, 4);

            // Create fake data: 5 flow is needed (not enough generation).
            tsOneLoad.AppendData(25);
            tsOneLoad.AppendData(25);
            tsTwoLoad.AppendData(25);
            tsTwoLoad.AppendData(25);
            tsOneGen.AppendData(30);
            tsOneGen.AppendData(20);
            tsTwoGen.AppendData(20);
            tsTwoGen.AppendData(25);
            // Run model.
            RunModel(nodes, edges, 5, 6);

            // Create fake data: 10 flow is needed (excess generation).
            tsOneLoad.AppendData(50);
            tsOneLoad.AppendData(50);
            tsTwoLoad.AppendData(50);
            tsTwoLoad.AppendData(50);
            tsOneGen.AppendData(60);
            tsOneGen.AppendData(40);
            tsTwoGen.AppendData(40);
            tsTwoGen.AppendData(80);
            // Run model.
            RunModel(nodes, edges, 10, 8);
        }

        private void RunModel(List<Node> nodes, EdgeSet edges, double expected, int steps)
        {
            var model = new NetworkModel(nodes, new CooperativeExportStrategy(new MinimalFlowStrategy(nodes, edges)));
            var simulation = new Simulation(model);
            simulation.Simulate(steps);
            var flowTs = simulation.Output.TimeSeries.First(item => item.Properties.ContainsKey("Flow"));
            var capacity = StatUtils.CalcCapacity(flowTs.GetAllValues().OrderBy(item => item).ToList());
            Assert.AreEqual(expected, capacity, 1e-6);
        }

    }
}
