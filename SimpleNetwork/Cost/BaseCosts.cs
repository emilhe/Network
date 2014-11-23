using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.LCOE;
using BusinessLogic.Nodes;
using BusinessLogic.TimeSeries;
using NUnit.Framework;
using SimpleImporter;
using Utils;
using Utils.Statistics;

namespace BusinessLogic.Cost
{
    public class CostCalculator
    {

        private const int _modelYear = 2002;

        private const double Rate = 4;
        private static double _mAnnualizationFactor;

        private readonly NetworkModel _mSkipFlowModel;
        private readonly NetworkModel _mWithFlowModel;
        private readonly List<CountryNode> _mNodes;        
        private readonly Simulation _mSim;

        private static double AnnualizationFactor
        {
            get
            {
                if (_mAnnualizationFactor == 0) _mAnnualizationFactor = CalcAnnualizationFactor(Rate);
                return _mAnnualizationFactor;
            }
        }

        #region Public interface

        public CostCalculator()
        {
            const int offset = _modelYear - 1979;

            _mNodes = ConfigurationUtils.CreateNodes(TsSource.VE, offset);
            _mSkipFlowModel = new NetworkModel(_mNodes, new CooperativeExportStrategy(new SkipFlowStrategy()));
            _mWithFlowModel = new NetworkModel(_mNodes,
                new ConstrainedFlowExportStrategy(_mNodes, ConfigurationUtils.GetEuropeEdges(_mNodes)));
            _mSim = new Simulation(_mSkipFlowModel);
        }

        /// <summary>
        /// System cost taking links into consideration (slow to evaluate).
        /// </summary>
        public Dictionary<string, double> SystemCost(Chromosome chromosome)
        {
            // TODO: Optimize this such that ONLY necessary information is recorded.
            AdaptSystem(chromosome);

            _mSim.Model = _mWithFlowModel;
            _mSim.Simulate(Utils.Utils.HoursInYear);

            return null;
        }

        /// <summary>
        /// System coost not taking links into consideration (fast to evaluate).
        /// </summary>
        public Dictionary<string, double> SystemCostWithoutLinks(Chromosome chromosome)
        {
            AdaptSystem(chromosome);
            // Run simulation.
            _mSim.Model = _mSkipFlowModel;
            _mSim.Simulate(Utils.Utils.HoursInYear);
            // Calculate cost elements.
            var costs = new Dictionary<string, double>();
            foreach (var cost in BaseCosts()) costs.Add(cost.Key, cost.Value);
            foreach (var cost in BackupCost(_mSim.Output)) costs.Add(cost.Key, cost.Value);
            // Scale costs to get LCOE.
            var scaling = _mNodes.Select(item => item.Model.AvgLoad).Sum()*Utils.Utils.HoursInYear*AnnualizationFactor;
            foreach (var key in costs.Keys.ToArray()) costs[key] = costs[key] / scaling;

            return costs;
        }

        #endregion

        private void AdaptSystem(Chromosome genes)
        {
            // Adapt the system (scale time series data).
            for (int i = 0; i < genes.Count; i++)
            {
                _mNodes[i].Model.Alpha = genes[i].Alpha;
                _mNodes[i].Model.Gamma = genes[i].Gamma;
            }
        }

        // Cost of transmission network.
        private double TransmissionCost(SimulationOutput output)
        {
            return 0;
        }

        // Cost of backup facilities.
        private Dictionary<string, double> BackupCost(SimulationOutput output)
        {
            // Extract system values.
            var curtailment = (DenseTimeSeries) output.TimeSeries.First(item => item.Name.Equals("Curtailment"));
            var balanceNeeds = curtailment.Values.Where(item => item < 0).Select(item => -item).OrderBy(item => item).ToList();
            // Calculate the needs.
            var capacity = StatUtils.Percentile(balanceNeeds, 99);
            var energy = balanceNeeds.Sum();

            return BackupCost(capacity, energy);
        }

        #region Cost calculations

        // Cost of wind/solar facilities.
        private Dictionary<string, double> BaseCosts()
        {
            var windCapacity = 0.0;
            var solarCapacity = 0.0;

            foreach (var model in _mNodes.Select(item => item.Model))
            {
                // Calculate capacities.
                windCapacity += model.Gamma*model.Alpha * model.AvgLoad / CountryInfo.GetWindCf(model.Name);
                solarCapacity += model.Gamma * (1 - model.Alpha) * model.AvgLoad / CountryInfo.GetSolarCf(model.Name);
            }

            return new Dictionary<string, double>
            {
                {"Wind", WindCost(windCapacity)},
                {"Solar", SolarCost(solarCapacity)}
            };
        }

        private static Dictionary<string, double> BackupCost(double capacity, double energy)
        {
            return new Dictionary<string, double>
            {
                {"Backup", capacity*(Costs.CCGT.CapExFixed*1e6 + Costs.CCGT.OpExFixed*1e3*AnnualizationFactor)},
                {"Fuel", energy*Costs.CCGT.OpExVariable*AnnualizationFactor}
            };
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
}
