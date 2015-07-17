using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Cost;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.Utils;
using NUnit.Framework;
using SimpleImporter;

namespace UnitTest
{

    [TestFixture]
    class ExportSchemeTest
    {

        private double delta = 1e-2;

        //[Test]
        //public void Compare()
        //{
        //    var uncSync = new SimulationController();
        //    uncSync.ExportStrategies.Add(new ExportSchemeInput
        //    {
        //        Scheme = ExportScheme.UnconstrainedSynchronized
        //    });
        //    var uncLocal = new SimulationController();
        //    uncLocal.ExportStrategies.Add(new ExportSchemeInput
        //    {
        //        Scheme = ExportScheme.UnconstrainedLocalized
        //    });
        //    var conLocal = new SimulationController();
        //    conLocal.ExportStrategies.Add(new ExportSchemeInput
        //    {
        //        Scheme = ExportScheme.ConstrainedLocalized
        //    });
        //    var conSync = new SimulationController();
        //    conSync.ExportStrategies.Add(new ExportSchemeInput
        //    {
        //        Scheme = ExportScheme.ConstrainedSynchronized
        //    });
        //    var ctrls = new[] { uncSync, uncLocal, conLocal, conSync };
        //    // Common setup.
        //    var nodes = ConfigurationUtils.CreateNodes();
        //    CommonSetup(ctrls, nodes);
        //    // Do the magic.
        //    foreach (var ctrl in ctrls)
        //    {
        //        Console.WriteLine("It took: {0} for {1}", Benchmark(ctrl, nodes), ctrl.ExportStrategies[0].Scheme);
        //    }
        //}

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
            var nodes = ConfigurationUtils.CreateNodes(TsSource.ISET, 0);
            CommonSetup(ctrls, nodes);
            var cost = GetDetailedSystemCosts(ctrls[0], nodes);
            // Assertions.
            Assert.AreEqual(cost["Transmission"], 7.1540392585009407, delta);
            Assert.AreEqual(cost["Backup"], 5.31600106329718, delta);
            Assert.AreEqual(cost["Fuel"], 10.3439799184806, delta);
        }

        [Test]
        public void ConstrainedSynchronized()
        {
            var uncSync = new SimulationController();
            uncSync.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedSynchronized
            });
            var ctrls = new[] { uncSync };
            // Common setup.
            var nodes = ConfigurationUtils.CreateNodes(TsSource.ISET, 0);
            CommonSetup(ctrls, nodes);
            var cost = GetDetailedSystemCosts(ctrls[0], nodes);
            // Assertions.
            Assert.AreEqual(cost["Transmission"], 7.1540392585009407, delta);
            Assert.AreEqual(cost["Backup"], 5.31600106329718, delta);
            Assert.AreEqual(cost["Fuel"], 10.3439799184806, delta);
        }

        //[Test]
        //public void UnconstrainedLocalized()
        //{
        //    var uncLocal = new SimulationController();
        //    uncLocal.ExportStrategies.Add(new ExportSchemeInput
        //    {
        //        Scheme = ExportScheme.UnconstrainedLocalized
        //    });
        //    var ctrls = new[] { uncLocal };
        //    // Common setup.
        //    var nodes = ConfigurationUtils.CreateNodes(TsSource.ISET, 0);
        //    CommonSetup(ctrls, nodes);
        //    var cost = GetDetailedSystemCosts(ctrls[0], nodes);
        //    // Assertions.
        //    Assert.AreEqual(cost["Transmission"], 5.46017035325324, delta);
        //    Assert.AreEqual(cost["Backup"], 7.20753402651722, delta);
        //    Assert.AreEqual(cost["Fuel"], 10.3439799184806, delta);
        //}

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
            var nodes = ConfigurationUtils.CreateNodes(TsSource.ISET, 0);
            CommonSetup(ctrls, nodes);
            var cost = GetDetailedSystemCosts(ctrls[0], nodes);
            // Assertions.
            Assert.AreEqual(cost["Transmission"], 5.37290868006467, delta);
            Assert.AreEqual(cost["Backup"], 7.1540392585009407, delta);
            Assert.AreEqual(cost["Fuel"], 10.3439799184806, delta);
        }

        private void CommonSetup(SimulationController[] ctrls, CountryNode[] nodes)
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

        private double Benchmark(SimulationController ctrl, CountryNode[] nodes)
        {
            var simpleCore = new SimpleCore(ctrl, 1, nodes);
            var now = DateTime.Now;
            simpleCore.Controller.EvaluateTs(new NodeGenes(1, 1));
            return DateTime.Now.Subtract(now).TotalMilliseconds;
        }

        private Dictionary<string, double> GetDetailedSystemCosts(SimulationController ctrl, CountryNode[] nodes)
        {
            var simpleCore = new SimpleCore(ctrl, 1, nodes);
            var eval = new ParameterEvaluator(simpleCore);
            return (new NodeCostCalculator(eval)).DetailedSystemCosts(new NodeGenes(1,1));
        }

    }
}
