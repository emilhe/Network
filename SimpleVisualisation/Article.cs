using System;
using System.Collections.Generic;
using System.Windows.Input;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using Utils;

namespace Main
{
    public class Article
    {

        public static void DoOptimizations()
        {

            //// Default optimizations.
            var tag = "default";
            //for (var k = 1; k < 4; k++)
            //{
            //    Optimization.Sequential(k, tag);
            //}

            // Offshore fraction optimizations.
            for (var k = 1; k < 4; k++)
            {
                tag = "offshore25pct";
                var seed = new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(string.Format(@"C:\proto\seqK={0}localized.txt",k)));                
                GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.25);
                Optimization.Sequential(k, tag, null, seed);

                tag = "offshore50pct";
                seed = new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(string.Format(@"C:\proto\localK={0}offshore25pct.txt", k)));                                
                GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.50);
                Optimization.Sequential(k, tag, null, seed);
            }
            GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0);

            //// Reduced solar cost optimizations
            //for (var k = 1; k < 4; k++)
            //{
            //    tag = "solar50pct";
            //    Optimization.Sequential(k, tag, new ParallelNodeCostCalculator {SolarCostModel = new ScaledSolarCostModel(0.50)});
            //    tag = "solar25pct";
            //    Optimization.Sequential(k, tag, new ParallelNodeCostCalculator {SolarCostModel = new ScaledSolarCostModel(0.25)});
            //}


        }

        public void ExportFigureData()
        {
            var ks = new List<double>() { 1, 2, 3 };

            Figures.PlayGround.ExportChromosomeData();
            Console.WriteLine("Chromosome done...");
            //Figures.PlayGround.ExportParameterOverviewData(ks);
            Console.WriteLine("Overview done...");
            //Figures.PlayGround.ExportCostDetailsData(ks);
            Console.WriteLine("Cost done...");
            Figures.PlayGround.ExportCostNoTransDetailsData(ks);
            Console.WriteLine("Cost no trans done...");
            Figures.PlayGround.ExportSolarCostAnalysisData(ks);
            Console.WriteLine("Solar done...");
            Figures.PlayGround.ExportOffshoreCostAnalysisData(ks);
            Console.WriteLine("Offshore done...");
            Figures.PlayGround.ExportTcCalcAnalysisData();
            Console.WriteLine("TC analysis done...");

            Console.WriteLine("All done!");
        }

        public void ExportData()
        {
            
        }

    }
}