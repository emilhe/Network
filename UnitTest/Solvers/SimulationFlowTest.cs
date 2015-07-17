using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Generators;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
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
            var tsDumb = new DenseTimeSeries("Empty ts");
            var tsOneLoad = new DenseTimeSeries("CountryNode 1 LoadTs");
            var tsTwoLoad = new DenseTimeSeries("CountryNode 2 LoadTs");
            var tsOneGen = new DenseTimeSeries("CountryNode 1 GenTs");
            var tsTwoGen = new DenseTimeSeries("CountryNode 2 GenTs");
            var genOne = new TimeSeriesGenerator("CountryNode 1 Gen", tsOneGen);
            var genTwo = new TimeSeriesGenerator("CountryNode 2 Gen", tsTwoGen);
            // Create fake data: No flow needed.
            tsOneLoad.AppendData(5);
            tsOneLoad.AppendData(5);
            tsTwoLoad.AppendData(5);
            tsTwoLoad.AppendData(5);
            tsOneGen.AppendData(5);
            tsOneGen.AppendData(5);
            tsTwoGen.AppendData(5);
            tsTwoGen.AppendData(5);
            tsDumb.AppendData(0);
            tsDumb.AppendData(0);
            tsDumb.AppendData(0);
            tsDumb.AppendData(0);
            tsDumb.AppendData(0);
            tsDumb.AppendData(0);
            tsDumb.AppendData(0);
            tsDumb.AppendData(0);
            // Create nodes.
            var node1 = new CountryNode(new ReModel("Denmark", tsOneLoad, tsDumb, tsDumb));
            var node2 = new CountryNode(new ReModel("Germany", tsTwoLoad, tsDumb, tsDumb));
            node1.Generators.Add(genOne);
            node2.Generators.Add(genTwo);
            var nodes = new[] { node1, node2 };
            var edges = Stuff.StraightLine(nodes);

            // Run model.
            RunModel(nodes, edges, 0, 2);
            RunNewModel(nodes, edges, 0, 2);

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
            RunNewModel(nodes, edges, 1, 4);

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
            RunNewModel(nodes, edges, 5, 6);

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
            RunNewModel(nodes, edges, 10, 8);
        }

        private void RunModel(CountryNode[] nodes, EdgeCollection edges, double expected, int steps)
        {
            var model = new NetworkModel(nodes, new ConLocalScheme(nodes, edges));
            var simulation = new SimulationCore(model);
            simulation.LogFlows = true;
            simulation.Simulate(steps);
            var flowTs = simulation.Output.TimeSeries.First(item => item.Properties.ContainsKey("Flow"));
            var capacity = MathUtils.CalcCapacity(flowTs.GetAllValues().OrderBy(item => item).ToList());
            Assert.AreEqual(expected, capacity, 1e-6);
        }

        private void RunNewModel(CountryNode[] nodes, EdgeCollection edges, double expected, int steps)
        {
            var model = new NetworkModel(nodes, new ConLocalScheme(nodes, edges));
            var simulation = new SimulationCore(model);
            simulation.LogFlows = true;
            simulation.Simulate(steps);       
            var flowTs = simulation.Output.TimeSeries.First(item => item.Properties.ContainsKey("Flow"));
            double capacity = 0;
            try
            {
                capacity = MathUtils.CalcCapacity(flowTs.GetAllValues().OrderBy(item => item).ToList());
            }
            catch (Exception ex)
            {
                // So far, just ignore...
            }
            Assert.AreEqual(expected, capacity, 1e-4);
        }

    }
}
