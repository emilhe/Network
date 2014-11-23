using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Cost;

namespace Main.Configurations
{
    class CostAnalysis
    {

        // Gamma fixed = 1
        public static void VaryAlpha(MainForm main)
        {
            var res = 20;
            var delta = 1 / ((double)res);
            var costCalc = new CostCalculator();
            var chromosome = new Chromosome(30, 0, 1.0);
            // Calculate costs and prepare data structures.
            var cost = costCalc.SystemCostWithoutLinks(chromosome);
            Dictionary<string, double[]> data = cost.ToDictionary(pair => pair.Key, pair => new double[res + 1]);
            foreach (var key in data.Keys.ToArray()) data[key][0] = cost[key];
            var alphas = new double[res + 1];
            // Main loop.
            for (int j = 1; j <= res; j++)
            {
                // Calculate costs.
                alphas[j] = j * delta;
                chromosome.Alpha = alphas[j];
                // Append costs to data structure.
                foreach (var item in costCalc.SystemCostWithoutLinks(chromosome)) data[item.Key][j] = item.Value;
            }

            // Setup view.
            var view = main.DisplayCost();
            view.AddData(data, alphas);
            view.MainChart.ChartAreas[0].AxisX.Title = "Mixing";
        }

        // Alpha fixed = 0.8
        public static void VaryGamma(MainForm main)
        {
            var res = 20;
            var delta = 1 / ((double)res);
            var costCalc = new CostCalculator();
            var chromosome = new Chromosome(30, 0.8, 0.5);
            // Calculate costs and prepare data structures.
            var cost = costCalc.SystemCostWithoutLinks(chromosome);
            Dictionary<string, double[]> data = cost.ToDictionary(pair => pair.Key, pair => new double[res + 1]);
            foreach (var key in data.Keys.ToArray()) data[key][0] = cost[key];
            var gammas = new double[res + 1];
            gammas[0] = 0.5;
            // Main loop.
            for (int j = 1; j <= res; j++)
            {
                // Calculate costs.
                gammas[j] = 0.5 + j * delta;
                chromosome.Gamma = gammas[j];
                // Append costs to data structure.
                foreach (var item in costCalc.SystemCostWithoutLinks(chromosome)) data[item.Key][j] = item.Value;
            }

            // Setup view.
            var view = main.DisplayCost();
            view.AddData(data, gammas);
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration";
        }

    }
}
