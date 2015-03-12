using System;
using System.Collections.Generic;
using BusinessLogic.Cost;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.Utils;
using NUnit.Framework;
using SimpleImporter;

namespace UnitTest
{

    [TestFixture]
    class PerformanceTest
    {

        [Test]
        public void Compare()
        {
            var uncSync = new SimulationController();
            uncSync.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.UnconstrainedSynchronized
            });
            var uncLocal = new SimulationController();
            uncLocal.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.UnconstrainedLocalized
            });
            var conLocal = new SimulationController();
            conLocal.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedLocalized
            });
            var ctrls = new[] { uncSync, uncLocal, conLocal };
            // Common setup.
            var nodes = ConfigurationUtils.CreateNodesNew();
            CommonSetup(ctrls, nodes);
            // Do the magic.
            foreach (var ctrl in ctrls)
            {
                Console.WriteLine("It took: {0} for {1}", Benchmark(ctrl, nodes), ctrl.ExportStrategies[0].Scheme);
            }
        }

        [Test]
        public void UnconstrainedSynchronized()
        {
            var uncSync = new SimulationController();
            uncSync.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.UnconstrainedSynchronized
            });
            var ctrls = new[] { uncSync};
            // Common setup.
            var nodes = ConfigurationUtils.CreateNodesNew();
            CommonSetup(ctrls, nodes);
            // Do the magic.
            foreach (var ctrl in ctrls)
            {
                Console.WriteLine("It took: {0} for {1}", Benchmark(ctrl, nodes), ctrl.ExportStrategies[0].Scheme);
            }
        }

        [Test]
        public void UnconstrainedLocalized()
        {
            var uncLocal = new SimulationController();
            uncLocal.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.UnconstrainedLocalized
            });
            var ctrls = new[] { uncLocal };
            // Common setup.
            var nodes = ConfigurationUtils.CreateNodesNew();
            CommonSetup(ctrls, nodes);
            // Do the magic.
            foreach (var ctrl in ctrls)
            {
                Console.WriteLine("It took: {0} for {1}", Benchmark(ctrl, nodes), ctrl.ExportStrategies[0].Scheme);
            }
        }

        [Test]
        public void ConstrainedLocalized()
        {
            var conLocal = new SimulationController();
            conLocal.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedLocalized
            });
            var ctrls = new[] { conLocal };
            // Common setup.
            var nodes = ConfigurationUtils.CreateNodesNew();
            CommonSetup(ctrls, nodes);
            // Do the magic.
            foreach (var ctrl in ctrls)
            {
                Console.WriteLine("It took: {0} for {1}", Benchmark(ctrl, nodes), ctrl.ExportStrategies[0].Scheme);
            }
        }

        private void CommonSetup(SimulationController[] ctrls, List<CountryNode> nodes)
        {
            // Common setup.
            foreach (var ctrl in ctrls)
            {
                ctrl.Sources.Add(new TsSourceInput { Length = 1, Offset = 0 });
                ctrl.NodeFuncs.Clear();
                ctrl.NodeFuncs.Add("No storage", input =>
                {
                    foreach (var node in nodes)
                        node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                    return nodes;
                });
                ctrl.CacheEnabled = false;
                ctrl.LogFlows = true;
            }
        }

        private double Benchmark(SimulationController ctrl, List<CountryNode> nodes)
        {
            var simpleCore = new SimpleCore(ctrl, 1, nodes);
            var now = DateTime.Now;
            simpleCore.TcController.EvaluateTs(new NodeGenes(1, 1));
            return DateTime.Now.Subtract(now).TotalMilliseconds;

            //var simpleCore = new SimpleCore(ctrl, 1, nodes);
            //var calc = new NodeCostCalculator(new ParameterEvaluator(simpleCore));
            //var now = DateTime.Now;
            //calc.SystemCost(new NodeGenes(1, 1), true);
            //return DateTime.Now.Subtract(now).TotalMilliseconds;
        }

    }
}
