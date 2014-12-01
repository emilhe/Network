using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using NUnit.Framework;
using SimpleImporter;
using Utils;
using Utils.Statistics;

namespace BusinessLogic.Cost
{
    public class CostCalculator
    {

        private const int ModelYear = 2002;

        private const double Rate = 4;
        private static double _mAnnualizationFactor;

        private readonly List<CountryNode> _mNodes;
        private readonly SimulationCore _mSimSkipFlow; 
        private readonly SimulationCore _mSimWithFlow;

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
            const int offset = ModelYear - 1979;

            // TODO: Check if this works?
            _mNodes = ConfigurationUtils.CreateNodes(TsSource.VE, offset);
            _mSimSkipFlow =
                new SimulationCore(new NetworkModel(_mNodes, new CooperativeExportStrategy(new SkipFlowStrategy())));
            _mSimWithFlow =
                new SimulationCore(new NetworkModel(_mNodes,
                    new ConstrainedFlowExportStrategy(_mNodes, ConfigurationUtils.GetEuropeEdges(_mNodes))));
        }

        /// <summary>
        /// Detailed system cost (what goes where included).
        /// </summary>
        public Dictionary<string, double> DetailedSystemCosts(Chromosome chromosome, bool includeTransmission = false)
        {
            AdaptSystem(chromosome);
            // Run simulation.
            var sim = includeTransmission ? _mSimWithFlow : _mSimSkipFlow;
            sim.Simulate(Utils.Utils.HoursInYear, includeTransmission ? LogLevelEnum.Flow : LogLevelEnum.System);
            // Calculate cost elements.
            var costs = new Dictionary<string, double>();
            if (includeTransmission) costs.Add("Transmission", TransmissionCost(sim.Output));
            foreach (var cost in BaseCosts()) costs.Add(cost.Key, cost.Value);
            foreach (var cost in BackupCost(sim.Output)) costs.Add(cost.Key, cost.Value);
            // Scale costs to get LCOE.
            var scaling = _mNodes.Select(item => item.Model.AvgLoad).Sum()*Utils.Utils.HoursInYear*AnnualizationFactor;
            foreach (var key in costs.Keys.ToArray()) costs[key] = costs[key] / scaling;

            return costs;
        }

        /// <summary>
        /// Overall system cost.
        /// </summary>
        public double SystemCost(Chromosome chromosome, bool includeTransmission = false)
        {
            AdaptSystem(chromosome);
            // Run simulation.
            var sim = includeTransmission ? _mSimWithFlow : _mSimSkipFlow;
            sim.Simulate(Utils.Utils.HoursInYear, includeTransmission ? LogLevelEnum.Flow : LogLevelEnum.System);
            // Calculate cost elements.
            var cost = BaseCosts().Values.Sum() + BackupCost(sim.Output).Values.Sum() + TransmissionCost(sim.Output);
            // Scale costs to get LCOE.
            var scaling = _mNodes.Select(item => item.Model.AvgLoad).Sum() * Utils.Utils.HoursInYear * AnnualizationFactor;

            return cost/scaling;
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
            var flowTs = output.TimeSeries.Where(item => item.Properties.ContainsKey("Flow"));
            var cost = 0.0;

            foreach (var ts in flowTs)
            {
                var capacity = StatUtils.CalcCapacity(ts.GetAllValues());
                cost += capacity * Costs.GetLinkCost(ts.Properties["From"], ts.Properties["To"], true);
            }

            return cost;
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
