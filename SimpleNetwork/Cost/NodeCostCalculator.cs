using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Utils;
using Utils;

namespace BusinessLogic.Cost
{
    public class NodeCostCalculator : INodeCostCalculator
    {

        public bool CacheEnabled { get { return _mEvaluator.CacheEnabled; } set { _mEvaluator.CacheEnabled = value; } }
        private readonly ParameterEvaluator _mEvaluator;

        private const double Rate = 4;
        private const double LifeTime = 30;

        private static double _mAnnualizationFactor;
        private static double AnnualizationFactor
        {
            get
            {
                if (_mAnnualizationFactor == 0) _mAnnualizationFactor = CalcAnnualizationFactor(Rate);
                return _mAnnualizationFactor;
            }
        }

        #region Public interface

        public NodeCostCalculator(ParameterEvaluator evaluator)
        {
            _mEvaluator = evaluator;
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
            var scaling = _mEvaluator.Nodes.Select(item => item.Model.AvgLoad).Sum()*Utils.Utils.HoursInYear*AnnualizationFactor;
            foreach (var key in costs.Keys.ToArray()) costs[key] = costs[key] / scaling;

            return costs;
        }

        // Does not REALLY belong here. Consider moving..
        public Dictionary<string, double> ParameterOverview(NodeGenes nodeGenes, bool includeTransmission = false)
        {
            // Calculate elements.
            var avgLoad = _mEvaluator.Nodes.Select(item => item.Model.AvgLoad).Sum();
            var parameterOverview = new Dictionary<string, double>();
            var costs = BaseCosts(nodeGenes).Values.Sum();

            if (includeTransmission)
            {
                var tc = TransmissionCapacityCost(nodeGenes);
                parameterOverview.Add("TC", tc / avgLoad);
                costs += tc;
            }
            var be = _mEvaluator.BackupEnergy(nodeGenes);
            costs += BackupEnergyCost(be);
            var bc = _mEvaluator.BackupCapacity(nodeGenes);
            costs += BackupCapacityCost(bc);
            parameterOverview.Add("BE", be / (avgLoad * Utils.Utils.HoursInYear));
            parameterOverview.Add("BC", bc / avgLoad);
            parameterOverview.Add("CF", _mEvaluator.CapacityFactor(nodeGenes));
            var scaling = _mEvaluator.Nodes.Select(item => item.Model.AvgLoad).Sum() * Utils.Utils.HoursInYear * AnnualizationFactor;
            parameterOverview.Add("LCOE", costs/scaling);

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
            var scaling = _mEvaluator.Nodes.Select(item => item.Model.AvgLoad).Sum() * Utils.Utils.HoursInYear * AnnualizationFactor;

            return cost/scaling;
        }

        #endregion

        #region Cost calculations

        // Cost of transmission network.
        private double TransmissionCapacityCost(NodeGenes nodeGenes)
        {
            return _mEvaluator.LinkCapacities(nodeGenes).Sum(link => link.Value*Costs.GetLinkCost(link.Key));
        }

        // Cost of wind/solar facilities.
        private Dictionary<string, double> BaseCosts(NodeGenes nodeGenes)
        {
            var windCapacity = 0.0;
            var solarCapacity = 0.0;

            foreach (var node in _mEvaluator.Nodes.Select(item => item.Model))
            {
                var gene = nodeGenes[node.Name];
                // Calculate capacities.
                windCapacity += gene.Gamma * gene.Alpha * node.AvgLoad / CountryInfo.GetOnshoreWindCf(node.Name);
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
                {"Backup", BackupCapacityCost(_mEvaluator.BackupCapacity(nodeGenes))},
                {"Fuel", BackupEnergyCost(_mEvaluator.BackupEnergy(nodeGenes))}
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
            return capacity*(Costs.OnshoreWind.CapExFixed*1e6 + Costs.OnshoreWind.OpExFixed*1e3*AnnualizationFactor);
        }

        private static double SolarCost(double capacity)
        {
            return capacity*(Costs.Solar.CapExFixed*1e6 + Costs.Solar.OpExFixed*1e3*AnnualizationFactor);
        }

        #endregion

        // WTF is this?
        private static double CalcAnnualizationFactor(double rate)
        {
            if (rate == 0) return LifeTime;
            return (1 - Math.Pow((1 + (rate/100.0)), -LifeTime))/(rate/100.0);
        }
    }

}
