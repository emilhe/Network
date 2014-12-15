using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BusinessLogic.Cost;
using Controls;
using Controls.Article;
using Controls.Charting;
using Utils;

namespace Main.Figures
{
    class PlayGround
    {

        public static void ChromosomeChart(MainForm main)
        {
            var view = new NodeGeneChart();
            
            // Homogeneous layouts.
            view.SetData(new[] { new NodeGenes(0.95, 1)}, false);
            Save(view, "HomoGenes.png");
            
            // Beta layouts.
            view.SetData(new[] { new NodeGenes(0, 1, 1.0), new NodeGenes(1, 1, 1.0) });
            Save(view, "Beta1Genes.png");
            view.SetData(new[] { new NodeGenes(0, 1, 2.0), new NodeGenes(1, 1, 2.0) });
            Save(view, "Beta2Genes.png");
            view.SetData(new[] { new NodeGenes(0, 1, 4.0), new NodeGenes(1, 1, 4.0) });
            Save(view, "Beta4Genes.png");

            // Maximum CF layouts.
            view.SetData(new[] { new NodeGenes(0, 1, 1), new NodeGenes(1, 1, 1) }, false);
            Save(view, "NuMax1Genes.png");
            view.SetData(new[] { new NodeGenes(0, 1, 2), new NodeGenes(1, 1, 2) });
            Save(view, "NuMax2Genes.png");
            view.SetData(new[] { new NodeGenes(0, 1, 5), new NodeGenes(1, 1, 5) });
            Save(view, "NuMax5Genes.png");

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
            var betaLayout = new NodeGenes(0.95, 1, BusinessLogic.Utils.Utils.FindBeta(1e6, 1e-3));
            view.SetData(new[] { betaLayout }, false);
            view.MainChart.ChartAreas[0].AxisY.Interval = 20;

            Save(view, "GeneticK=1mio.png");

            main.Show(view);
        }

        public static void ParameterOverviewChart(MainForm main, List<double> betaValues, bool inclTrans = false)
        {
            var costCalc = new NodeCostCalculator();
            var data = CalcBetaCurves(betaValues, 0.0, costCalc, inclTrans);

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

        public static void ParameterOverviewChartGenetic(MainForm main, List<int> kValues, bool inclTrans = false)
        {
            var costCalc = new NodeCostCalculator();
            var data = CalcBetaCurves(kValues, 0.5, costCalc, inclTrans);

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

        private static Dictionary<string, Dictionary<double, BetaWrapper>> CalcBetaCurves(List<double> betaValues, double alphaStart, NodeCostCalculator costCalc, bool inclTrans = false)
        {
            // Prepare the data structures.
            var alphaRes = 20;
            var delta = (1 - alphaStart) / alphaRes;
            var alphas = new double[alphaRes + 1];
            var data = new Dictionary<string, Dictionary<double, BetaWrapper>>();

            // Calculate beta curves.
            for (int j = 0; j < betaValues.Count; j++)
            {
                for (int i = 0; i <= alphaRes; i++)
                {
                    alphas[i] = alphaStart + (i) * delta;
                    var results = costCalc.ParameterOverview(new NodeGenes(alphas[i], 1, betaValues[j]), inclTrans);
                    foreach (var pair in results)
                    {
                        if (!data.ContainsKey(pair.Key)) data.Add(pair.Key, new Dictionary<double, BetaWrapper>());
                        if (!data[pair.Key].ContainsKey(betaValues[j])) data[pair.Key].Add(betaValues[j], new BetaWrapper()
                        {
                            Beta = betaValues[j],
                            BetaX = alphas,
                            BetaY = new double[alphaRes + 1]
                        });
                        data[pair.Key][betaValues[j]].BetaY[i] = pair.Value;
                    }
                }
            }

            return data;
        }

        private static Dictionary<string, Dictionary<int, BetaWrapper>> CalcBetaCurves(List<int> kValues, double alphaStart, NodeCostCalculator costCalc, bool inclTrans = false)
        {
            // Prepare the data structures.
            var alphaRes = 20;
            var delta = (1 - alphaStart) / alphaRes;
            var alphas = new double[alphaRes + 1];
            var betas = new double[kValues.Count];
            var data = new Dictionary<string, Dictionary<int, BetaWrapper>>();
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
                    var results = costCalc.ParameterOverview(new NodeGenes(alphas[i], 1, betas[j]), inclTrans);
                    foreach (var pair in results)
                    {
                        if (!data.ContainsKey(pair.Key)) data.Add(pair.Key, new Dictionary<int, BetaWrapper>());
                        if (!data[pair.Key].ContainsKey(kValues[j])) data[pair.Key].Add(kValues[j], new BetaWrapper()
                        {
                            K = kValues[j],
                            Beta = betas[j],
                            BetaX = alphas,
                            BetaY = new double[alphaRes + 1],
                            CustomXs = alphas,
                            CustomYs = new double[alphaRes + 1],
                        });
                        data[pair.Key][kValues[j]].BetaY[i] = pair.Value;
                    }
                    results = costCalc.ParameterOverview(new NodeGenes(alphas[i], 1, kValues[j]), inclTrans);
                    foreach (var pair in results)
                    {
                        data[pair.Key][kValues[j]].CustomYs[i] = pair.Value;
                    }
                }
            }

            return data;
        }

        private static void Save(NodeGeneChart view, string name)
        {
            ChartUtils.SaveChart(view.MainChart, 1250, 325, Path.Combine(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\", name));            
        }

    }
}
