using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using BusinessLogic;
using BusinessLogic.Cost;
using SimpleImporter;
using Utils;

namespace Main.Configurations
{
    class CostAnalysis
    {

        // Gamma fixed = 1.0
        public static void VaryBeta(MainForm main)
        {
            var betas = new double[]{0,1,2,4,8,16,32};
            var alphaRes = 20;
            var delta = 1 / ((double)alphaRes);
            var costCalc = new CostCalculator();
            // Calculate costs and prepare data structures.
            var data = new Dictionary<string, double[]>();
            var alphas = new double[alphaRes];
            // Main loop.
            for (int j = 0; j < betas.Length; j++)
            {
                var points = new double[alphaRes];
                for (int i = 0; i < alphaRes; i++)
                {         
                    alphas[i] = (i+1) * delta;
                    points[i] = costCalc.SystemCostWithoutLinks(BetaScaling(1, alphas[i], betas[j]));
                }
                data.Add(string.Format("Beta = {0}",betas[j]), points);
            }

            // Setup view.
            var view = main.DisplayPlot();
            foreach (var pair in data)
            {
                view.AddData(alphas, pair.Value, pair.Key, false);                
            }
            view.MainChart.ChartAreas[0].AxisY.Minimum = 40;
            view.MainChart.ChartAreas[0].AxisX.Title = "Alpha";
        }
        
        // Gamma fixed = 1
        public static void VaryAlpha(MainForm main)
        {
            var res = 20;
            var delta = 1 / ((double)res);
            var sources = new List<string> {"Wind", "Solar", "Backup", "Fuel"};
            var countries = ProtoStore.LoadCountries();
            var costCalc = new CostCalculator();
            var chromosome = new Chromosome(countries, 0, 1.0);
            // Calculate costs and prepare data structures.
            Dictionary<string, double[]> data = sources.ToDictionary(name => name, name => new double[res + 1]);
            var alphas = new double[res + 1];
            // Main loop.
            for (int j = 0; j <= res; j++)
            {
                // Calculate costs.
                alphas[j] = j * delta;
                chromosome.Alpha = alphas[j];
                // Append costs to data structure.
                foreach (var item in costCalc.DetailedSystemCostWithoutLinks(chromosome)) data[item.Key][j] = item.Value;
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
            var sources = new List<string> { "Wind", "Solar", "Backup", "Fuel" };
            var countries = ProtoStore.LoadCountries();
            var costCalc = new CostCalculator();
            var chromosome = new Chromosome(countries, 0.8, 0.5);
            // Calculate costs and prepare data structures
            Dictionary<string, double[]> data = sources.ToDictionary(name => name, pair => new double[res + 1]);
            var gammas = new double[res + 1];
            // Main loop.
            for (int j = 0; j <= res; j++)
            {
                // Calculate costs.
                gammas[j] = 0.5 + j * delta;
                chromosome.Gamma = gammas[j];
                // Append costs to data structure.
                foreach (var item in costCalc.DetailedSystemCostWithoutLinks(chromosome)) data[item.Key][j] = item.Value;
            }

            // Setup view.
            var view = main.DisplayCost();
            view.AddData(data, gammas);
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration";
        }

        // Gamma fixed = 1
        public static void Beta16(MainForm main)
        {
            var res = 20;
            var delta = 1 / ((double)res);
            var sources = new List<string> { "Wind", "Solar", "Backup", "Fuel" };
            var costCalc = new CostCalculator();
            // Calculate costs and prepare data structures.
            Dictionary<string, double[]> data = sources.ToDictionary(name => name, name => new double[res]);
            var alphas = new double[res + 1];
            // Main loop.
            for (int j = 0; j < res; j++)
            {
                // Calculate costs.
                alphas[j] = (j+1) * delta;
                // Append costs to data structure.
                foreach (var item in costCalc.DetailedSystemCostWithoutLinks(BetaScaling(1, alphas[j], 16)))
                    data[item.Key][j] = item.Value;
            }

            // Setup view.
            var view = main.DisplayCost();
            view.AddData(data, alphas);
            view.MainChart.ChartAreas[0].AxisX.Title = "Mixing";
        }

        /// <summary>
        /// Calculate chromosome for a given beta scaling. Note that the result is NOT defined for alpha = 0.
        /// </summary>
        /// <param name="gamma"> system gamma </param>
        /// <param name="alpha"> system alpha </param>
        /// <param name="beta"> scaling parameter </param>
        /// <returns> scaled chromosome </returns>
        private static Chromosome BetaScaling(double gamma, double alpha, double beta)
        {
            var contries = ProtoStore.LoadCountries();
            var chromosome = new Chromosome(contries, alpha, gamma);
            var cfW = CountryInfo.GetWindCf();
            var cfS = CountryInfo.GetSolarCf();
            // Calculated load weighted beta-scaled cf factors.
            var wSum = 0.0;
            var sSum = 0.0;
            foreach (var i in contries)
            {
                wSum += CountryInfo.GetMeanLoad(i) * Math.Pow(cfW[i], beta);
                sSum += CountryInfo.GetMeanLoad(i) * Math.Pow(cfS[i], beta);
            }
            // Now calculate alpha i.
            foreach (var i in contries)
            {
                // EMHER: Semi certain about the gamma equaltion. 
                chromosome[i].Alpha *= 1/(alpha + (1-alpha)*Math.Pow(cfS[i]/cfW[i],beta)*wSum/sSum);
                // EMHER: Quite certain about the gamma equaltion. 
                chromosome[i].Gamma *= CountryInfo.GetMeanLoadSum() * alpha / wSum * Math.Pow(cfW[i],beta) / chromosome[i].Alpha;
            }
            return chromosome;
        }

    }
}
