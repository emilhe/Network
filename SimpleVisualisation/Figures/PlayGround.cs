using System.IO;
using BusinessLogic.Cost;
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
            //layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=1mio.txt");
            //view.SetData(new[] { layout });
            //Save(view, "GeneticK=1mio.png");

            main.Show(view);
        }

        private static void Save(NodeGeneChart view, string name)
        {
            ChartUtils.SaveChart(view.MainChart, 1250, 325, Path.Combine(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\", name));            
        }

    }
}
