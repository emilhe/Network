using System.Collections.Generic;
using System.IO;
using BusinessLogic.Cost;
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
            view.SetData(new[] { new NodeGenes(0.8, 1)}, false);
            Save(view, "HomoGenes.png");
            
            // Beta layouts.
            view.SetData(new[] { new NodeGenes(0, 1, 1), new NodeGenes(1, 1, 1) });
            Save(view, "Beta1Genes.png");
            view.SetData(new[] { new NodeGenes(0, 1, 2), new NodeGenes(1, 1, 2) });
            Save(view, "Beta2Genes.png");
            view.SetData(new[] { new NodeGenes(0, 1, 4), new NodeGenes(1, 1, 4) });
            Save(view, "Beta4Genes.png");

            // Genetic layouts.
            const string basePath = @"C:\Users\Emil\Dropbox\Master Thesis\Layouts\geneticWithConstraint";
            var layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=1.txt");
            view.SetData(new[] { layout });
            Save(view, "GeneticK=1.png");
            layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=2.txt");
            view.SetData(new[] { layout });
            Save(view, "GeneticK=2.png");
            layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=5.txt");
            view.SetData(new[] { layout });
            Save(view, "GeneticK=5.png");
            layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=1mio.txt");
            view.SetData(new[] { layout });
            Save(view, "GeneticK=1mio.png");

            main.Show(view);
        }

        public static void ParameterOverviewChart(MainForm main, List<double> betaValues, bool inclTrans = false)
        {
            var view = new ParameterOverviewChart();

            // Construct data.
            var alphaStart = 0;
            var alphaRes = 10;
            var delta = (double) (1 - alphaStart) / alphaRes;
            var costCalc = new NodeCostCalculator();
            // Calculate costs and prepare data structures.
            var alphas = new double[alphaRes + 1];
            var data = new Dictionary<string, Dictionary<double, double[]>>();
            // Main loop.
            for (int j = 0; j < betaValues.Count; j++)
            {
                for (int i = 0; i <= alphaRes; i++)
                {
                    alphas[i] = alphaStart + (i) * delta;
                    var results = costCalc.ParameterOverview(new NodeGenes(alphas[i], 1, betaValues[j]), inclTrans);
                    foreach (var pair in results)
                    {
                        if (!data.ContainsKey(pair.Key)) data.Add(pair.Key, new Dictionary<double, double[]>());
                        if (!data[pair.Key].ContainsKey(betaValues[j])) data[pair.Key].Add(betaValues[j], new double[alphaRes + 1]);
                        data[pair.Key][betaValues[j]][i] = pair.Value;
                    }
                }
            }

            // Map to view.
            foreach (var variables in data)
            {
                foreach (var betas in variables.Value)
                {
                    view.AddData(variables.Key, betas.Key, alphas, betas.Value);
                }
            }

            // Adjust view.
            view.MainChart.ChartAreas["BC"].AxisY.Minimum = 0.7;
            view.MainChart.ChartAreas["BE"].AxisY.Maximum = 0.7;
            view.MainChart.ChartAreas["TC"].AxisY.Minimum = 0.5;
            view.MainChart.ChartAreas["CF"].AxisY.Minimum = 0.15;

            ChartUtils.SaveChart(view.MainChart, 1000, 1000, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\ParameterOverview.png");            

            main.Show(view);
        }


        private static void Save(NodeGeneChart view, string name)
        {
            ChartUtils.SaveChart(view.MainChart, 1250, 325, Path.Combine(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\", name));            
        }

    }
}
