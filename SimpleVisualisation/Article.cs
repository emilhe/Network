using System;
using System.Collections.Generic;
using System.Windows.Input;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.Utils;
using Utils;

namespace Main
{
    public class Article
    {

        public static void DoOptimizations()
        {
            //// Build custom core to use storage.
            //var costCalc = new ParallelNodeCostCalculator();   
            //var tag = "sync5h";            
            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodesNew();
            //    ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
            //    return nodes;
            //}, "5h storage sync")));

            //// Default optimizations.
            //for (var k = 1; k < 4; k++)
            //{
            //    Optimization.Sequential(k);
            //    //Optimization.Sequential(k, tag, costCalc);
            //}

            //// Offshore fraction optimizations.
            //for (var k = 1; k < 4; k++)
            //{
            //    GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.25);
            //    Optimization.Sequential(k,"offshore25pct");
            //    //var seed = new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(string.Format(@"C:\proto\seqK={0}localized.txt", k)));
            //    //GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.25);
            //    //Optimization.Sequential(k, tag, null, seed);

            //    GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.50);
            //    Optimization.Sequential(k, "offshore50pct");
            //    //seed = new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(string.Format(@"C:\proto\localK={0}offshore25pct.txt", k)));
            //    //GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.50);
            //    //Optimization.Sequential(k, tag, null, seed);
            //}
            //GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0);

            //// Reduced solar cost optimizations
            //for (var k = 1; k < 4; k++)
            //{
            //    Optimization.Sequential(k, "solar25pct",
            //        new ParallelNodeCostCalculator { SolarCostModel = new ScaledSolarCostModel((1 - 0.25)) });
            //    Optimization.Sequential(k, "solar50pct",
            //        new ParallelNodeCostCalculator { SolarCostModel = new ScaledSolarCostModel((1 - 0.50)) });
            //    Optimization.Sequential(k, "solar75pct",
            //        new ParallelNodeCostCalculator {SolarCostModel = new ScaledSolarCostModel((1 - 0.75))});
            //}
        }

        public static void ExportFigureData()
        {
            var ks = new List<double>() { 1, 2, 3 };

            //Figures.PlayGround.ExportChromosomeData();
            //Console.WriteLine("Chromosome done...");
            //Figures.PlayGround.ExportParameterOverviewData(ks);
            //Console.WriteLine("Overview done...");
            //Figures.PlayGround.ExportCostDetailsData(ks);
            //Console.WriteLine("Cost done...");
            //Figures.PlayGround.ExportSolarCostAnalysisData(ks);
            //Console.WriteLine("Solar done...");
            //Figures.PlayGround.ExportOffshoreCostAnalysisData(ks);
            //Console.WriteLine("Offshore done...");
            //Figures.PlayGround.ExportTcCalcAnalysisData();
            //Console.WriteLine("TC analysis done...");

            Console.WriteLine("All done!");
        }

        public static void ExportExtraFigureData()
        {
            var ks = new List<double>() { 1, 2, 3 };

            Figures.PlayGround.ExportTcCalcAnalysisData();
            //Figures.PlayGround.ExportParameterOverviewDataNone(ks);
            //Figures.PlayGround.ExportParameterOverviewData5h(ks);
            //Figures.PlayGround.ExportParameterOverviewData35h(ks);
            //Figures.PlayGround.ExportParameterOverviewData5h35h(ks);
  
            Console.WriteLine("All done!");
        }

        public void ExportData()
        {
            
        }

    }
}