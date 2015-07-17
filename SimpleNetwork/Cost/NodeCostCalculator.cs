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

        #region Public interface

        public NodeCostCalculator(ParameterEvaluator evaluator)
        {
            Evaluator = evaluator;
        }

        /// <summary>
        /// Detailed system cost (what goes where included).
        /// </summary>
        public Dictionary<string, double> DetailedSystemCosts(NodeGenes nodeGenes)
        {
            Evaluator.MemoryCacheEnabled = true;
            // Calculate cost elements.
            var costs = new Dictionary<string, double>();
            costs.Add("Transmission", TcCostModel.Eval(Evaluator.LinkCapacities(nodeGenes))/Costs.AnnualizationFactor(Costs.LinkLifeTime));
            foreach (var cost in BaseCosts(nodeGenes)) costs.Add(cost.Key, cost.Value);
            foreach (var cost in BackupCosts(nodeGenes)) costs.Add(cost.Key, cost.Value);
            Evaluator.MemoryCacheEnabled = false;
            Evaluator.FlushMemoryCache();
            // Scale costs to get LCOE.
            var scaling = Evaluator.Nodes.Select(item => item.Model.AvgLoad).Sum()*Stuff.HoursInYear;
            foreach (var key in costs.Keys.ToArray()) costs[key] = costs[key] / scaling;

            return costs;
        }

        // Does not REALLY belong here. Consider moving..
        public Dictionary<string, double> ParameterOverview(NodeGenes nodeGenes)
        {
            Evaluator.MemoryCacheEnabled = true;
            // Calculate elements.
            var avgLoad = Evaluator.Nodes.Select(item => item.Model.AvgLoad).Sum();
            var scaling = avgLoad*Stuff.HoursInYear;
            var parameterOverview = new Dictionary<string, double>();
            var costs = BaseCosts(nodeGenes).Values.Sum()/scaling;

            var tc = Evaluator.LinkCapacities(nodeGenes);
            parameterOverview.Add("TC", tc.Select(item => Costs.LinkLength[item.Key]*item.Value).Sum()/avgLoad);
            costs += TcCostModel.Eval(tc)/Costs.AnnualizationFactor(Costs.LinkLifeTime)/scaling;
            var be = Evaluator.BackupEnergy(nodeGenes);
            costs += BcCostModel.BackupEnergyCost(be)/Costs.AnnualizationFactor(Costs.CCGT.Lifetime)/scaling;
            var bc = Evaluator.BackupCapacity(nodeGenes);
            costs += BcCostModel.BackupCapacityCost(bc)/Costs.AnnualizationFactor(Costs.CCGT.Lifetime)/scaling;
            parameterOverview.Add("BE", be/(avgLoad*Stuff.HoursInYear));
            parameterOverview.Add("BC", bc/avgLoad);
            parameterOverview.Add("CF", Evaluator.CapacityFactor(nodeGenes));
            Evaluator.MemoryCacheEnabled = false;
            Evaluator.FlushMemoryCache();
            // Scale costs to get LCOE.
            parameterOverview.Add("LCOE", costs);

            return parameterOverview;
        }

        /// <summary>
        /// Overall system cost.
        /// </summary>
        public double SystemCost(NodeGenes nodeGenes)
        {
            Evaluator.MemoryCacheEnabled = true;
            // Calculate cost elements.
            var cost = BaseCosts(nodeGenes).Values.Sum() + BackupCosts(nodeGenes).Values.Sum();
            cost += TcCostModel.Eval(Evaluator.LinkCapacities(nodeGenes)) / Costs.AnnualizationFactor(Costs.LinkLifeTime);
            Evaluator.MemoryCacheEnabled = false;
            Evaluator.FlushMemoryCache();
            // Scale costs to get LCOE.
            var scaling = Evaluator.Nodes.Select(item => item.Model.AvgLoad).Sum() * Stuff.HoursInYear;

            return cost/scaling;
        }

        #endregion

        #region Cost calculations

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
                {"Solar", SolarCostModel.SolarCost(solarCapacity)/Costs.AnnualizationFactor(Costs.Solar.Lifetime)},
                {"Onshore wind", onshoreCost/Costs.AnnualizationFactor(Costs.OnshoreWind.Lifetime)},
                {"Offshore wind", offshoreCost/Costs.AnnualizationFactor(Costs.OffshoreWind.Lifetime)},
            };
        }

        // Cost of backup facilities and fuel.
        private Dictionary<string, double> BackupCosts(NodeGenes nodeGenes)
        {
            return new Dictionary<string, double>
            {
                {"Backup", BcCostModel.BackupCapacityCost(Evaluator.BackupCapacity(nodeGenes))/Costs.AnnualizationFactor(Costs.CCGT.Lifetime)},
                {"Fuel", BcCostModel.BackupEnergyCost(Evaluator.BackupEnergy(nodeGenes))/Costs.AnnualizationFactor(Costs.CCGT.Lifetime)}
            };
        }

        #endregion

    }

}
