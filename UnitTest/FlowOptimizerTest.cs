﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleNetwork;

namespace UnitTest
{
    [TestFixture]
    public class FlowOptimizerTest
    {

        private const double FlowDelta = 1e-4;

        [TestFixtureSetUp]
        public void Init()
        {
            
        }

        [Test]
        public void TwoNodeTest()
        {
            var opt = new FlowOptimizer("TwoNodeTest.log", 2);
            var nodes = new double[] {2, -1};
            var edges = new EdgeSet(2);
            edges.AddEdge(0, 1);
            // Test the most basic test case.
            opt.SetEdges(edges);
            opt.SetNodes(nodes, new[]{0.0,0.0}, new[] { double.MaxValue, double.MaxValue });
            opt.Solve();
            AreAlmostEqual(new double[,]{{0,1},{0,0}}, opt.Flows, FlowDelta);
            AreAlmostEqual(new double[] { 1, 0 }, opt.NodeOptimum, FlowDelta * edges.NodeCount);
            // Test that changing nodes changes solution.
            nodes = new double[] { 2, -2 };
            opt.SetNodes(nodes, new[] { 0.0, 0.0 }, new[] { double.MaxValue, double.MaxValue });
            opt.Solve();
            AreAlmostEqual(new double[,] { { 0, 2 }, { 0, 0 } }, opt.Flows, FlowDelta);
            AreAlmostEqual(new double[] { 0, 0 }, opt.NodeOptimum, FlowDelta * edges.NodeCount);
            // Test that charging capacity limits are respected
            nodes = new double[] { 2, -1 };
            opt.SetNodes(nodes, new double[] { 0, 0 }, new double[] { 0, 1 });
            opt.Solve();
            AreAlmostEqual(new double[,] { { 0, 2 }, { 0, 0 } }, opt.Flows, FlowDelta);
            AreAlmostEqual(new double[] { 0, 1 }, opt.NodeOptimum, FlowDelta * edges.NodeCount);
        }

        [Test]
        public void FourNodeAllEdgeTest()
        {
            var opt = new FlowOptimizer("FourNodeAllEdgeTest.log", 4);
            var nodes = new double[] { 2, -1, 4, -3 };
            var edges = new EdgeSet(4);
            edges.AddEdge(0, 1);
            edges.AddEdge(0, 2);
            edges.AddEdge(0, 3);
            edges.AddEdge(1, 2);
            edges.AddEdge(1, 3);
            edges.AddEdge(2, 3);
            // Test that the minimum flow is realised.
            opt.SetEdges(edges);
            opt.SetNodes(nodes, new double[] { 0, 0, 0, 0 }, new[] { double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue });
            opt.Solve();
            var flowSum = 0.0;
            foreach (var flow in opt.Flows) flowSum += flow;
            AreAlmostEqual(4.5, flowSum, FlowDelta * edges.NodeCount);
            AreAlmostEqual(new double[] { 0, 0, 2, 0 }, opt.NodeOptimum, FlowDelta);
        }

        [Test]
        public void FourNodeFewEdgeTest()
        {
            var opt = new FlowOptimizer("FourNodeAllEdgeTest.log", 4);
            var nodes = new double[] { 2, -1, 4, -3 };
            var edges = new EdgeSet(4);
            edges.AddEdge(0, 1);
            edges.AddEdge(0, 2);
            edges.AddEdge(2, 3);
            // Test that the minimum flow is realised.
            opt.SetEdges(edges);
            opt.SetNodes(nodes, new double[] { 0, 0, 0, 0 }, new[] { double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue });
            opt.Solve();
            var flowSum = 0.0;
            foreach (var flow in opt.Flows) flowSum += flow;
            AreAlmostEqual(4, flowSum, FlowDelta * edges.NodeCount);
            AreAlmostEqual(new double[] { 1, 0, 1, 0 }, opt.NodeOptimum, FlowDelta);
        }
            
        [Test]
        public void ChargeLimitTest()
        {
            var opt = new FlowOptimizer("ChargeLimitTest.log", 2);
            var nodes = new double[] { 2, -1 };
            var edges = new EdgeSet(2);
            edges.AddEdge(0, 1);
            // Test that charging capacity limits are respected
            opt.SetEdges(edges);
            opt.SetNodes(nodes, new double[] { 0, 0}, new double[] { 0, 1 });
            opt.Solve();
            AreAlmostEqual(new double[,] { { 0, 2 }, { 0, 0 } }, opt.Flows, FlowDelta);
            AreAlmostEqual(new double[] { 0, 1 }, opt.NodeOptimum, FlowDelta * edges.NodeCount);
        }

        [Test]
        public void DischargeTest()
        {
            var opt = new FlowOptimizer("DischargeTest.log", 2);
            var nodes = new double[] { 2, -3 };
            var edges = new EdgeSet(2);
            edges.AddEdge(0, 1);
            // Test the most basic test case.
            opt.SetEdges(edges);
            opt.SetNodes(nodes, new[] { -1 - FlowDelta * edges.EdgeCount, 0 - FlowDelta * edges.EdgeCount },
                new[] { FlowDelta * edges.EdgeCount, FlowDelta * edges.EdgeCount });
            opt.Solve();
            AreAlmostEqual(new double[,] { { 0, 3 }, { 0, 0 } }, opt.Flows, FlowDelta);
            AreAlmostEqual(new double[] { -1, 0 }, opt.NodeOptimum, FlowDelta * edges.NodeCount);
        }

        private static void AreAlmostEqual<T>(T expected, T result, double delta)
        {
            Assert.That(expected, Is.EqualTo(result).Within(delta));
        }

    }

}
