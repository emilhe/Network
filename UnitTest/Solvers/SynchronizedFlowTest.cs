using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;
using BusinessLogic.Cost;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using NUnit.Framework;
using Utils;

namespace UnitTest
{
    class SynchronizedFlowTest
    {

        [Test]
        public void UnconstrainedSynchronizedFlowTest()
        {
            var ts = SpawnTs();
            // Normal events.
            for (int i = 1; i <= 5; i++)
            {
                ts[0].AppendData(0);
                ts[1].AppendData(2*i);
                ts[2].AppendData(-2);
                ts[3].AppendData(-1);
                ts[4].AppendData(-3);
                ts[5].AppendData(-2);
            }
            var ctrl = SpawnController(ts);
            ctrl.LogFlows = true;
            var results = ctrl.EvaluateTs(null).First().TimeSeries.Where(item => item.Properties.ContainsKey("Flow")).ToArray();
            // Validation.
            var ideal = new List<double[]>
            {
                new[] {1.6, 2.4, 3.2, 4, 4.8},
                new[] {1.6, 2.4, 3.2, 4, 4.8},
                new[] {0.15, 0.35, 0.55, 0.75, 0.95},
                new[] {0.65, 0.85, 1.05, 1.25, 1.45},
                new[] {0.5, 0.5, 0.5, 0.5, 0.5},
                new[] {-0.15, -0.35, -0.55, -0.75, -0.95},
                new[] {-0.65, -0.85, -1.05, -1.25, -1.45},
            };
            for (int index = 0; index < results.Length; index++)
            {
                ValidateResult(results[index], ideal[index]);
            }
        }

        private static void ValidateResult(ITimeSeries ts, double[] expected)
        {
            var i = 0;
            foreach (var item in ts)
            {
                Assert.AreEqual(expected[i], item.Value, 1e-5);
                i++;
            }
        }

        private static MockSimulationController SpawnController(DenseTimeSeries[] ts)
        {
            Costs.Unsafe = true;
            var nodes = SpawnNodes(ts);
            var model = new NetworkModel(nodes, new UncSyncScheme(nodes, Edges(nodes), new double[nodes.Count].Ones()));
            return new MockSimulationController(new SimulationCore(model))
            {
                Ticks = ts[0].Count
            };
        }

        private static List<INode> SpawnNodes(DenseTimeSeries[] ts)
        {
            return new List<INode>
            {
                new CountryNode(new ReModel("Denmark", ts[1], ts[0], ts[0], ts[0])),
                new CountryNode(new ReModel("Germany", ts[2], ts[0], ts[0], ts[0])),
                new CountryNode(new ReModel("Spain", ts[3], ts[0], ts[0], ts[0])),
                new CountryNode(new ReModel("Sweden", ts[4], ts[0], ts[0], ts[0])),
                new CountryNode(new ReModel("Norway", ts[5], ts[0], ts[0], ts[0]))
            };
        }

        private static DenseTimeSeries[] SpawnTs()
        {
            return new[]
            {
                new DenseTimeSeries("zero"),
                new DenseTimeSeries("top"),
                new DenseTimeSeries("upperLeft"),
                new DenseTimeSeries("lowerLeft"),
                new DenseTimeSeries("upperRight"),
                new DenseTimeSeries("lowerRight")
            };
        }

        private static EdgeCollection Edges(List<INode> nodes)
        {
            var builder = new EdgeBuilder(nodes.Select(item => item.Name).ToArray());
            builder.Connect(0, 1);
            builder.Connect(0, 4);
            builder.Connect(1, 2);
            builder.Connect(1, 3);
            builder.Connect(2, 3);
            builder.Connect(2, 4);
            builder.Connect(3, 4);
            return builder.ToEdges();
        }

        class MockSimulationController : ISimulationController
        {

            #region Not implemented

            public bool PrintDebugInfo
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool CacheEnabled
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool InvalidateCache
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public List<TsSourceInput> Sources
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public List<ExportSchemeInput> ExportStrategies
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public Dictionary<string, Func<TsSourceInput, List<CountryNode>>> NodeFuncs
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public Dictionary<string, Func<List<CountryNode>, EdgeCollection>> EdgeFuncs
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public Dictionary<string, Func<IFailureStrategy>> FailFuncs
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public List<SimulationOutput> EvaluateTs(double penetration, double mixing)
            {
                throw new NotImplementedException();
            }

            public List<GridResult> EvaluateGrid(GridScanParameters grid)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region Semi implemented

            public bool LogAllNodeProperties
            {
                get { throw new NotImplementedException(); }
                set { _mSimulationCore.LogAllNodeProperties = value; }
            }

            public bool LogFlows
            {
                get { throw new NotImplementedException(); }
                set { _mSimulationCore.LogFlows = value; }
            }

            #endregion

            public int Ticks { get; set; }

            private readonly SimulationCore _mSimulationCore;

            public MockSimulationController(SimulationCore core)
            {
                _mSimulationCore = core;
            }

            public List<SimulationOutput> EvaluateTs(NodeGenes genes)
            {
                _mSimulationCore.Simulate(Ticks);
                return new List<SimulationOutput> { _mSimulationCore.Output };
            }

        }

    }
}
