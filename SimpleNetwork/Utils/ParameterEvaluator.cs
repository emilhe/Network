using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Cost;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using SimpleImporter;
using Utils;
using Utils.Statistics;

namespace BusinessLogic.Utils
{

    #region Core

    public interface IParameterEvaluatorCore
    {
        CountryNode[] Nodes { get; }
        ModelYearConfig Config { get; }
        ISimulationController Controller { get; }
    }

    public class SimpleCore : IParameterEvaluatorCore
    {

        public CountryNode[] Nodes { get; private set; }
        public ModelYearConfig Config { get { return _mConfig; } }
        public ISimulationController Controller { get { return _mCtrl; } }

        private readonly ModelYearConfig _mConfig;
        private readonly ISimulationController _mCtrl;

        public SimpleCore(ISimulationController controller, int length = 32, CountryNode[] nodes = null, ModelYearConfig config = null)
        {
            _mCtrl = controller;

            if (config == null)
            {
                config = new ModelYearConfig
                {
                    Parameters = new Dictionary<string, double>
                    {
                        {"be", 1.0/length},
                        {"bc", 1},
                        {"tc", 1}

                    }
                };
            }
            _mConfig = config;

            if (nodes == null) nodes = ConfigurationUtils.CreateNodesNew();
            Nodes = nodes;
        }


    }

    /// <summary>
    /// Do only ONE simulation, but use all 32 years of data.
    /// </summary>
    public class FullCore : IParameterEvaluatorCore
    {

        public FullCore(int length = 32, Func<CountryNode[]> spawnFunc = null, string tag = "No storage", ExportScheme scheme = ExportScheme.UnconstrainedSynchronized)
        {
            var nodes = (spawnFunc == null) ? ConfigurationUtils.CreateNodesNew() : spawnFunc();

            // Transmission eval.
            var ctrl = new SimulationController();
            ctrl.ExportStrategies.Add(new ExportSchemeInput { Scheme = scheme });
            ctrl.LogFlows = true;
            ctrl.Sources.Add(new TsSourceInput { Length = length, Offset = 0 });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add(tag, input => nodes);

            _mCore = new SimpleCore(ctrl, length, nodes);
        }

        #region Delegation

        private readonly SimpleCore _mCore;

        public CountryNode[] Nodes
        {
            get { return _mCore.Nodes; }
        }

        public ModelYearConfig Config
        {
            get { return _mCore.Config; }
        }

        public ISimulationController Controller
        {
            get { return _mCore.Controller; }
        }

        #endregion

    }

    ///// <summary>
    ///// Do simulation with ONE YEAR only.
    ///// </summary>
    //public class FastCore : IParameterEvaluatorCore
    //{

    //    public FastCore(ModelYearConfig config)
    //    {
    //        var nodes = ConfigurationUtils.CreateNodesNew();

    //        // Backup controller.
    //        var ctrl = new SimulationController();
    //        ctrl.ExportStrategies.Add(new ExportSchemeInput
    //        {
    //            Scheme = ExportScheme.UnconstrainedSynchronized
    //        });
    //        ctrl.Sources.Add(new TsSourceInput { Length = 1, Offset = config.Parameters["be"].Key });
    //        ctrl.NodeFuncs.Clear();
    //        ctrl.NodeFuncs.Add("No storage", input =>
    //        {
    //            foreach (var node in Nodes)
    //                node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
    //            return nodes;
    //        });

    //        _mCore = new SimpleCore(ctrl, 1, nodes);
    //    }

    //    #region Delegation

    //    private readonly SimpleCore _mCore;

    //    public CountryNode[] Nodes
    //    {
    //        get { return _mCore.Nodes; }
    //    }

    //    public ModelYearConfig Config
    //    {
    //        get { return _mCore.Config; }
    //    }

    //    public ISimulationController BeController
    //    {
    //        get { return _mCore.BeController; }
    //    }

    //    public ISimulationController BcController
    //    {
    //        get { return _mCore.BcController; }
    //    }

    //    public ISimulationController Controller
    //    {
    //        get { return _mCore.Controller; }
    //    }

    //    #endregion

    //}

    /// <summary>
    /// Do ONE simulation only,
    /// </summary>
    public class ModelYearCore : IParameterEvaluatorCore
    {

        #region Delegation

        private readonly SimpleCore _mCore;

        public CountryNode[] Nodes
        {
            get { return _mCore.Nodes; }
        }

        public ModelYearConfig Config
        {
            get { return _mCore.Config; }
        }

        public ISimulationController Controller
        {
            get { return _mCore.Controller; }
        }

        #endregion

        public ModelYearCore(ModelYearConfig config)
        {
            var nodes = ConfigurationUtils.CreateNodesNew();

            // Transmission eval.
            var ctrl = new SimulationController();
            ctrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.UnconstrainedSynchronized
            });
            ctrl.LogFlows = true;
            ctrl.Sources.Add(new TsSourceInput { Length = 1, Offset = config.Offset });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("No storage", input => nodes);

            _mCore = new SimpleCore(ctrl, 1, nodes, config);
        }

    }

    //public class StorageModelYearCore : IParameterEvaluatorCore
    //{

    //    #region Delegation

    //    private readonly SimpleCore _mCore;

    //    public CountryNode[] Nodes
    //    {
    //        get { return _mCore.Nodes; }
    //    }

    //    public ModelYearConfig Config
    //    {
    //        get { return _mCore.Config; }
    //    }

    //    public ISimulationController BeController
    //    {
    //        get { return _mCore.BeController; }
    //    }

    //    public ISimulationController BcController
    //    {
    //        get { return _mCore.BcController; }
    //    }

    //    public ISimulationController Controller
    //    {
    //        get { return _mCore.Controller; }
    //    }

    //    #endregion

    //    public StorageModelYearCore()
    //    {
    //        var syncConfig =
    //            FileUtils.FromJsonFile<ModelYearConfig>(
    //                @"C:\Users\Emil\Dropbox\Master Thesis\OneYearAlpha0.5to1Gamma0.5to2Sync.txt");
    //        var nodes = ConfigurationUtils.CreateNodesNew();
    //        ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);

    //        // Transmission eval.
    //        var ctrl = new SimulationController();
    //        ctrl.ExportStrategies.Add(new ExportSchemeInput
    //        {
    //            Scheme = ExportScheme.UnconstrainedSynchronized
    //        });
    //        ctrl.LogFlows = true;
    //        ctrl.Sources.Add(new TsSourceInput {Length = 1, Offset = syncConfig.Offset}); // SAME OFFSET: CRUTIAL!!
    //        ctrl.NodeFuncs.Clear();
    //        ctrl.NodeFuncs.Add("6h storage sync", input => nodes);

    //        _mCore = new SimpleCore(ctrl, 1, nodes, syncConfig);
    //    }

    //}


    ///// <summary>
    ///// Do THREE simulations, one for Kb, Kc and Tc,
    ///// </summary>
    //public class TripleModelYearCore : IParameterEvaluatorCore
    //{
    //    public CountryNode[] Nodes { get; private set; }
    //    public ModelYearConfig Config { get { return _mConfig; } }
    //    public ISimulationController BeController { get { return _mBeCtrl; } }
    //    public ISimulationController BcController { get { return _mBcCtrl; } }
    //    public ISimulationController Controller { get { return _mCtrl; } }

    //    private readonly ModelYearConfig _mConfig;
    //    private readonly ISimulationController _mBeCtrl;
    //    private readonly ISimulationController _mBcCtrl;
    //    private readonly ISimulationController _mCtrl;

    //    // TODO: CHANGE TO SYNCHRONIZED
    //    public TripleModelYearCore(ModelYearConfig config)
    //    {
    //        _mConfig = config;
    //        // Backup energy controller.
    //        var beCtrl = new SimulationController {CacheEnabled = false};
    //        beCtrl.ExportStrategies.Add(new ExportSchemeInput
    //        {
    //            Scheme = ExportScheme.ConstrainedSynchronized
    //        });
    //        beCtrl.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["be"].Key });
    //        beCtrl.NodeFuncs.Clear();
    //        beCtrl.NodeFuncs.Add("No storage", input =>
    //        {
    //            foreach (var node in Nodes)
    //                node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
    //            return Nodes;
    //        });
    //        _mBeCtrl = beCtrl;
    //        // Backup capacity controller.
    //        var bcCtrl = new SimulationController() { CacheEnabled = false };
    //        bcCtrl.ExportStrategies.Add(new ExportSchemeInput
    //        {
    //            Scheme = ExportScheme.ConstrainedSynchronized
    //        });
    //        bcCtrl.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["bc"].Key });
    //        bcCtrl.NodeFuncs.Clear();
    //        bcCtrl.NodeFuncs.Add("No storage", input =>
    //        {
    //            foreach (var node in Nodes)
    //                node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
    //            return Nodes;
    //        });
    //        _mBcCtrl = bcCtrl;
    //        // Transmission capacity controller.
    //        var tcCtrl = new SimulationController() { CacheEnabled = false };
    //        tcCtrl.ExportStrategies.Add(new ExportSchemeInput
    //        {
    //            Scheme = ExportScheme.ConstrainedSynchronized
    //        });
    //        tcCtrl.LogFlows = true;
    //        tcCtrl.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["tc"].Key });
    //        tcCtrl.NodeFuncs.Clear();
    //        tcCtrl.NodeFuncs.Add("No storage", input =>
    //        {
    //            foreach (var node in Nodes)
    //                node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
    //            return Nodes;
    //        });
    //        _mCtrl = tcCtrl;
    //        // TODO: Make source configurable
    //        Nodes = ConfigurationUtils.CreateNodesNew();
    //    }

    //}

    #endregion

    public class ParameterEvaluator
    {

        public CountryNode[] Nodes { get { return _mCore.Nodes; } }

        public bool InvalidateCache
        {
            get { return _mCore.Controller.InvalidateCache; }
            set { _mCore.Controller.InvalidateCache = value; }
        }

        public bool CacheEnabled
        {
            get { return _mCore.Controller.CacheEnabled; }
            set { _mCore.Controller.CacheEnabled = value; }
        }

        #region Memory cache

        private readonly Dictionary<NodeGenes, List<SimulationOutput>> _mMemoryCache = new Dictionary<NodeGenes, List<SimulationOutput>>();

        public bool MemoryCacheEnabled { get; set; }

        public void FlushMemoryCache()
        {
            _mMemoryCache.Clear();
        }

        private List<SimulationOutput> EvaluateTs(ISimulationController ctrl, NodeGenes genes)
        {
            if (!MemoryCacheEnabled) return ctrl.EvaluateTs(genes);
            if (!_mMemoryCache.ContainsKey(genes)) _mMemoryCache.Add(genes, ctrl.EvaluateTs(genes));
            return _mMemoryCache[genes];
        }

        #endregion

        // TODO: Enable switching cores on the fly...
        private readonly IParameterEvaluatorCore _mCore;

        public ParameterEvaluator(bool full)
        {
            if (full) _mCore = new FullCore();
            else
            {
                // TODO: Make config a variable? For now, just use default...
                var config = FileUtils.FromJsonFile<ModelYearConfig>(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_prod\model_year\Alpha0.5to1Gamma0.5to2Sync.txt");
                _mCore = new ModelYearCore(config);
            }
        }

        public ParameterEvaluator(IParameterEvaluatorCore core)
        {
            _mCore = core;
        }

        #region Data evaluation

        // Capacity factor
        public double CapacityFactor(NodeGenes nodeGenes)
        {
            var windCF =
                nodeGenes.Select(
                    item =>
                        item.Value.Alpha * item.Value.Gamma * CountryInfo.GetMeanLoad(item.Key) *
                        CountryInfo.GetOnshoreWindCf(item.Key)).Sum() /
                CountryInfo.GetMeanLoadSum();
            var solarCF = nodeGenes.Select(
                item =>
                    (1 - item.Value.Alpha) * item.Value.Gamma * CountryInfo.GetMeanLoad(item.Key) *
                    CountryInfo.GetSolarCf(item.Key)).Sum() /
                          CountryInfo.GetMeanLoadSum();

            return (windCF + solarCF);
        }

        // Sigma
        public double Sigma(NodeGenes nodeGenes)
        {
            var data = EvaluateTs(_mCore.Controller, nodeGenes);
            return Sigma(data[0]);
        }

        public static double Sigma(SimulationOutput data)
        {
            var ts = data.TimeSeries.Single(item => item.Name.Equals("Mismatch"));
            var std = ts.GetAllValues().StdDev(item => item / CountryInfo.GetMeanLoadSum());
            return std;
        }

        // Individual link capacities.
        public Dictionary<string, double> LinkCapacities(NodeGenes nodeGenes)
        {
            var config = _mCore.Config.Parameters["tc"];
            var data = EvaluateTs(_mCore.Controller, nodeGenes);
            return LinkCapacities(data[0], config);
        }

        public static Dictionary<string, double> LinkCapacities(SimulationOutput data, double scale = 1, int from = 0, int to = -1)
        {
            // Extract system values.
            var result = new Dictionary<string, double>();
            var flowTs = data.TimeSeries.Where(item => item.Properties.ContainsKey("Flow"));
            foreach (var ts in flowTs)
            {
                var values = ts.GetAllValues().Skip(from);
                if (to != -1) values = values.Take(to);
                var capacity = MathUtils.CalcCapacity(values);
                result.Add(Costs.GetKey(ts.Properties["From"], ts.Properties["To"]), capacity * scale);
            }

            return result;
        }

        // Total capacity weighted by lengths.
        public double TransmissionCapacity(Dictionary<string, double> capacities)
        {
            return capacities.Select(item => Costs.LinkLength[item.Key] * item.Value).Sum();
        }

        public double TransmissionCapacity(NodeGenes nodeGenes)
        {
            return LinkCapacities(nodeGenes).Select(item => Costs.LinkLength[item.Key] * item.Value).Sum();
        }

        public static double TransmissionCapacity(SimulationOutput data, double scale = 1, int from = 0, int to = -1)
        {
            return LinkCapacities(data, scale, from, to).Select(item => Costs.LinkLength[item.Key] * item.Value).Sum();
        }

        // Total backup capacity.
        public double BackupCapacity(NodeGenes nodeGenes)
        {
            var config = _mCore.Config.Parameters["bc"];
            var data = EvaluateTs(_mCore.Controller, nodeGenes);
            return BackupCapacity(data[0], config);
        }

        public static double BackupCapacity(SimulationOutput data, double scale = 1, int from = 0, int to = -1)
        {
            var bc = 0.0;
            var balancingTs = data.TimeSeries.Where(item => item.Name.Contains("Balancing"));
            foreach (var ts in balancingTs)
            {
                var values = ts.GetAllValues().Skip(from);
                if (to != -1) values = values.Take(to);
                bc += MathUtils.Percentile(values.Select(item => Math.Max(0, -item)), 99);
            }

            return bc * scale;
        }

        // Total backup energy.
        public double BackupEnergy(NodeGenes nodeGenes)
        {
            var config = _mCore.Config.Parameters["be"];
            var data = EvaluateTs(_mCore.Controller, nodeGenes);
            return BackupEnergy(data[0], config);
        }

        public static double BackupEnergy(SimulationOutput data, double scale = 1, int from = 0, int to = -1)
        {
            var be = 0.0;
            var balancingTs = data.TimeSeries.Where(item => item.Name.Contains("Balancing"));
            foreach (var ts in balancingTs)
            {
                var values = ts.GetAllValues().Skip(from);
                if (to != -1) values = values.Take(to);
                be += values.Select(item => Math.Max(0, -item)).Sum();
            }

            return be * scale;
        }

        #endregion

    }

    public class ModelYearConfig
    {
        public double AlphaMin { get; set; }
        public double AlphaMax { get; set; }
        public double GammaMin { get; set; }
        public double GammaMax { get; set; }
        public int Offset { get; set; }
        public Dictionary<string, double> Parameters { get; set; }
    }

}
