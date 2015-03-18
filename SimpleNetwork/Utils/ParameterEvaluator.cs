﻿using System;
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
        List<CountryNode> Nodes { get; }
        ModelYearConfig Config { get; }
        ISimulationController BeController { get; }
        ISimulationController BcController { get; }
        ISimulationController TcController { get; }
    }

    public class SimpleCore : IParameterEvaluatorCore
    {

        public List<CountryNode> Nodes { get; private set; }
        public ModelYearConfig Config { get { return _mConfig; } }
        public ISimulationController BeController { get { return _mCtrl; } }
        public ISimulationController BcController { get { return _mCtrl; } }
        public ISimulationController TcController { get { return _mCtrl; } }

        private readonly ModelYearConfig _mConfig;
        private readonly ISimulationController _mCtrl;

        public SimpleCore(ISimulationController controller, int length = 32, List<CountryNode> nodes = null, ModelYearConfig config = null)
        {
            _mCtrl = controller;

            if (config == null)
            {
                config = new ModelYearConfig
                {
                    Parameters = new Dictionary<string, KeyValuePair<int, double>>
                    {
                        {"be", new KeyValuePair<int, double>(0, 1.0/length)},
                        {"bc", new KeyValuePair<int, double>(0, 1)},
                        {"tc", new KeyValuePair<int, double>(0, 1)}

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

        public FullCore(int length = 32, List<CountryNode> nodes = null)
        {
            if (nodes == null) nodes = ConfigurationUtils.CreateNodesNew();

            // Transmission eval.
            var ctrl = new SimulationController();
            ctrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedLocalized
            });
            ctrl.LogFlows = true;
            ctrl.Sources.Add(new TsSourceInput { Length = length, Offset = 0 });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("No storage", input => nodes);

            _mCore = new SimpleCore(ctrl, length, nodes);
        }

        #region Delegation

        private readonly SimpleCore _mCore;

        public List<CountryNode> Nodes
        {
            get { return _mCore.Nodes; }
        }

        public ModelYearConfig Config
        {
            get { return _mCore.Config; }
        }

        public ISimulationController BeController
        {
            get { return _mCore.BeController; }
        }

        public ISimulationController BcController
        {
            get { return _mCore.BcController; }
        }

        public ISimulationController TcController
        {
            get { return _mCore.TcController; }
        }

        #endregion

    }

    /// <summary>
    /// Do simulation with ONE YEAR only.
    /// </summary>
    public class FastCore : IParameterEvaluatorCore
    {

        public FastCore(ModelYearConfig config)
        {
            var nodes = ConfigurationUtils.CreateNodesNew();
            
            // Backup controller.
            var ctrl = new SimulationController();
            ctrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.UnconstrainedSynchronized
            });
            ctrl.Sources.Add(new TsSourceInput { Length = 1, Offset = config.Parameters["be"].Key });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return nodes;
            });

            _mCore = new SimpleCore(ctrl, 1, nodes);
        }

        #region Delegation

        private readonly SimpleCore _mCore;

        public List<CountryNode> Nodes
        {
            get { return _mCore.Nodes; }
        }

        public ModelYearConfig Config
        {
            get { return _mCore.Config; }
        }

        public ISimulationController BeController
        {
            get { return _mCore.BeController; }
        }

        public ISimulationController BcController
        {
            get { return _mCore.BcController; }
        }

        public ISimulationController TcController
        {
            get { return _mCore.TcController; }
        }

        #endregion

    }

    /// <summary>
    /// Do simulation WITHOUT flow at all.
    /// </summary>
    public class NoFlowCore : IParameterEvaluatorCore
    {

        public NoFlowCore(int length = 32, List<CountryNode> nodes = null)
        {
            if (nodes == null) nodes = ConfigurationUtils.CreateNodesNew();

            // Controller.
            var ctrl = new SimulationController();
            ctrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.None
            });
            ctrl.Sources.Add(new TsSourceInput { Length = length, Offset = 0 });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return nodes;
            });

            _mCore = new SimpleCore(ctrl, 1, nodes);
        }

        #region Delegation

        private readonly SimpleCore _mCore;

        public List<CountryNode> Nodes
        {
            get { return _mCore.Nodes; }
        }

        public ModelYearConfig Config
        {
            get { return _mCore.Config; }
        }

        public ISimulationController BeController
        {
            get { return _mCore.BeController; }
        }

        public ISimulationController BcController
        {
            get { return _mCore.BcController; }
        }

        public ISimulationController TcController
        {
            get { return _mCore.TcController; }
        }

        #endregion

    }

    /// <summary>
    /// Do ONE simulation only,
    /// </summary>
    public class ModelYearCore : IParameterEvaluatorCore
    {

        #region Delegation

        private readonly SimpleCore _mCore;

        public List<CountryNode> Nodes
        {
            get { return _mCore.Nodes; }
        }

        public ModelYearConfig Config
        {
            get { return _mCore.Config; }
        }

        public ISimulationController BeController
        {
            get { return _mCore.BeController; }
        }

        public ISimulationController BcController
        {
            get { return _mCore.BcController; }
        }

        public ISimulationController TcController
        {
            get { return _mCore.TcController; }
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
            ctrl.Sources.Add(new TsSourceInput { Length = 1, Offset = config.Parameters["be"].Key }); // SAME OFFSET: CRUTIAL!!
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("No storage", input => nodes);

            _mCore = new SimpleCore(ctrl, 1, nodes);
        }

    }

    /// <summary>
    /// Do THREE simulations, one for Kb, Kc and Tc,
    /// </summary>
    public class TripleModelYearCore : IParameterEvaluatorCore
    {
        public List<CountryNode> Nodes { get; private set; }
        public ModelYearConfig Config { get { return _mConfig; } }
        public ISimulationController BeController { get { return _mBeCtrl; } }
        public ISimulationController BcController { get { return _mBcCtrl; } }
        public ISimulationController TcController { get { return _mTcCtrl; } }

        private readonly ModelYearConfig _mConfig;
        private readonly ISimulationController _mBeCtrl;
        private readonly ISimulationController _mBcCtrl;
        private readonly ISimulationController _mTcCtrl;

        // TODO: CHANGE TO SYNCHRONIZED
        public TripleModelYearCore(ModelYearConfig config)
        {
            _mConfig = config;
            // Backup energy controller.
            var beCtrl = new SimulationController {CacheEnabled = false};
            beCtrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedSynchronized
            });
            beCtrl.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["be"].Key });
            beCtrl.NodeFuncs.Clear();
            beCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return Nodes;
            });
            _mBeCtrl = beCtrl;
            // Backup capacity controller.
            var bcCtrl = new SimulationController() { CacheEnabled = false };
            bcCtrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedSynchronized
            });
            bcCtrl.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["bc"].Key });
            bcCtrl.NodeFuncs.Clear();
            bcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return Nodes;
            });
            _mBcCtrl = bcCtrl;
            // Transmission capacity controller.
            var tcCtrl = new SimulationController() { CacheEnabled = false };
            tcCtrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedSynchronized
            });
            tcCtrl.LogFlows = true;
            tcCtrl.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["tc"].Key });
            tcCtrl.NodeFuncs.Clear();
            tcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return Nodes;
            });
            _mTcCtrl = tcCtrl;
            // TODO: Make source configurable
            Nodes = ConfigurationUtils.CreateNodesNew();
        }

    }

    #endregion

    public class ParameterEvaluator
    {

        public List<CountryNode> Nodes { get { return _mCore.Nodes; }}

        public bool InvalidateCache
        {
            get { return _mCore.BcController.InvalidateCache; }
            set
            {
                _mCore.BcController.InvalidateCache = value;
                _mCore.BeController.InvalidateCache = value;
                _mCore.TcController.InvalidateCache = value;
            }
        }

        public bool CacheEnabled
        {
            get { return _mCore.BcController.CacheEnabled; }
            set
            {
                _mCore.BcController.CacheEnabled = value;
                _mCore.BeController.CacheEnabled = value;
                _mCore.TcController.CacheEnabled = value;
            }
        }

        // TODO: Enable switching cores on the fly...
        private readonly IParameterEvaluatorCore _mCore;

        public ParameterEvaluator(bool full)
        {
            if (full) _mCore = new FullCore();
            else
            {   
                // TODO: Make config a variable? For now, just use default...
                var config = FileUtils.FromJsonFile<ModelYearConfig>(@"C:\Users\Emil\Dropbox\Master Thesis\OneYearAlpha0.5to1Gamma0.5to2Sync.txt");
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
            var data = _mCore.BeController.EvaluateTs(nodeGenes);
            return Sigma(data[0]);
        }

        public static double Sigma(SimulationOutput data)
        {
            var ts = data.TimeSeries.Single(item => item.Name.Equals("Mismatch"));
            var std = ts.GetAllValues().StdDev(item => item / CountryInfo.GetMeanLoadSum());
            return std;
        }

        // Individual link capacities.
        public Dictionary<string,double> LinkCapacities(NodeGenes nodeGenes)
        {
            var config = _mCore.Config.Parameters["tc"];
            var data = _mCore.TcController.EvaluateTs(nodeGenes);
            return LinkCapacities(data[0], config.Value);
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
        public double TransmissionCapacity(NodeGenes nodeGenes)
        {
            return LinkCapacities(nodeGenes).Select(item => Costs.LinkLength[item.Key]*item.Value).Sum();
        }

        public static double TransmissionCapacity(SimulationOutput data, double scale = 1, int from = 0, int to = -1)
        {
            return LinkCapacities(data, scale, from, to).Select(item => Costs.LinkLength[item.Key] * item.Value).Sum();
        }

        // Total backup capacity.
        public double BackupCapacity(NodeGenes nodeGenes)
        {
            var config = _mCore.Config.Parameters["bc"];
            var data = _mCore.BcController.EvaluateTs(nodeGenes);
            return BackupCapacity(data[0], config.Value);
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

            return bc * 1;
        }

        // Total backup energy.
        public double BackupEnergy(NodeGenes nodeGenes)
        {
            var config = _mCore.Config.Parameters["be"];
            var data = _mCore.BeController.EvaluateTs(nodeGenes);
            return BackupEnergy(data[0], config.Value);
        }

        public static double BackupEnergy(SimulationOutput data, double scale = 1, int from = 0, int to = -1)
        {
            var bc = 0.0;
            var balancingTs = data.TimeSeries.Where(item => item.Name.Contains("Balancing"));
            foreach (var ts in balancingTs)
            {
                var values = ts.GetAllValues().Skip(from);
                if (to != -1) values = values.Take(to);
                bc += values.Select(item => Math.Max(0, -item)).Sum();
            }

            return bc * scale;
        }

        #endregion

    }

    public class ModelYearConfig
    {
        public double AlphaMin { get; set; }
        public double AlphaMax { get; set; }
        public double GammaMin { get; set; }
        public double GammaMax { get; set; }
        public Dictionary<string, KeyValuePair<int, double>> Parameters { get; set; }
    }

}
