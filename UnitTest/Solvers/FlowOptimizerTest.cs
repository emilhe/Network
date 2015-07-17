using System;
using System.Collections.Generic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Utils;
using NUnit.Framework;

namespace UnitTest
{

    [TestFixture]
    public class FlowOptimizerTest
    {

        private const double FlowDelta = 1e-3;

        [Test]
        public void TwoNodeTest()
        {
            var nodes = new double[] {2, -1};
            var nodeNames = new[] {"Node1", "Node2"};
            var builder = new EdgeBuilder(nodeNames);
            builder.Connect(0, 1);
            var edges = builder.ToEdges();
            var opt = new ConLocalOptimizer(edges, 0);
            // Test the most basic test case.
            opt.SetNodes(nodes, new List<double[]>(), new List<double[]>());
            opt.Solve();
            AreAlmostEqual(new double[] {1, 0}, opt.NodeOptima, FlowDelta*edges.NodeCount);
            // Test that changing nodes changes solut<ion.
            nodes = new double[] {2, -2};
            opt.SetNodes(nodes, null, null);
            opt.Solve();
            AreAlmostEqual(new double[] { 0, 0 }, opt.NodeOptima, FlowDelta * edges.NodeCount);
            // Test that charging capacity limits are respected
            nodes = new double[] {-1, 2};
            opt.SetNodes(nodes, null, null);
            opt.Solve();
            AreAlmostEqual(new double[] { 0, 1 }, opt.NodeOptima, FlowDelta * edges.NodeCount);
        }

        [Test]
        public void FourNodeAllEdgeTest()
        {
            var nodes = new double[] {2, -1, 4, -3};
            var nodeNames = new[] {"Node1", "Node2", "Node3", "Node4"};
            var builder = new EdgeBuilder(nodeNames);
            builder.Connect(0, 1);
            builder.Connect(0, 2);
            builder.Connect(0, 3);
            builder.Connect(1, 2);
            builder.Connect(1, 3);
            builder.Connect(2, 3);
            var edges = builder.ToEdges();
            var opt = new ConLocalOptimizer(edges, 0);
            // Test that the minimum flow is realised.
            opt.SetNodes(nodes, null, null);
            opt.Solve();
            var flowSum = 0.0;
            foreach (var flow in opt.Flows) flowSum += Math.Abs(flow);
            AreAlmostEqual(4.5, flowSum, FlowDelta*edges.NodeCount);
            AreAlmostEqual(new double[] {0, 0, 2, 0}, opt.NodeOptima, FlowDelta);
        }

        [Test]
        public void FourNodeFewEdgeTest()
        {
            var nodes = new double[] {2, -1, 4, -3};
            var nodeNames = new[] {"Node1", "Node2", "Node3", "Node4"};
            var builder = new EdgeBuilder(nodeNames);
            builder.Connect(0, 1);
            builder.Connect(0, 2);
            builder.Connect(2, 3);
            var edges = builder.ToEdges();
            var opt = new ConLocalOptimizer(edges, 0);
            // Test that the minimum flow is realised.
            opt.SetNodes(nodes, null, null);
            opt.Solve();
            var flowSum = 0.0;
            foreach (var flow in opt.Flows) flowSum += Math.Abs(flow);
            AreAlmostEqual(4, flowSum, FlowDelta*edges.NodeCount);
            AreAlmostEqual(new double[] {1, 0, 1, 0}, opt.NodeOptima, FlowDelta);
        }

        [Test]
        public void ChargeLimitTest()
        {
            var nodes = new double[] {2, -1};
            var nodeNames = new[] {"Node1", "Node2"};
            var builder = new EdgeBuilder(nodeNames);
            builder.Connect(0, 1);
            var edges = builder.ToEdges();
            var opt = new ConLocalOptimizer(edges, 1);
            // Test that charging capacity limits are respected
            opt.SetNodes(nodes, new List<double[]> { new double[] { 0, 0 } }, new List<double[]> { new double[] { 0, 1 } });
            opt.Solve();
            AreAlmostEqual(new double[,] {{0, -2}, {0, 0}}, opt.Flows, FlowDelta);
            AreAlmostEqual(new double[] {0, 0}, opt.NodeOptima, FlowDelta*edges.NodeCount);
            AreAlmostEqual(new double[] {0, 1}, opt.StorageOptima[0], FlowDelta*edges.NodeCount);
        }

        [Test]
        public void DischargeTest()
        {
            var nodes = new double[] {2, -3};
            var nodeNames = new[] {"Node1", "Node2"};
            var builder = new EdgeBuilder(nodeNames);
            builder.Connect(0, 1);
            var edges = builder.ToEdges();
            var opt = new ConLocalOptimizer(edges, 1);
            // Test the most basic test case.
            opt.SetNodes(nodes, new List<double[]> {new[] {0.0, -1}}, new List<double[]> {new[] {0.0, 0}});
            opt.Solve();
            AreAlmostEqual(new double[,] {{0, -2}, {0, 0}}, opt.Flows, FlowDelta);
            AreAlmostEqual(new double[] {0, 0}, opt.NodeOptima, FlowDelta*edges.NodeCount);
            AreAlmostEqual(new double[] {0, -1}, opt.StorageOptima[0], FlowDelta*edges.NodeCount);
        }

        [Test]
        public void StorageLevelTest()
        {
            var nodes = new double[] { 0, -3 };
            var nodeNames = new[] { "Node1", "Node2" };
            var builder = new EdgeBuilder(nodeNames);
            builder.Connect(0, 1);
            var edges = builder.ToEdges();
            var opt = new ConLocalOptimizer(edges, 2);
            // Test the most basic test case.
            opt.SetNodes(nodes, new List<double[]> { new[] { -1.0, 0 }, new[] { -2.0, -2.0 }},new List<double[]> { new[] { 0.0, 0.0 }, new[] { 0.0, 0 }});
            opt.Solve();
            AreAlmostEqual(new double[,] { { 0, -1 }, { 0, 0 } }, opt.Flows, FlowDelta);
            AreAlmostEqual(new double[] { 0, 0 }, opt.NodeOptima, FlowDelta * edges.NodeCount);
            AreAlmostEqual(new double[] { -1, 0 }, opt.StorageOptima[0], FlowDelta * edges.NodeCount);
            AreAlmostEqual(new double[] { 0, -2 }, opt.StorageOptima[1], FlowDelta * edges.NodeCount);
        }

        private static void AreAlmostEqual<T>(T expected, T result, double delta)
        {
            Assert.That(expected, Is.EqualTo(result).Within(delta));
        }

    }
}