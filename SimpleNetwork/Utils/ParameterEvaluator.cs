using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Cost;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using SimpleImporter;
using Utils;
using Utils.Statistics;

namespace BusinessLogic.Utils
{
    public class ParameterEvaluator
    {

        public List<CountryNode> Nodes { get; private set; }        
        
        public bool CacheEnabled
        {
            set
            {
                _mBcCtrl.CacheEnabled = value;
                _mBeCtrl.CacheEnabled = value;
                _mTcCtrl.CacheEnabled = value;
            }
        }

        private ModelYearConfig _mConfig;

        private SimulationController _mBeCtrl;
        private SimulationController _mBcCtrl;
        private SimulationController _mTcCtrl;

        #region Construction

        // Per default the alpha \in [0.5-1.0] and gamma \in [1:2] profile is loaded.
        public ParameterEvaluator(bool full = false)
        {
            if (full)
            {
                var fullConfig = new ModelYearConfig
                {
                    Parameters = new Dictionary<string, KeyValuePair<int, double>>
                    {
                        {"be", new KeyValuePair<int, double>(0, 1.0/32.0)},
                        {"bc", new KeyValuePair<int, double>(0, 1)},
                        {"tc", new KeyValuePair<int, double>(0, 1)}

                    }
                };
                Initialize(fullConfig, 32);
                return;
            }

            Initialize(FileUtils.FromJsonFile<ModelYearConfig>(@"C:\proto\noStorageAlpha0.5to1Gamma1to2.txt"), 1);
        }

        public ParameterEvaluator(ModelYearConfig config)
        {
            Initialize(config, 1);
        }

        private void Initialize(ModelYearConfig config, int length)
        {
            _mConfig = config;
            // Backup energy controller.
            _mBeCtrl = new SimulationController();
            _mBeCtrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.Cooperative,
                DistributionStrategy = DistributionStrategy.SkipFlow
            });
            _mBeCtrl.LogLevel = LogLevelEnum.System;
            _mBeCtrl.Sources.Add(new TsSourceInput {Length = length, Offset = _mConfig.Parameters["be"].Key});
            _mBeCtrl.NodeFuncs.Clear();
            _mBeCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int) input.Offset*Utils.HoursInYear);
                return Nodes;
            });
            // Backup capacity controller.
            _mBcCtrl = new SimulationController();
            _mBcCtrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.Cooperative,
                DistributionStrategy = DistributionStrategy.SkipFlow
            });
            _mBcCtrl.LogLevel = LogLevelEnum.System;
            _mBcCtrl.Sources.Add(new TsSourceInput {Length = length, Offset = _mConfig.Parameters["bc"].Key});
            _mBcCtrl.NodeFuncs.Clear();
            _mBcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int) input.Offset*Utils.HoursInYear);
                return Nodes;
            });
            // Transmission capacity controller.
            _mTcCtrl = new SimulationController();
            _mTcCtrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.ConstrainedFlow
            });
            _mTcCtrl.LogLevel = LogLevelEnum.Flow;
            _mTcCtrl.Sources.Add(new TsSourceInput {Length = length, Offset = _mConfig.Parameters["tc"].Key});
            _mTcCtrl.NodeFuncs.Clear();
            _mTcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in Nodes)
                    node.Model.SetOffset((int) input.Offset*Utils.HoursInYear);
                return Nodes;
            });

            // TODO: Make source configurable
            Nodes = ConfigurationUtils.CreateNodesNew(false);
        }

        #endregion

        #region Data evaluation

        // Sigma
        public double Sigma(NodeGenes nodeGenes)
        {
            var data = _mBeCtrl.EvaluateTs(nodeGenes);
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
            var config = _mConfig.Parameters["tc"];
            
            // Run simulation.
            var data = _mTcCtrl.EvaluateTs(nodeGenes);
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
            var config = _mConfig.Parameters["bc"];

            // Run simulation.
            var data = _mBcCtrl.EvaluateTs(nodeGenes);
            // Extract system values.
            var curtailment = (DenseTimeSeries) data[0].TimeSeries.First(item => item.Name.Equals("Curtailment"));
            var balanceNeeds = curtailment.Values.Where(item => item < 0).Select(item => -item).OrderBy(item => item);

            return MathUtils.Percentile(balanceNeeds, 99)*config.Value;
        }

        // Total backup energy.
        public double BackupEnergy(NodeGenes nodeGenes)
        {
            var config = _mConfig.Parameters["be"];

            // Run simulation.
            var data = _mBeCtrl.EvaluateTs(nodeGenes);
            // Extract system values.
            var curtailment = (DenseTimeSeries) data[0].TimeSeries.First(item => item.Name.Equals("Curtailment"));
            var balanceNeeds =
                curtailment.Values.Where(item => item < 0).Select(item => -item).OrderBy(item => item).ToList();

            return balanceNeeds.Sum()*config.Value;
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
