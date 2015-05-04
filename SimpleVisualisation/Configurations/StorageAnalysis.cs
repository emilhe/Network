using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.Utils;
using Controls;
using SimpleImporter;
using Utils;

namespace Main.Configurations
{
    class StorageAnalysis
    {

        public static void ExportParameterOverviewData5hStorage(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false };
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            }, "5h storage con sync", ExportScheme.ConstrainedSynchronized))){CacheEnabled = false};
            var opt = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            opt.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data5hSyncOpt.txt");
            Console.WriteLine("Optimization strategy 1 done");
           
            //data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data10hStorageSync.txt");
        }

        public static void ExportStorageOverview(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator() { CacheEnabled = false, Full = true};
            // No storage
            var proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data0hSyncProj.txt");
            Console.WriteLine("No storage done.");
            // 5h storage
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            }, "5h storage sync"))) { CacheEnabled = false };
            proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data5hSyncProj.txt");
            Console.WriteLine("5h storage done.");
            // 35h storage
            costCalc = new ParallelNodeCostCalculator() { CacheEnabled = false };
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupHomoStuff(nodes, 32, false, true, false);
                return nodes;
            }, "35h storage sync"))) { CacheEnabled = false };
            proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data35hSyncProj.txt");
            Console.WriteLine("35h storage done.");
            // 35h + 5h storage
            costCalc = new ParallelNodeCostCalculator() { CacheEnabled = false };    
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                return nodes;
            }, "5h35h storage sync"))) { CacheEnabled = false };
            proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data5h35hSyncProj.txt");
            Console.WriteLine("35h + 5h storage done.");

            //data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data10hStorageSync.txt");
        }

        public static void ExportStorageReal(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator() { CacheEnabled = false, Full = true };
            // No storage
            var proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data0hSyncProj.txt");
            Console.WriteLine("No storage done.");

            //data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data10hStorageSync.txt");
        }


        public static void Test(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator() { CacheEnabled = false };
            // 1h storage
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(8, () =>
            {
                var nodes = ConfigurationUtils.CreateNodes(TsSource.ISET);
                return nodes;
            }, "ISET TEST DATA"))) { CacheEnabled = false, TcCostModel = new VariableLengthModel()};
            var proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\TestData.txt");
            Console.WriteLine("Test done");
        }

        private static Dictionary<string, Dictionary<double, BetaWrapper>> CalcBetaCurves(List<double> kValues, double alphaStart, Func<NodeGenes[], double[]> evalX, Func<NodeGenes[], Dictionary<string, double>[]> evalY) //, string optPath = DefaultOptimumPath, bool skipPoints = false)
        {
            // Prepare the data structures.
            var alphaRes = 10;
            var delta = (1 - alphaStart) / alphaRes;
            var alphas = new double[alphaRes + 1];
            var betas = new double[kValues.Count];
            var data = new Dictionary<string, Dictionary<double, BetaWrapper>>();
            for (int j = 0; j < betas.Length; j++)
            {
                betas[j] = Stuff.FindBeta(kValues[j], 1e-3);
                for (int i = 0; i <= alphaRes; i++)
                {
                    alphas[i] = alphaStart + (i) * delta;
                }
            }
            // Prepare genes.
            //var optGenes = new NodeGenes[betas.Length];
            var betaGenes = new NodeGenes[betas.Length * (alphaRes + 1)];
            var cfMaxGenes = new NodeGenes[betas.Length * (alphaRes + 1)];
            for (int j = 0; j < betas.Length; j++)
            {
                for (int i = 0; i <= alphaRes; i++)
                {
                    betaGenes[i + j * (alphaRes + 1)] = NodeGenesFactory.SpawnBeta(alphas[i], 1, betas[j]);
                    cfMaxGenes[i + j * (alphaRes + 1)] = NodeGenesFactory.SpawnCfMax(alphas[i], 1, kValues[j]);
                }
                //if (skipPoints) continue;
                //optGenes[j] = FileUtils.FromJsonFile<NodeGenes>(
                //    string.Format(optPath,
                //        kValues[j]));
            }
            // Do evaluation.
            var xValues = evalX(betaGenes);
            var betaValues = evalY(betaGenes);
            //var cfMaxValues = evalY(cfMaxGenes);
            // Extract data.
            for (int j = 0; j < betas.Length; j++)
            {
                betaValues.ToJsonFile(@"C:\proto\tmp.txt");
                for (int i = 0; i <= alphaRes; i++)
                {
                    var xValue = xValues[i + j * (alphaRes + 1)];
                    var betaValue = betaValues[i + j * (alphaRes + 1)];
                    foreach (var pair in betaValue)
                    {
                        if (!data.ContainsKey(pair.Key)) data.Add(pair.Key, new Dictionary<double, BetaWrapper>());
                        if (!data[pair.Key].ContainsKey(kValues[j])) data[pair.Key].Add(kValues[j], new BetaWrapper()
                        {
                            K = kValues[j],
                            Beta = betas[j],
                            BetaX = new double[alphaRes + 1],
                            BetaY = new double[alphaRes + 1],
                            //MaxCfX = new double[alphaRes + 1],
                            //MaxCfY = new double[alphaRes + 1],
                        });
                        data[pair.Key][kValues[j]].BetaY[i] = pair.Value;
                        data[pair.Key][kValues[j]].BetaX[i] = xValue;
                    }
                    //var cfMaxValue = cfMaxValues[i + j * (alphaRes + 1)];
                    //foreach (var pair in cfMaxValue)
                    //{
                    //    data[pair.Key][kValues[j]].MaxCfY[i] = pair.Value;
                    //    data[pair.Key][kValues[j]].MaxCfX[i] = xValue;
                    //}
                }
                //// Should the optimum point be included?
                //if (!skipPoints)
                //{
                //    var optXValues = evalX(optGenes);
                //    var optYValues = evalY(optGenes);
                //    var optXValue = optXValues[j];
                //    var optYValue = optYValues[j];
                //    foreach (var pair in optYValue)
                //    {
                //        data[pair.Key][kValues[j]].GeneticX = optXValue;
                //        data[pair.Key][kValues[j]].GeneticY = pair.Value;
                //    }
                //}
            }

            return data;
        }


    }
}
