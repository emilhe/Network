using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using BusinessLogic;
using BusinessLogic.Cost;
using Controls.Charting;
using SimpleImporter;
using Utils;

namespace Main.Configurations
{
    class CostAnalysis
    {

        #region Detailed beta analysis

        // Gamma fixed = 1
        public static void Beta0(MainForm main, bool inclTrans = false)
        {
            var view = BetaCalc(main, 10, 0, inclTrans);
            ChartUtils.SaveChart(view.MainChart, 1000, 800,
                @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\Test.png");
                //@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\TransmissionBeta0ACDC.png");
        }

        // Gamma fixed = 1
        public static void Beta16(MainForm main, bool inclTrans = false)
        {
            var view = BetaCalc(main, 20, 16, inclTrans);
            ChartUtils.SaveChart(view.MainChart, 1000, 800,
                @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\Test2.png");
        }

        private static CostView BetaCalc(MainForm main, int res, double beta, bool inclTrans)
        {
            var delta = 1 / ((double)res);
            var sources = new List<string> { "Transmission", "Wind", "Solar", "Backup", "Fuel" };
            var costCalc = new NodeCostCalculator();
            Dictionary<string, double[]> data = sources.ToDictionary(name => name, name => new double[res+1]);
            var alphas = new double[res + 1];

            // Main loop.
            for (int j = 0; j <= res; j++)
            {
                // Calculate costs.
                alphas[j] = j*delta;
                // Append costs to data structure.
                foreach (var item in costCalc.DetailedSystemCosts(new NodeGenes(1, alphas[j], beta), inclTrans))
                {
                    data[item.Key][j] = item.Value;
                }
            }

            // Setup view.
            var view = main.DisplayCost();
            view.AddData(data, alphas);
            view.MainChart.ChartAreas[0].AxisX.Title = "Mixing";
            return view;
        }

        #endregion

        // Gamma fixed = 1.0
        public static void VaryBeta(MainForm main, bool inclTrans = false, string path = null)
        {
            var betas = new double[]{0,1,2,4,8,16,32};
            var alphaRes = 20;
            var delta = 1 / ((double)alphaRes);
            var costCalc = new NodeCostCalculator();
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
                    points[i] = costCalc.SystemCost(new NodeGenes(alphas[i], 1, betas[j]), inclTrans);
                }
                data.Add(string.Format("Beta = {0}",betas[j]), points);
            }

            // Setup view.
            var view = main.DisplayPlot();
            foreach (var pair in data)
            {
                view.AddData(alphas, pair.Value, pair.Key, false);                
            }
            // Add a single point (found using optimization).
            if (path != null)
            {
                var dna = FileUtils.FromJsonFile<NodeGenes>(path);
                view.AddData(new[] { dna.Alpha }, new[] { costCalc.SystemCost(dna, inclTrans) }, "Genetic optimum", true, true);
            }
            view.MainChart.ChartAreas[0].AxisY.Minimum = 40;
            view.MainChart.ChartAreas[0].AxisX.Title = "Alpha";
            ChartUtils.SaveChart(view.MainChart, 1000, 800,
    @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\VaryBetaWithTrans.png");
        }

        // Alpha fixed = 0.8
        public static void VaryGamma(MainForm main)
        {
            var res = 20;
            var delta = 1 / ((double)res);
            var sources = new List<string> { "Transmission", "Wind", "Solar", "Backup", "Fuel" };
            var costCalc = new NodeCostCalculator();
            var chromosome = new NodeGenes(0.8, 0.5);

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
                foreach (var item in costCalc.DetailedSystemCosts(chromosome, true)) data[item.Key][j] = item.Value;
            }

            // Setup view.
            var view = main.DisplayCost();
            view.AddData(data, gammas);
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration";
        }

        // Alpha fixed = 0.8
        public static void PlotShit(MainForm main, NodeGenes genes)
        {
            var res = 2;
            var delta = 1 / ((double)res);
            var sources = new List<string> { "Transmission", "Wind", "Solar", "Backup", "Fuel" };
            var countries = ProtoStore.LoadCountries();
            var costCalc = new NodeCostCalculator();
            //var chromosome = new NodeGenes(countries, 0.8, 0.5);
            // Calculate costs and prepare data structures
            Dictionary<string, double[]> data = sources.ToDictionary(name => name, pair => new double[res + 1]);
            var gammas = new double[res + 1];
            // Main loop.
            for (int j = 0; j <= res; j++)
            {
                // Calculate costs.
                gammas[j] = 0.5 + j * delta;
                //chromosome.Gamma = gammas[j];
                // Append costs to data structure.
                foreach (var item in costCalc.DetailedSystemCosts(genes, true)) data[item.Key][j] = item.Value;
            }

            // Setup view.
            var view = main.DisplayCost();
            view.AddData(data, gammas);
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration";
        }

        ///// <summary>
        ///// Calculate chromosome for a given beta scaling.
        ///// </summary>
        ///// <param name="gamma"> system gamma </param>
        ///// <param name="alpha"> system alpha </param>
        ///// <param name="beta"> scaling parameter </param>
        ///// <returns> scaled chromosome </returns>
        //private static NodeGenes BetaScaling(double gamma, double alpha, double beta)
        //{
        //    // The result is NOT defined in alpha = 0.
        //    if (Math.Abs(alpha) < 1e-5) alpha = 1e-5;

        //    var contries = ProtoStore.LoadCountries();
        //    var chromosome = new NodeGenes(alpha, gamma);
        //    var cfW = CountryInfo.GetWindCf();
        //    var cfS = CountryInfo.GetSolarCf();
        //    // Calculated load weighted beta-scaled cf factors.
        //    var wSum = 0.0;
        //    var sSum = 0.0;
        //    foreach (var i in contries)
        //    {
        //        wSum += CountryInfo.GetMeanLoad(i) * Math.Pow(cfW[i], beta);
        //        sSum += CountryInfo.GetMeanLoad(i) * Math.Pow(cfS[i], beta);
        //    }
        //    // Now calculate alpha i.
        //    foreach (var i in contries)
        //    {
        //        // EMHER: Semi certain about the gamma equaltion. 
        //        chromosome[i].Alpha *= 1/(alpha + (1-alpha)*Math.Pow(cfS[i]/cfW[i],beta)*wSum/sSum);
        //        // EMHER: Quite certain about the gamma equaltion. 
        //        chromosome[i].Gamma *= CountryInfo.GetMeanLoadSum() * alpha / wSum * Math.Pow(cfW[i],beta) / chromosome[i].Alpha;
        //    }
        //    // Make sanity check.
        //    var dAlpha = alpha - chromosome.Alpha;
        //    var gGamma = gamma - chromosome.Gamma;
        //    if (Math.Abs(dAlpha) > 1e-6) throw new ArgumentException("Alpha value wrong");
        //    if (Math.Abs(gGamma) > 1e-6) throw new ArgumentException("Gamma value wrong");

        //    return chromosome;
        //}

    }
}
