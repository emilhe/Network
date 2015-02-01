using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Cost.CostModels;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.Utils;
using Utils;

namespace BusinessLogic.Cost
{
    public class NodeCostCalculator : INodeCostCalculator
    {

        public bool CacheEnabled { get { return Evaluator.CacheEnabled; } set { Evaluator.CacheEnabled = value; } }
        public readonly ParameterEvaluator Evaluator;

        // Default cost models.
        public ITransmissionCostModel TcCostModel = new VariableLengthModel();
        public IBackupCostModel BcCostModel = new BackupCostModelImpl();        
        public ISolarCostModel SolarCostModel = new SolarCostModelImpl();
        public IWindCostModel WindCostModel = new WindCostModelImpl();

        public const double Lifetime = 30;

        #region Public interface

        public NodeCostCalculator(ParameterEvaluator evaluator)
        {
            Evaluator = evaluator;
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
            var scaling = Evaluator.Nodes.Select(item => item.Model.AvgLoad).Sum()*Utils.Stuff.HoursInYear*Costs.AnnualizationFactor(Lifetime); //TODO: Why annualization factor here???
            foreach (var key in costs.Keys.ToArray()) costs[key] = costs[key] / scaling;

            return costs;
        }

        // Does not REALLY belong here. Consider moving..
        public Dictionary<string, double> ParameterOverview(NodeGenes nodeGenes, bool includeTransmission = false)
        {
            // Calculate elements.
            var avgLoad = Evaluator.Nodes.Select(item => item.Model.AvgLoad).Sum();
            var parameterOverview = new Dictionary<string, double>();
            var costs = BaseCosts(nodeGenes).Values.Sum();

            if (includeTransmission)
            {
                var tc = TransmissionCapacityCost(nodeGenes);
                parameterOverview.Add("TC", tc / avgLoad);
                costs += tc;
            }
            var be = Evaluator.BackupEnergy(nodeGenes);
            costs += BcCostModel.BackupEnergyCost(be);
            var bc = Evaluator.BackupCapacity(nodeGenes);
            costs += BcCostModel.BackupCapacityCost(bc);
            parameterOverview.Add("BE", be / (avgLoad * Utils.Stuff.HoursInYear));
            parameterOverview.Add("BC", bc / avgLoad);
            parameterOverview.Add("CF", Evaluator.CapacityFactor(nodeGenes));
            var scaling = Evaluator.Nodes.Select(item => item.Model.AvgLoad).Sum() * Utils.Stuff.HoursInYear * Costs.AnnualizationFactor(Lifetime);
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
            var scaling = Evaluator.Nodes.Select(item => item.Model.AvgLoad).Sum() * Stuff.HoursInYear * Costs.AnnualizationFactor(Lifetime);

            return cost/scaling;
        }

        #endregion

        #region Cost calculations

        // Cost of transmission network.
        private double TransmissionCapacityCost(NodeGenes nodeGenes)
        {
            return TcCostModel.Eval(Evaluator.LinkCapacities(nodeGenes));
        }

        // Cost of wind/solar facilities.
        private Dictionary<string, double> BaseCosts(NodeGenes nodeGenes)
        {
            var solarCapacity = 0.0;
            var onshoreWindCapacity = 0.0;
            var offshoreWindCapacity = 0.0;

            foreach (var node in Evaluator.Nodes.Select(item => item.Model))
            {
                var gene = nodeGenes[node.Name];
                // Calculate capacities.
                solarCapacity += gene.Gamma * (1 - gene.Alpha) * node.AvgLoad / CountryInfo.GetSolarCf(node.Name);
                onshoreWindCapacity += gene.Gamma * gene.Alpha * (1-gene.OffshoreFraction) * node.AvgLoad / CountryInfo.GetOnshoreWindCf(node.Name);
                if (CountryInfo.GetOffshoreWindCf(node.Name) == 0) continue;
                offshoreWindCapacity += gene.Gamma * gene.Alpha * gene.OffshoreFraction * node.AvgLoad / CountryInfo.GetOffshoreWindCf(node.Name);
            }

            var onshoreCost = WindCostModel.OnshoreWindCost(onshoreWindCapacity);
            var offshoreCost = WindCostModel.OffshoreWindCost(offshoreWindCapacity);

            return new Dictionary<string, double>
            {
                //{"Wind",onshoreCost + offshoreCost},
                {"Solar", SolarCostModel.SolarCost(solarCapacity)},
                {"Onshore wind", onshoreCost},
                {"Offshore wind", offshoreCost},
            };
        }

        // Cost of backup facilities and fuel.
        private Dictionary<string, double> BackupCosts(NodeGenes nodeGenes)
        {
            return new Dictionary<string, double>
            {
                {"Backup", BcCostModel.BackupCapacityCost(Evaluator.BackupCapacity(nodeGenes))},
                {"Fuel", BcCostModel.BackupEnergyCost(Evaluator.BackupEnergy(nodeGenes))}
            };
        }

        #endregion

    }

}
