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

        public SimpleCore(ISimulationController controller, int length = 32, List<CountryNode> nodes = null)
        {
            _mCtrl = controller;
            _mConfig = new ModelYearConfig
            {
                Parameters = new Dictionary<string, KeyValuePair<int, double>>
                {
                    {"be", new KeyValuePair<int, double>(0, 1.0/length)},
                    {"bc", new KeyValuePair<int, double>(0, 1)},
                    {"tc", new KeyValuePair<int, double>(0, 1)}

                }
            };

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
                Scheme = ExportScheme.UnconstrainedSynchronized
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
    /// Do THREE simulations, one for Kb, Kc and Tc,
    /// </summary>
    public class ModelYearCore : IParameterEvaluatorCore
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
        public ModelYearCore(ModelYearConfig config)
        {
            _mConfig = config;
            // Backup energy controller.
            _mBeCtrl = new SimulationController() {CacheEnabled = false};
            _mBeCtrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedSynchronized
            });
            _mBeCtrl.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["be"].Key });
            _mBeCtrl.NodeFuncs.Clear();
            _mBeCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return Nodes;
            });
            // Backup capacity controller.
            _mBcCtrl = new SimulationController() { CacheEnabled = false };
            _mBcCtrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedSynchronized
            });
            _mBcCtrl.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["bc"].Key });
            _mBcCtrl.NodeFuncs.Clear();
            _mBcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return Nodes;
            });
            // Transmission capacity controller.
            _mTcCtrl = new SimulationController() { CacheEnabled = false };
            _mTcCtrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedSynchronized
            });
            _mTcCtrl.LogFlows = true;
            _mTcCtrl.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["tc"].Key });
            _mTcCtrl.NodeFuncs.Clear();
            _mTcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return Nodes;
            });
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
                var config = FileUtils.FromJsonFile<ModelYearConfig>(@"C:\Users\Emil\Dropbox\Master Thesis\noStorageAlpha0.5to1Gamma0.5to2.txt");
                _mCore = new ModelYearCore(config);
            }
        }

        public ParameterEvaluator(IParameterEvaluatorCore core)
        {
            _mCore = core;
        }

        #region Data evaluation

        // Sigma
        public double Sigma(NodeGenes nodeGenes)
        {
            var data = _mCore.BeController.EvaluateTs(nodeGenes);
            var ts = data[0].TimeSeries.Single(item => item.Name.Equals("Mismatch"));
            var std = ts.GetAllValues().StdDev(item => item/CountryInfo.GetMeanLoadSum());
            return std;
        }

        // Capacity factor
        public double CapacityFactor(NodeGenes nodeGenes)
        {
            var windCF =
                nodeGenes.Select(
                    item =>
                        item.Value.Alpha*item.Value.Gamma*CountryInfo.GetMeanLoad(item.Key)*
                        CountryInfo.GetOnshoreWindCf(item.Key)).Sum()/
                CountryInfo.GetMeanLoadSum();
            var solarCF = nodeGenes.Select(
                item =>
                    (1 - item.Value.Alpha)*item.Value.Gamma*CountryInfo.GetMeanLoad(item.Key)*
                    CountryInfo.GetSolarCf(item.Key)).Sum()/
                          CountryInfo.GetMeanLoadSum();

            return (windCF + solarCF);
        }

        // Individual link capacities.
        public Dictionary<string,double> LinkCapacities(NodeGenes nodeGenes)
        {
            var config = _mCore.Config.Parameters["tc"];
            
            // Run simulation.
            var data = _mCore.TcController.EvaluateTs(nodeGenes);
            // Extract system values.
            var result = new Dictionary<string, double>();
            var flowTs = data[0].TimeSeries.Where(item => item.Properties.ContainsKey("Flow"));
            foreach (var ts in flowTs)
            {
                var capacity = MathUtils.CalcCapacity(ts.GetAllValues());
                result.Add(Costs.GetKey(ts.Properties["From"], ts.Properties["To"]), capacity* config.Value);
            }

            return result;
        }

        // Total capacity weighted by lengths.
        public double TransmissionCapacity(NodeGenes nodeGenes)
        {
            return LinkCapacities(nodeGenes).Select(item => Costs.LinkLength[item.Key]*item.Value).Sum();
        }

        // Total backup capacity.
        public double BackupCapacity(NodeGenes nodeGenes)
        {
            var config = _mCore.Config.Parameters["bc"];

            // Run simulation.
            var data = _mCore.BcController.EvaluateTs(nodeGenes);
            // Extract system values.
            var bc = 0.0;
            var balancingTs = data[0].TimeSeries.Where(item => item.Name.Contains("Balancing"));
            foreach (var ts in balancingTs)
            {
                bc += MathUtils.Percentile(ts.GetAllValues().Select(item => Math.Max(0,-item)), 99);
            }

            return bc * config.Value;
        }

        // Total backup energy.
        public double BackupEnergy(NodeGenes nodeGenes)
        {
            var config = _mCore.Config.Parameters["be"];

            // Run simulation.
            var data = _mCore.BeController.EvaluateTs(nodeGenes);
            // Extract system values.
            var bc = 0.0;
            var balancingTs = data[0].TimeSeries.Where(item => item.Name.Contains("Balancing"));
            foreach (var ts in balancingTs)
            {
                bc += ts.GetAllValues().Select(item => Math.Max(0,-item)).Sum();
            }

            return bc * config.Value;
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
