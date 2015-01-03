using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BusinessLogic.Cost;
using BusinessLogic.Utils;
using Controls;
using Controls.Article;
using Controls.Charting;
using Utils;

namespace Main.Figures
{
    class PlayGround
    {

        private const string GeneticPath =
            @"C:\Users\Emil\Dropbox\Master Thesis\Layouts\onshoreVEgeneticConstraintTransK={0}.txt";

        #region Data export to JSON for external rendering

        public static void ExportChromosomeData()
        {
            var mix = 1;
            var basePath = @"C:\Users\Emil\Dropbox\Master Thesis\Python\chromosomes\";
            var layouts = new Dictionary<NodeGenes, string>();

            // Homogeneous layout.
            //layouts.Add(new NodeGenes(1,1), "homo.txt");
            // Standard beta/max CF layouts
            layouts.Add(NodeGenesFactory.SpawnBeta(1, 1, 1), "beta=1wind.txt");
            layouts.Add(NodeGenesFactory.SpawnBeta(0, 1, 1), "beta=1solar.txt");
            layouts.Add(NodeGenesFactory.SpawnCfMax(1, 1, 2), "k=2cfMaxWind.txt");
            layouts.Add(NodeGenesFactory.SpawnCfMax(0, 1, 2), "k=2cfMaxSolar.txt");
            // Beta layouts.
            layouts.Add(NodeGenesFactory.SpawnBeta(mix, 1, BusinessLogic.Utils.Utils.FindBeta(1, 1e-3, mix)), "k=1beta.txt");
            layouts.Add(NodeGenesFactory.SpawnBeta(mix, 1, BusinessLogic.Utils.Utils.FindBeta(2, 1e-3, mix)), "k=2beta.txt");
            layouts.Add(NodeGenesFactory.SpawnBeta(mix, 1, BusinessLogic.Utils.Utils.FindBeta(3, 1e-3, mix)), "k=3beta.txt");
            // Maximum CF layouts.
            layouts.Add(NodeGenesFactory.SpawnCfMax(mix, 1, 1), "k=1cfMax.txt");
            layouts.Add(NodeGenesFactory.SpawnCfMax(mix, 1, 2), "k=2cfMax.txt");
            layouts.Add(NodeGenesFactory.SpawnCfMax(mix, 1, 3), "k=3cfMax.txt");
            // Genetic layouts.
            layouts.Add(FileUtils.FromJsonFile<NodeGenes>(string.Format(GeneticPath, 1)), "k=1genetic.txt.");
            layouts.Add(FileUtils.FromJsonFile<NodeGenes>(string.Format(GeneticPath, 2)), "k=2genetic.txt");
            layouts.Add(FileUtils.FromJsonFile<NodeGenes>(string.Format(GeneticPath, 3)), "k=3genetic.txt");

            // Save the data as JSON.
            foreach (var pair in layouts) pair.Key.Export().ToJsonFile(basePath + pair.Value);  
        }

        public static void ExportParameterOverviewData(List<double> kValues, bool inclTrans = false)
        {
            var costCalc = new NodeCostCalculator(new ParameterEvaluator(true));
            var data = CalcBetaCurves(kValues, 0.0, genes => genes.Alpha, genes => costCalc.ParameterOverview(genes, inclTrans));

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\dataS2.txt");
        }

        public static void ExportCostDetailsData(List<double> kValues, bool inclTrans = false)
        {
            var costCalc = new NodeCostCalculator(new ParameterEvaluator(true));
            var data = CalcCostDetails(kValues, 1, genes => costCalc.DetailedSystemCosts(genes, inclTrans));

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\costs\cost.txt");
        }

        public static void ExportMismatchData(List<double> kValues, bool inclTrans = false)
        {
            var paramEval = new ParameterEvaluator();
            var costCalc = new NodeCostCalculator(paramEval);
            var data = CalcBetaCurves(kValues, 0.0, paramEval.Sigma, genes => new Dictionary<string, double>
            {
                {"CF", paramEval.CapacityFactor(genes)},
                {"LCOE", costCalc.SystemCost(genes, inclTrans)}
            });

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\sigmas\sigma.txt");
        }

        #endregion

        private static Dictionary<string, Dictionary<double, BetaWrapper>> CalcBetaCurves(List<double> kValues, double alphaStart, Func<NodeGenes, double> evalX, Func<NodeGenes, Dictionary<string, double>> evalY)
        {
            // Prepare the data structures.
            var alphaRes = 10;
            var delta = (1 - alphaStart) / alphaRes;
            var alphas = new double[alphaRes + 1];
            var betas = new double[kValues.Count];
            var data = new Dictionary<string, Dictionary<double, BetaWrapper>>();
            for (int j = 0; j < betas.Length; j++)
            {
                betas[j] = BusinessLogic.Utils.Utils.FindBeta(kValues[j], 1e-3);
                for (int i = 0; i <= alphaRes; i++)
                {
                    alphas[i] = alphaStart + (i) * delta;
                }
            }

            // Calculate beta curves.
            for (int j = 0; j < betas.Length; j++)
            {
                for (int i = 0; i <= alphaRes; i++)
                {
                    var genes = NodeGenesFactory.SpawnBeta(alphas[i], 1, betas[j]);
                    var xValue = evalX(genes);
                    var results = evalY(genes);
                    foreach (var pair in results)
                    {
                        if (!data.ContainsKey(pair.Key)) data.Add(pair.Key, new Dictionary<double, BetaWrapper>());
                        if (!data[pair.Key].ContainsKey(kValues[j])) data[pair.Key].Add(kValues[j], new BetaWrapper()
                        {
                            K = kValues[j],
                            Beta = betas[j],
                            BetaX = new double[alphaRes + 1],
                            BetaY = new double[alphaRes + 1],
                            MaxCfX = new double[alphaRes + 1],    
                            MaxCfY = new double[alphaRes + 1],
                        });
                        data[pair.Key][kValues[j]].BetaY[i] = pair.Value;
                        data[pair.Key][kValues[j]].BetaX[i] = xValue;
                    }
                    results = evalY(NodeGenesFactory.SpawnCfMax(alphas[i], 1, kValues[j]));
                    foreach (var pair in results)
                    {
                        data[pair.Key][kValues[j]].MaxCfY[i] = pair.Value;
                        data[pair.Key][kValues[j]].MaxCfX[i] = xValue;
                    }
                }
            }

            // Calculate genetic points.
            for (int j = 0; j < kValues.Count; j++)
            {
                var genes =
                    FileUtils.FromJsonFile<NodeGenes>(
                        string.Format(GeneticPath,
                            kValues[j]));
                var xValue = evalX(genes);
                var results = evalY(genes);
                foreach (var pair in results)
                {
                    data[pair.Key][kValues[j]].GeneticX = xValue;
                    data[pair.Key][kValues[j]].GeneticY = pair.Value;
                }
            }

            return data;
        }

        private static CostWrapper CalcCostDetails(List<double> kValues, double alpha, Func<NodeGenes, Dictionary<string, double>> eval)
        {
            // Prepare the data structures.
            var betas = new double[kValues.Count];
            var data = new CostWrapper
            {
                Costs = new Dictionary<string, List<double>>(),
                Labels = new List<string>()
            };
            for (int j = 0; j < betas.Length; j++)
            {
                betas[j] = BusinessLogic.Utils.Utils.FindBeta(kValues[j], 1e-3);
            }

            for (int j = 0; j < betas.Length; j++)
            {
                // Add BETA point.
                data.Labels.Add(string.Format(@"$\beta$={0}", betas[j].ToString("0.0")));
                var results = eval(NodeGenesFactory.SpawnBeta(alpha, 1, betas[j]));
                foreach (var pair in results)
                {
                    if (!data.Costs.ContainsKey(pair.Key)) data.Costs.Add(pair.Key, new List<double>());
                    data.Costs[pair.Key].Add(pair.Value);
                }
                // Add MaxCF point.
                data.Labels.Add(string.Format(@"K={0}", kValues[j].ToString("0")));
                results = eval(NodeGenesFactory.SpawnCfMax(alpha, 1, kValues[j]));
                foreach (var pair in results)
                {
                    data.Costs[pair.Key].Add(pair.Value);
                }
                // Add genetic point.
                data.Labels.Add(string.Format(@"GA@K={0}", kValues[j].ToString("0")));
                var genes = FileUtils.FromJsonFile<NodeGenes>(string.Format(GeneticPath,kValues[j]));
                results = eval(genes);
                foreach (var pair in results)
                {
                    data.Costs[pair.Key].Add(pair.Value);
                }
            }

            return data;
        }

        #region Deprecated

        //private static Dictionary<string, Dictionary<double, BetaWrapper>> CalcBetaCurves(List<double> betaValues, double alphaStart, NodeCostCalculator costCalc, bool inclTrans = false)
        //{
        //    // Prepare the data structures.
        //    var alphaRes = 20;
        //    var delta = (1 - alphaStart) / alphaRes;
        //    var alphas = new double[alphaRes + 1];
        //    var data = new Dictionary<string, Dictionary<double, BetaWrapper>>();

        //    // Calculate beta curves.
        //    for (int j = 0; j < betaValues.Count; j++)
        //    {
        //        for (int i = 0; i <= alphaRes; i++)
        //        {
        //            alphas[i] = alphaStart + (i) * delta;
        //            var results = costCalc.ParameterOverview(NodeGenesFactory.SpawnBeta(alphas[i], 1, betaValues[j]), inclTrans);
        //            foreach (var pair in results)
        //            {
        //                if (!data.ContainsKey(pair.Key)) data.Add(pair.Key, new Dictionary<double, BetaWrapper>());
        //                if (!data[pair.Key].ContainsKey(betaValues[j])) data[pair.Key].Add(betaValues[j], new BetaWrapper
        //                {
        //                    Beta = betaValues[j],
        //                    BetaX = alphas,
        //                    BetaY = new double[alphaRes + 1]
        //                });
        //                data[pair.Key][betaValues[j]].BetaY[i] = pair.Value;
        //            }
        //        }
        //    }

        //    return data;
        //}


        public static void ChromosomeChart(MainForm main)
        {
            var mix = 0.95;
            var basePath = @"C:\Users\Emil\Dropbox\Master Thesis\Python\chromosomes\";
            var layouts = new Dictionary<NodeGenes, string>();
            // Homogeneous layout.
            layouts.Add(NodeGenesFactory.SpawnBeta(mix, 1, 0.0), "k=1.txt");
            // Beta layouts.
            (NodeGenesFactory.SpawnBeta(mix, 1, 1.0)).Export().ToJsonFile(basePath + "beta=1.txt");
            (NodeGenesFactory.SpawnBeta(mix, 1, BusinessLogic.Utils.Utils.FindBeta(1.5, 1e-3, mix))).Export().ToJsonFile(basePath + "k=15beta.txt");
            (NodeGenesFactory.SpawnBeta(mix, 1, BusinessLogic.Utils.Utils.FindBeta(2, 1e-3, mix))).Export().ToJsonFile(basePath + "k=2beta.txt");
            (NodeGenesFactory.SpawnBeta(mix, 1, BusinessLogic.Utils.Utils.FindBeta(3, 1e-3, mix))).Export().ToJsonFile(basePath + "k=3beta.txt");
            // Maximum CF layouts.
            (NodeGenesFactory.SpawnCfMax(mix, 1, 1.5)).Export().ToJsonFile(basePath + "k=15nuMax.txt");
            (NodeGenesFactory.SpawnCfMax(mix, 1, 2)).Export().ToJsonFile(basePath + "k=2nuMax.txt");
            (NodeGenesFactory.SpawnCfMax(mix, 1, 3)).Export().ToJsonFile(basePath + "k=3nuMax.txt");

            // Save the data as JSON.
            foreach (var pair in layouts) pair.Key.Export().ToJsonFile(basePath + pair.Value);

            //view.SetData(new[] { new NodeGenes(0, 1, 1.0), new NodeGenes(1, 1, 1.0) });
            //Save(view, "Beta1Genes.png");
            //view.SetData(new[] { new NodeGenes(0, 1, 2.0), new NodeGenes(1, 1, 2.0) });
            //Save(view, "Beta2Genes.png");
            //view.SetData(new[] { new NodeGenes(0, 1, 4.0), new NodeGenes(1, 1, 4.0) });
            //Save(view, "Beta4Genes.png");

            //// Maximum CF layouts.
            //view.SetData(new[] { new NodeGenes(0, 1, 1), new NodeGenes(1, 1, 1) }, false);
            //Save(view, "NuMax1Genes.png");
            //view.SetData(new[] { new NodeGenes(0, 1, 2), new NodeGenes(1, 1, 2) });
            //Save(view, "NuMax2Genes.png");
            //view.SetData(new[] { new NodeGenes(0, 1, 5), new NodeGenes(1, 1, 5) });
            //Save(view, "NuMax5Genes.png");

            //// Genetic layouts.
            //const string basePath = @"C:\Users\Emil\Dropbox\Master Thesis\Layouts\geneticWithConstraint";
            ////var layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=1.txt");
            //var layout = new NodeGenes(0.95, 1, 1);
            //var betaLayout = new NodeGenes(layout.Alpha, layout.Gamma, BusinessLogic.Utils.Utils.FindBeta(1, 1e-3));
            //view.SetData(new[] { layout, betaLayout}, false);
            //Save(view, "LayoutK=1.png");
            //layout = new NodeGenes(0.95, 1, 2);
            ////layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=2.txt");
            //betaLayout = new NodeGenes(layout.Alpha, layout.Gamma, BusinessLogic.Utils.Utils.FindBeta(2, 1e-3));
            //view.SetData(new[] { layout, betaLayout });
            //Save(view, "LayoutK=2.png");
            ////layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=5.txt");
            //layout = new NodeGenes(0.95, 1, 5);
            //betaLayout = new NodeGenes(layout.Alpha, layout.Gamma, BusinessLogic.Utils.Utils.FindBeta(5, 1e-3));
            //view.SetData(new[] { layout, betaLayout });
            //Save(view, "LayoutK=5.png");
            //var layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=1mio.txt");
            //var betaLayout = new NodeGenes(0.95, 1, BusinessLogic.Utils.Utils.FindBeta(1e6, 1e-3));
            //view.SetData(new[] { betaLayout }, false);
            //view.MainChart.ChartAreas[0].AxisY.Interval = 20;

            //Save(view, "GeneticK=1mio.png");

            //main.Show(view);
        }

        public static void ParameterOverviewChart(MainForm main, List<double> betaValues, bool inclTrans = false)
        {
            var costCalc = new NodeCostCalculator(new ParameterEvaluator());
            var data = CalcBetaCurves(betaValues, 0.0, genes => genes.Alpha, genes => costCalc.ParameterOverview(genes, inclTrans));

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data.txt");

            // Map to view.
            var view = new ParameterOverviewChart();
            foreach (var variables in data)
            {
                view.AddData(variables.Key, variables.Value.Values.ToList(), false);
            }

            // Adjust view.
            view.MainChart.ChartAreas["BC"].AxisY.Minimum = 0.7;
            view.MainChart.ChartAreas["BE"].AxisY.Maximum = 0.7;
            view.MainChart.ChartAreas["TC"].AxisY.Minimum = 0.5;
            view.MainChart.ChartAreas["CF"].AxisY.Minimum = 0.1;

            ChartUtils.SaveChart(view.MainChart, 1000, 1000, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\ParameterOverview.png");

            main.Show(view);
        }

        public static void ParameterOverviewChartGenetic(MainForm main, List<double> kValues, bool inclTrans = false)
        {
            var costCalc = new NodeCostCalculator(new ParameterEvaluator());
            var data = CalcBetaCurves(kValues, 0.5, genes => genes.Alpha, genes => costCalc.ParameterOverview(genes, inclTrans));

            //// Calculate genetic points.
            //for (int j = 0; j < kValues.Count; j++)
            //{
            //    var genes =
            //        FileUtils.FromJsonFile<NodeGenes>(
            //            string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\geneticWithConstraintK={0}.txt",
            //                kValues[j]));
            //    var results = costCalc.ParameterOverview(genes, inclTrans);
            //    foreach (var pair in results)
            //    {
            //        data[pair.Key][kValues[j]].GeneticX = genes.Alpha;
            //        data[pair.Key][kValues[j]].GeneticY = pair.Value;
            //    }
            //}

            // Map to view.
            var view = new ParameterOverviewChart();
            foreach (var variables in data)
            {
                view.AddData(variables.Key, variables.Value.Values.ToList(), true);
            }

            // Adjust view.
            view.MainChart.ChartAreas["BC"].AxisX.Minimum = 0.5;
            view.MainChart.ChartAreas["BE"].AxisX.Minimum = 0.5;
            view.MainChart.ChartAreas["TC"].AxisX.Minimum = 0.5;
            view.MainChart.ChartAreas["CF"].AxisX.Minimum = 0.5;

            view.MainChart.ChartAreas["BC"].AxisY.Minimum = 0.7;
            view.MainChart.ChartAreas["BE"].AxisY.Minimum = 0.15;
            view.MainChart.ChartAreas["BE"].AxisY.Maximum = 0.35;
            //view.MainChart.ChartAreas["TC"].AxisY.Minimum = 0.5;
            view.MainChart.ChartAreas["CF"].AxisY.Minimum = 0.1;

            ChartUtils.SaveChart(view.MainChart, 1000, 1000, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\ParameterOverviewGenetic.png");

            main.Show(view);
        }

        private static void Save(NodeGeneChart view, string name)
        {
            ChartUtils.SaveChart(view.MainChart, 1250, 325, Path.Combine(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\", name));
        }

        #endregion

    }
}
