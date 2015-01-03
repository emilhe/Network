using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using Optimization;
using SimpleImporter;
using Utils;
using Utils.Statistics;

namespace BusinessLogic.Cost
{
    public class NodeCostCalculator : INodeCostCalculator
    {

        public List<string> Names { get { return _mNodes.Select(item => item.Name).ToList(); } }
        
        private ModelYearConfig _mConfig;

        private const double Rate = 4;
        private static double _mAnnualizationFactor;

        private List<CountryNode> _mNodes;
        private SimulationController _mBeCtrl;
        private SimulationController _mBcCtrl;
        private SimulationController _mTcCtrl; 

        private static double AnnualizationFactor
        {
            get
            {
                if (_mAnnualizationFactor == 0) _mAnnualizationFactor = CalcAnnualizationFactor(Rate);
                return _mAnnualizationFactor;
            }
        }

        #region Public interface

        // Per default the alpha \in [0.5-1.0] and gamma \in [1:2] profile is loaded.
        public NodeCostCalculator(bool cache = true, bool full = false)
        {
            if (full)
            {
                var fullConfig = new ModelYearConfig()
                {
                    Parameters = new Dictionary<string, KeyValuePair<int, double>>
                    {
                        {"be", new KeyValuePair<int, double>(0, 1.0/32.0)},
                        {"bc", new KeyValuePair<int, double>(0, 1)},
                        {"tc", new KeyValuePair<int, double>(0, 1)}

                    }
                };
                Initialize(fullConfig, 32, cache);
                return;

            }

            Initialize(FileUtils.FromJsonFile<ModelYearConfig>(@"C:\proto\noStorageAlpha0.5to1Gamma1to2.txt"), 1, cache);
        }

        public NodeCostCalculator(ModelYearConfig config, bool cache = true)
        {
            Initialize(config, 1, cache);
        }

        private void Initialize(ModelYearConfig config, int length, bool cache = true)
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
            _mBeCtrl.Sources.Add(new TsSourceInput { Length = length, Offset = _mConfig.Parameters["be"].Key });
            _mBeCtrl.NodeFuncs.Clear();
            _mBeCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in _mNodes) node.Model.SetOffset((int)input.Offset * Utils.Utils.HoursInYear);
                return _mNodes;
            });
            _mBeCtrl.CacheEnabled = cache;
            // Backup capacity controller.
            _mBcCtrl = new SimulationController();
            _mBcCtrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.Cooperative,
                DistributionStrategy = DistributionStrategy.SkipFlow
            });
            _mBcCtrl.LogLevel = LogLevelEnum.System;
            _mBcCtrl.Sources.Add(new TsSourceInput { Length = length, Offset = _mConfig.Parameters["bc"].Key });
            _mBcCtrl.NodeFuncs.Clear();
            _mBcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in _mNodes) node.Model.SetOffset((int)input.Offset * Utils.Utils.HoursInYear);
                return _mNodes;
            });
            _mBcCtrl.CacheEnabled = cache;
            // Transmission capacity controller.
            _mTcCtrl = new SimulationController();
            _mTcCtrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.ConstrainedFlow
            });
            _mTcCtrl.LogLevel = LogLevelEnum.Flow;
            _mTcCtrl.Sources.Add(new TsSourceInput { Length = length, Offset = _mConfig.Parameters["tc"].Key });
            _mTcCtrl.NodeFuncs.Clear();
            _mTcCtrl.NodeFuncs.Add("No storage", input =>
            {
                foreach (var node in _mNodes) node.Model.SetOffset((int)input.Offset * Utils.Utils.HoursInYear);
                return _mNodes;
            });
            _mTcCtrl.CacheEnabled = cache;

            _mNodes = ConfigurationUtils.CreateNodes(TsSource.VE);
        }

        /// <summary>
        /// Detailed system cost (what goes where included).
        /// </summary>
        public Dictionary<string, double> DetailedSystemCosts(NodeGenes nodeGenes, bool includeTransmission = false)
        {
            // Calculate    cost elements.
            var costs = new Dictionary<string, double>();
            if (includeTransmission) costs.Add("Transmission", TransmissionCapacityCost(nodeGenes));
            foreach (var cost in BaseCosts(nodeGenes)) costs.Add(cost.Key, cost.Value);
            foreach (var cost in BackupCosts(nodeGenes)) costs.Add(cost.Key, cost.Value);
            // Scale costs to get LCOE.
            var scaling = _mNodes.Select(item => item.Model.AvgLoad).Sum()*Utils.Utils.HoursInYear*AnnualizationFactor;
            foreach (var key in costs.Keys.ToArray()) costs[key] = costs[key] / scaling;

            return costs;
        }

        // Does not REALLY belong here. Consider moving..
        public Dictionary<string, double> ParameterOverview(NodeGenes nodeGenes, bool includeTransmission = false)
        {
            // Calculate elements.
            var avgLoad = _mNodes.Select(item => item.Model.AvgLoad).Sum();
            var parameterOverview = new Dictionary<string, double>();
            if (includeTransmission) parameterOverview.Add("TC", TransmissionCapacity(nodeGenes)/avgLoad);
            parameterOverview.Add("BE", BackupEnergy(nodeGenes) / (avgLoad * Utils.Utils.HoursInYear));
            parameterOverview.Add("BC", BackupCapacity(nodeGenes)/avgLoad);
            parameterOverview.Add("CF", CapacityFactor(nodeGenes));

            return parameterOverview;
        }

        /// <summary>
        /// Overall system cost.
        /// </summary>
        public double SystemCost(NodeGenes nodeGenes, bool includeTransmission = false)
        {
            // Calculate cost elements.
            var cost = BaseCosts(nodeGenes).Values.Sum() + BackupCosts(nodeGenes).Values.Sum();
            if (includeTransmission) cost += TransmissionCapacityCost(nodeGenes);
            // Scale costs to get LCOE.
            var scaling = _mNodes.Select(item => item.Model.AvgLoad).Sum() * Utils.Utils.HoursInYear * AnnualizationFactor;

            return cost/scaling;
        }

        #endregion

        #region Data evaluation

        // Capacity factor
        private double CapacityFactor(NodeGenes nodeGenes)
        {
            var windCF =
                nodeGenes.Select(
                    item => item.Value.Alpha*item.Value.Gamma*CountryInfo.GetMeanLoad(item.Key)*CountryInfo.GetWindCf(item.Key)).Sum()/
                CountryInfo.GetMeanLoadSum();
            var solarCF = nodeGenes.Select(
                    item => (1-item.Value.Alpha)*item.Value.Gamma * CountryInfo.GetMeanLoad(item.Key) * CountryInfo.GetSolarCf(item.Key)).Sum() /
                CountryInfo.GetMeanLoadSum();

            return (windCF+solarCF);

        }

        // Cost of transmission network.
        private double TransmissionCapacity(NodeGenes nodeGenes)
        {
            var config = _mConfig.Parameters["tc"];

            // Run simulation.
            var data = _mTcCtrl.EvaluateTs(nodeGenes);
            // Extract system values.
            var flowTs = data[0].TimeSeries.Where(item => item.Properties.ContainsKey("Flow"));
            var overallCapacity = 0.0;
            foreach (var ts in flowTs)
            {
                var capacity = MathUtils.CalcCapacity(ts.GetAllValues());
                overallCapacity += capacity; // * Costs.GetLinkLength(ts.Properties["From"], ts.Properties["To"]);
            }

            return overallCapacity * config.Value;
        }

        // Cost of backup facilities.
        private double BackupCapacity(NodeGenes nodeGenes)
        {
            var config = _mConfig.Parameters["bc"];

            // Run simulation.
            var data = _mBcCtrl.EvaluateTs(nodeGenes);
            // Extract system values.
            var curtailment = (DenseTimeSeries)data[0].TimeSeries.First(item => item.Name.Equals("Curtailment"));
            var balanceNeeds = curtailment.Values.Where(item => item < 0).Select(item => -item).OrderBy(item => item);

            return MathUtils.Percentile(balanceNeeds, 99) * config.Value;
        }

        // Cost of backup facilities.
        private double BackupEnergy(NodeGenes nodeGenes)
        {
            var config = _mConfig.Parameters["be"];

            // Run simulation.
            var data = _mBeCtrl.EvaluateTs(nodeGenes);
            // Extract system values.
            var curtailment = (DenseTimeSeries)data[0].TimeSeries.First(item => item.Name.Equals("Curtailment"));
            var balanceNeeds = curtailment.Values.Where(item => item < 0).Select(item => -item).OrderBy(item => item).ToList();

            return balanceNeeds.Sum() * config.Value;
        }

        #endregion

        #region Cost calculations

        // Cost of transmission network.
        private double TransmissionCapacityCost(NodeGenes nodeGenes)
        {
            var config = _mConfig.Parameters["tc"];

            // Run simulation.
            var data = _mTcCtrl.EvaluateTs(nodeGenes);
            // Extract system values.
            var flowTs = data[0].TimeSeries.Where(item => item.Properties.ContainsKey("Flow"));
            var cost = 0.0;
            foreach (var ts in flowTs)
            {
                var capacity = MathUtils.CalcCapacity(ts.GetAllValues());
                cost += capacity * Costs.GetLinkCost(ts.Properties["From"], ts.Properties["To"], true);
            }

            return cost * config.Value;
        }

        // Cost of wind/solar facilities.
        private Dictionary<string, double> BaseCosts(NodeGenes nodeGenes)
        {
            var windCapacity = 0.0;
            var solarCapacity = 0.0;

            foreach (var node in _mNodes.Select(item => item.Model))
            {
                var gene = nodeGenes[node.Name];
                // Calculate capacities.
                windCapacity += gene.Gamma * gene.Alpha * node.AvgLoad / CountryInfo.GetWindCf(node.Name);
                solarCapacity += gene.Gamma * (1 - gene.Alpha) * node.AvgLoad / CountryInfo.GetSolarCf(node.Name);
            }
                
            return new Dictionary<string, double>
            {
                {"Wind", WindCost(windCapacity)},
                {"Solar", SolarCost(solarCapacity)}
            };
        }

        // Cost of backup facilities and fuel.
        private Dictionary<string, double> BackupCosts(NodeGenes nodeGenes)
        {
            return new Dictionary<string, double>
            {
                {"Backup", BackupCapacityCost(BackupCapacity(nodeGenes))},
                {"Fuel", BackupEnergyCost(BackupEnergy(nodeGenes))}
            };
        }

        private static double BackupEnergyCost(double energy)
        {
            return energy*Costs.CCGT.OpExVariable*AnnualizationFactor;
        }

        private static double BackupCapacityCost(double capacity)
        {
            return capacity*(Costs.CCGT.CapExFixed*1e6 + Costs.CCGT.OpExFixed*1e3*AnnualizationFactor);
        }

        private static double WindCost(double capacity)
        {
            return capacity*(Costs.Wind.CapExFixed*1e6 + Costs.Wind.OpExFixed*1e3*AnnualizationFactor);
        }

        private static double SolarCost(double capacity)
        {
            return capacity*(Costs.Solar.CapExFixed*1e6 + Costs.Solar.OpExFixed*1e3*AnnualizationFactor);
        }

        #endregion

        // WTF is this?
        private static double CalcAnnualizationFactor(double rate)
        {
            if (rate == 0) return 30;
            return (1 - Math.Pow((1 + (rate/100.0)), -30))/(rate/100.0);
        }
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
