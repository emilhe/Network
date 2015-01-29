using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Utils;
using Controls;
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
            var costCalc = new NodeCostCalculator(new ParameterEvaluator(false));
            Dictionary<string, double[]> data = sources.ToDictionary(name => name, name => new double[res+1]);
            var alphas = new double[res + 1];

            // Main loop.
            for (int j = 0; j <= res; j++)
            {
                // Calculate costs.
                alphas[j] = j*delta;
                // Append costs to data structure.
                foreach (var item in costCalc.DetailedSystemCosts(NodeGenesFactory.SpawnBeta(1, alphas[j], beta), inclTrans))
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

        public static void CompareBeta(MainForm main, List<int> betaValues)
        {

            var alphaStart = 0.8;
            var alphaRes = 4;
            var delta = (1 - alphaStart) / alphaRes;
            var costCalc = new NodeCostCalculator(new ParameterEvaluator(true){CacheEnabled = true});
            // Calculate costs and prepare data structures.
            var data = new List<BetaWrapper>(betaValues.Count);
            var alphas = new double[alphaRes + 1];
            // Main loop.
            for (int j = 0; j < betaValues.Count; j++)
            {
                var points = new double[alphaRes + 1];
                // With trans.
                for (int i = 0; i <= alphaRes; i++)
                {
                    alphas[i] = alphaStart + (i) * delta;
                    points[i] = costCalc.SystemCost(NodeGenesFactory.SpawnBeta(alphas[i], 1, betaValues[j]), true);
                }
                data.Add(new BetaWrapper
                {
                    BetaX = alphas,
                    BetaY = points,
                    Beta = betaValues[j]
                });
                var points2 = new double[alphaRes + 1];
                // Without trans.
                for (int i = 0; i <= alphaRes; i++)
                {
                    points2[i] = costCalc.SystemCost(NodeGenesFactory.SpawnBeta(alphas[i], 1, betaValues[j]));
                }
                data.Add(new BetaWrapper
                {
                    BetaX = alphas,
                    BetaY = points2,
                    Beta = betaValues[j],
                    Note = "NoTrans"
                });
            }
            //// Add genetic points.
            //for (int i = 0; i < betas.Length; i++)
            //{
            //    var genes =
            //        FileUtils.FromJsonFile<NodeGenes>(
            //            string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\geneticWithConstraintK={0}.txt",
            //                kValues[i]));
            //    data[i].GeneticX = genes.Alpha;
            //    data[i].GeneticY = costCalc.SystemCost(genes, inclTrans);
            //}
            //// Add special genetic point.
            //var unlimitedGenes = FileUtils.FromJsonFile<NodeGenes>(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\geneticWithConstraintK=1mio.txt");
            //data.Add(new BetaWrapper { K = -1, GeneticX = unlimitedGenes.Alpha, GeneticY = costCalc.SystemCost(unlimitedGenes, inclTrans) });

            // Setup view.
            var view = main.DisplayPlot();
            view.AddData(data);
            view.MainChart.ChartAreas[0].AxisY.Minimum = 50;
            view.MainChart.ChartAreas[0].AxisY.Maximum = 100;
            view.MainChart.ChartAreas[0].AxisX.Title = "Alpha";
            ChartUtils.SaveChart(view.MainChart, 1300, 800,
                @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\VaryBeta32.png");
        }


        // Gamma fixed = 1.0
        public static void BetaWithGenetic(MainForm main, List<int> kValues, bool inclTrans = false)
        {

            var alphaStart = 0.5;
            var alphaRes = 10;
            var delta = (1-alphaStart)/alphaRes;
            var costCalc = new NodeCostCalculator(new ParameterEvaluator(true) { CacheEnabled = true });
            // Calculate costs and prepare data structures.
            var data = new List<BetaWrapper>(alphaRes + 1);
            var alphas = new double[alphaRes+1];
            var betas = new double[kValues.Count];
            // TEST
            var k = NodeGenesFactory.SpawnBeta(0.5, 1, betas[0]);
            var b = NodeGenesFactory.SpawnCfMax(0.5, 1, kValues[0]);
            // Main loop.
            for (int j = 0; j < betas.Length; j++)
            {
                var betaPoints = new double[alphaRes+1];
                var maxCfPoints = new double[alphaRes + 1];
                for (int i = 0; i <= alphaRes; i++)
                {
                    alphas[i] = alphaStart + (i)*delta;
                    betas[j] = Stuff.FindBeta(kValues[j], 1e-3);
                    betaPoints[i] = costCalc.SystemCost(NodeGenesFactory.SpawnBeta(alphas[i], 1, betas[j]), inclTrans);
                    maxCfPoints[i] = costCalc.SystemCost(NodeGenesFactory.SpawnCfMax(alphas[i], 1, kValues[j]), inclTrans);
                }
                data.Add(new BetaWrapper
                {
                    BetaX = alphas,
                    BetaY = betaPoints,
                    K = kValues[j],
                    Beta = betas[j],
                    MaxCfX = alphas,
                    MaxCfY = maxCfPoints
                });
            }
            //// Add genetic points.
            //for (int i = 0; i < betas.Length; i++)
            //{
            //    var genes =
            //        FileUtils.FromJsonFile<NodeGenes>(
            //            string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\geneticWithConstraintK={0}.txt",
            //                kValues[i]));
            //    data[i].GeneticX = genes.Alpha;
            //    data[i].GeneticY = costCalc.SystemCost(genes, inclTrans);
            //}
            // Add special genetic point.
            var genes = FileUtils.FromJsonFile<NodeGenes>(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\VEgeneticWithConstraintTransK=1.txt");
            data.Add(new BetaWrapper { K = 1, GeneticX = genes.Alpha, GeneticY = costCalc.SystemCost(genes, inclTrans), Note = "1Y + T"});
            genes = FileUtils.FromJsonFile<NodeGenes>(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\VEgeneticWithoutConstraintTransK=1.txt");
            data.Add(new BetaWrapper { K = 1, GeneticX = genes.Alpha, GeneticY = costCalc.SystemCost(genes, inclTrans), Note = "1Y" });
            genes = FileUtils.FromJsonFile<NodeGenes>(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\32VEgeneticWithoutConstraintTransK=1.txt");
            data.Add(new BetaWrapper { K = 1, GeneticX = genes.Alpha, GeneticY = costCalc.SystemCost(genes, inclTrans), Note = "32Y" });

            // Setup view.
            var view = main.DisplayPlot();
            view.AddData(data);
            view.MainChart.ChartAreas[0].AxisY.Minimum = 60;
            view.MainChart.ChartAreas[0].AxisY.Maximum = 110;
            view.MainChart.ChartAreas[0].AxisX.Minimum = 0.5;
            view.MainChart.ChartAreas[0].AxisX.Maximum = 1;

            view.MainChart.ChartAreas[0].AxisX.Title = "Alpha";
            ChartUtils.SaveChart(view.MainChart, 1000, 1000,//1300, 800,
                @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\VaryBetaWithGeneticNoTransmissionK=1.png");
        }

        // Gamma fixed = 1.0
        public static void VaryBeta(MainForm main, bool inclTrans = false, List<string> paths = null)
        {
            var betas = new[]{0,1.273,1.920,2.359,2.693};
            var alphaRes = 10;
            var delta = 1 / ((double)alphaRes);
            var costCalc = new NodeCostCalculator(new ParameterEvaluator(false));
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
                    points[i] = costCalc.SystemCost(NodeGenesFactory.SpawnBeta(alphas[i], 1, betas[j]), inclTrans);
                }
                data.Add(string.Format("Beta = {0} (K={1})",betas[j], j+1), points);
            }

            // Setup view.
            var view = main.DisplayPlot();
            foreach (var pair in data)
            {
                view.AddData(alphas, pair.Value, pair.Key, false);                
            }
            // Add a single point (found using optimization).
            if (paths != null)
            {
                var idx = 2;
                foreach (var path in paths)
                {
                    var dna = FileUtils.FromJsonFile<NodeGenes>(path);
                    view.AddData(new[] { dna.Alpha }, new[] { costCalc.SystemCost(dna, inclTrans) }, string.Format("Genetic optimum K={0}",idx), true, true);
                    idx++;
                }
            }
            view.MainChart.ChartAreas[0].AxisY.Minimum = 40;
            view.MainChart.ChartAreas[0].AxisX.Title = "Alpha";
            ChartUtils.SaveChart(view.MainChart, 1000, 800,
    @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\VaryBetaWithGenetic.png");
        }

        // Alpha fixed = 0.8
        public static void VaryGamma(MainForm main)
        {
            var res = 20;
            var delta = 1 / ((double)res);
            var sources = new List<string> { "Transmission", "Wind", "Solar", "Backup", "Fuel" };
            var costCalc = new NodeCostCalculator(new ParameterEvaluator(false));
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
            var costCalc = new NodeCostCalculator(new ParameterEvaluator(false));
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
        //    var cfW = CountryInfo.GetOnshoreWindCf();
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
