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
        SimulationController BeController { get; }
        SimulationController BcController { get; }
        SimulationController TcController { get; }
    }

    public class FullCore : IParameterEvaluatorCore
    {
        public List<CountryNode> Nodes { get; private set; }
        public ModelYearConfig Config { get { return _mConfig; } }
        public SimulationController BeController { get { return _mCtrlWithTrans; } }
        public SimulationController BcController { get { return _mCtrlWithTrans; } }
        public SimulationController TcController{get{return _mCtrlWithTrans;}}

        private readonly ModelYearConfig _mConfig;
        private readonly SimulationController _mCtrlWithTrans;
        //private readonly SimulationController _mCtrlWithoutTrans;

        public FullCore(int length = 32, List<CountryNode> nodes = null)
        {
            // Transmission eval.
            _mCtrlWithTrans = new SimulationController();
            _mCtrlWithTrans.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.ConstrainedFlow
            });
            _mCtrlWithTrans.LogFlows = true;
            _mCtrlWithTrans.LogNodalBalancing = true;
            _mCtrlWithTrans.LogSystemProperties = true;
            _mCtrlWithTrans.Sources.Add(new TsSourceInput { Length = length, Offset = 0 });
            _mCtrlWithTrans.NodeFuncs.Clear();
            _mCtrlWithTrans.NodeFuncs.Add("No storage", input => Nodes);
            //// The other eval.
            //_mCtrlWithoutTrans = new SimulationController();
            //_mCtrlWithoutTrans.ExportStrategies.Add(new ExportStrategyInput
            //{
            //    ExportStrategy = ExportStrategy.Cooperative,
            //    DistributionStrategy = DistributionStrategy.SkipFlow
            //});
            //_mCtrlWithoutTrans.LogLevel = LogLevelEnum.System;
            //_mCtrlWithoutTrans.Sources.Add(new TsSourceInput { Length = 32, Offset = 0 });
            //_mCtrlWithoutTrans.NodeFuncs.Clear();
            //_mCtrlWithoutTrans.NodeFuncs.Add("No storage", input => Nodes);
            // The config (fake).
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

    public class ModelYearCore : IParameterEvaluatorCore
    {
        public List<CountryNode> Nodes { get; private set; }
        public ModelYearConfig Config { get { return _mConfig; } }
        public SimulationController BeController { get { return _mBeCtrl; } }
        public SimulationController BcController { get { return _mBcCtrl; } }
        public SimulationController TcController { get { return _mTcCtrl; } }

        private readonly ModelYearConfig _mConfig;
        private readonly SimulationController _mBeCtrl;
        private readonly SimulationController _mBcCtrl;
        private readonly SimulationController _mTcCtrl;

        public ModelYearCore(ModelYearConfig config)
        {
            _mConfig = config;
            // Backup energy controller.
            _mBeCtrl = new SimulationController();
            _mBeCtrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.Cooperative,
                DistributionStrategy = DistributionStrategy.SkipFlow
            });
            _mBeCtrl.LogSystemProperties = true;
            _mBeCtrl.Sources.Add(new TsSourceInput {Length = 1, Offset = _mConfig.Parameters["be"].Key});
            _mBeCtrl.NodeFuncs.Clear();
            _mBeCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int) input.Offset*Stuff.HoursInYear);
                return Nodes;
            });
            // Backup capacity controller.
            _mBcCtrl = new SimulationController();
            _mBcCtrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.Cooperative,
                DistributionStrategy = DistributionStrategy.SkipFlow
            });
            _mBcCtrl.LogNodalBalancing = true;
            _mBcCtrl.Sources.Add(new TsSourceInput {Length = 1, Offset = _mConfig.Parameters["bc"].Key});
            _mBcCtrl.NodeFuncs.Clear();
            _mBcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int) input.Offset*Stuff.HoursInYear);
                return Nodes;
            });
            // Transmission capacity controller.
            _mTcCtrl = new SimulationController();
            _mTcCtrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.ConstrainedFlow
            });
            _mTcCtrl.LogFlows = true;
            _mTcCtrl.Sources.Add(new TsSourceInput {Length = 1, Offset = _mConfig.Parameters["tc"].Key});
            _mTcCtrl.NodeFuncs.Clear();
            _mTcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int) input.Offset*Stuff.HoursInYear);
                return Nodes;
            });
            // TODO: Make source configurable
            Nodes = ConfigurationUtils.CreateNodesNew();
        }

    }

    public class FastCore : IParameterEvaluatorCore
    {
        public List<CountryNode> Nodes { get; private set; }
        public ModelYearConfig Config { get { return _mConfig; } }
        public SimulationController BeController { get { return _mCtrlTrans; } }
        public SimulationController BcController { get { return _mCtrlTrans; } }
        public SimulationController TcController { get { return _mCtrlTrans; } }

        private readonly SimulationController _mCtrlTrans;

        private readonly ModelYearConfig _mConfig;

        public FastCore(ModelYearConfig config)
        {
            _mConfig = config;
            // Backup controller.
            _mCtrlTrans = new SimulationController();
            _mCtrlTrans.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.Cooperative,
                DistributionStrategy = DistributionStrategy.SkipFlow
            });
            _mCtrlTrans.LogNodalBalancing = true;
            _mCtrlTrans.Sources.Add(new TsSourceInput { Length = 1, Offset = _mConfig.Parameters["be"].Key });
            _mCtrlTrans.NodeFuncs.Clear();
            _mCtrlTrans.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return Nodes;
            });
            // TODO: Make source configurable
            Nodes = ConfigurationUtils.CreateNodesNew();
        }
    }

    public class NoFlowCore : IParameterEvaluatorCore
    {
        public List<CountryNode> Nodes { get; private set; }

        public ModelYearConfig Config
        {
            get { return _mConfig; }
        }

        public SimulationController BeController { get { return _mCtrl; } }
        public SimulationController BcController { get { return _mCtrl; } }
        public SimulationController TcController { get { return _mCtrl; } }

        private readonly ModelYearConfig _mConfig;
        private readonly SimulationController _mCtrl;

        public NoFlowCore(int length = 32, List<CountryNode> nodes = null)
        {
            // Controller.
            _mCtrl = new SimulationController();
            _mCtrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.None
            });
            _mCtrl.LogNodalBalancing = true;
            _mCtrl.LogSystemProperties = true;
            _mCtrl.Sources.Add(new TsSourceInput { Length = length, Offset = 0 });
            _mCtrl.NodeFuncs.Clear();
            _mCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int)input.Offset * Stuff.HoursInYear);
                return Nodes;
            });
            // The config (fake). 
            _mConfig = new ModelYearConfig
            {
                Parameters = new Dictionary<string, KeyValuePair<int, double>>
                {
                    {"be", new KeyValuePair<int, double>(0, 1.0/length)},
                    {"bc", new KeyValuePair<int, double>(0, 1)},
                    {"tc", new KeyValuePair<int, double>(0, 1)}

                }
            };
            // TODO: Make source configurable
            Nodes = nodes;
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
            var balancing = (DenseTimeSeries)data[0].TimeSeries.First(item => item.Name.Equals("Backup"));

            return balancing.GetAllValues().Sum() * config.Value;
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
