using System;
using System.Collections.Generic;
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
    public class ParameterEvaluationTest
    {

        [Test]
        public void BakupEnergyTest()
        {



            // TODO: Parameter evaluation tests!!!
        }

        [Test]
        public void TransmissionCapacityTest()
        {
            var ts = SpawnTs();
            // Zero events.
            //for (int i = 0; i <= 100; i++)
            //{
            //    ts[0].AppendData(0);
            //    ts[1].AppendData(2 * i);
            //    ts[2].AppendData(-2);
            //    ts[3].AppendData(-1);
            //    ts[4].AppendData(-3);
            //    ts[5].AppendData(-2);
            //}
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
            //// Extreme events.
            //ts[0].AppendData(0);
            //ts[1].AppendData(80);
            //ts[2].AppendData(-20);
            //ts[3].AppendData(-10);
            //ts[4].AppendData(-30);
            //ts[5].AppendData(-20);
            var eval = SpawnParamEval(ts);

            var tCap = eval.LinkCapacities(null);
            Assert.AreEqual(tCap["Denmark-Germany"], 4, 1e-3);
            Assert.AreEqual(tCap["Denmark-Norway"], 4, 1e-3);
            Assert.AreEqual(tCap["Germany-Spain"], 0.75, 1e-3);
            Assert.AreEqual(tCap["Germany-Sweden"], 1.25, 1e-3);
            Assert.AreEqual(tCap["Spain-Sweden"], 0.5, 1e-3);
            Assert.AreEqual(tCap["Spain-Norway"], 0.75, 1e-3);
            Assert.AreEqual(tCap["Sweden-Norway"], 1.25, 1e-3);

            var bCap = eval.BackupCapacity(null);
            var bEne = eval.BackupEnergy(null);
        }

        [Test]
        public void BackupCapacityTest()
        {



            // TODO: Parameter evaluation tests!!!
        }

        private static ParameterEvaluator SpawnParamEval(DenseTimeSeries[] ts)
        {
            Costs.Unsafe = true;
            var nodes = SpawnNodes(ts);
            var model = new NetworkModel(nodes, new ConLocalScheme(nodes, Edges(nodes)));
            var ctrl = new MockSimulationController(new SimulationCore(model))
            {
                LogAllNodeProperties = true,
                LogFlows = true,
                Ticks = ts[0].Count
            };
            return new ParameterEvaluator(new SimpleCore(ctrl, 1, nodes));
        }

        private static List<CountryNode> SpawnNodes(DenseTimeSeries[] ts)
        {
            return new List<CountryNode>
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

        private static EdgeCollection Edges(List<CountryNode> nodes)
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
                return new List<SimulationOutput> { _mSimulationCore.Output};
            }

        }

    }
}
