using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Simulation;
using BusinessLogic.Utils;
using Controls;
using Controls.Article;
using Controls.Charting;
using SimpleImporter;
using Utils;

namespace Main.Figures
{
    static class PlayGround
    {

        public const string DefaultOptimumPath = @"C:\Users\Emil\Dropbox\BACKUP\Layouts\VE50gadK={0}@default.txt";
        public const string SolarCost25PctOptimumPath = @"C:\Users\Emil\Dropbox\BACKUP\Layouts\VE50gadK={0}@solar25pct.txt";
        public const string SolarCost50PctOptimumPath = @"C:\Users\Emil\Dropbox\BACKUP\Layouts\VE50gadK={0}@solar50pct.txt";
        public const string SolarCost75PctOptimumPath = @"C:\Users\Emil\Dropbox\BACKUP\Layouts\VE50gadK={0}@solar75pct.txt";
        public const string Offshore25PctOptimumPath = @"C:\Users\Emil\Dropbox\BACKUP\Layouts\VE50gadK={0}@offshore25pct.txt";
        public const string Offshore50PctOptimumPath = @"C:\Users\Emil\Dropbox\BACKUP\Layouts\VE50gadK={0}@offshore50pct.txt";
        public const string StorageOptimumPath = @"C:\Users\Emil\Dropbox\BACKUP\Layouts\VE50gadK={0}@{1}@unbiased.txt";

        #region Data export to JSON for external rendering

        #region Primary data

        public static void ExportChromosomeData()
        {
            var mix = 0.84;
            var basePath = @"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\chromosomes\";
            var layouts = new Dictionary<NodeGenes, string>();

            // Standard beta/max CF layouts
            layouts.Add(NodeGenesFactory.SpawnBeta(1, 1, 1), "beta=1wind.txt");
            layouts.Add(NodeGenesFactory.SpawnBeta(0, 1, 1), "beta=1solar.txt");
            layouts.Add(NodeGenesFactory.SpawnCfMax(1, 1, 2), "k=2cfMaxWind.txt");
            layouts.Add(NodeGenesFactory.SpawnCfMax(0, 1, 2), "k=2cfMaxSolar.txt");

            // Layouts for different K values.
            for (int k = 1; k < 4; k++)
            {
                // Beta layouts.
                layouts.Add(NodeGenesFactory.SpawnBeta(mix, 1, Stuff.FindBeta(k, 1e-3, mix)), string.Format("k={0}beta.txt", k));
                // Maximum CF layouts.
                layouts.Add(NodeGenesFactory.SpawnCfMax(mix, 1, k), string.Format("k={0}cfMax.txt", k));
                // Optimized layouts.
                layouts.Add(FileUtils.FromJsonFile<NodeGenes>(string.Format(DefaultOptimumPath, k)), string.Format("k={0}optimized.txt.", k));
            }

            // Optimized OFFSHORE layouts.
            for (int k = 1; k < 4; k++)
            {
                GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.25);
                var genes = FileUtils.FromJsonFile<NodeGenes>(string.Format(Offshore25PctOptimumPath, k));
                GenePool.ApplyOffshoreFraction(genes);
                layouts.Add(genes, string.Format("k={0}offshore25pct.txt", k));
                GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.5);
                genes = FileUtils.FromJsonFile<NodeGenes>(string.Format(Offshore50PctOptimumPath, k));
                GenePool.ApplyOffshoreFraction(genes);
                layouts.Add(genes, string.Format("k={0}offshore50pct.txt", k));
                GenePool.OffshoreFractions = null;
            }

            // Optimized SOLAR layouts.
            for (int k = 1; k < 4; k++)
            {
                var genes = FileUtils.FromJsonFile<NodeGenes>(string.Format(SolarCost25PctOptimumPath, k));
                layouts.Add(genes, string.Format("k={0}solar25pct.txt", k));
                genes = FileUtils.FromJsonFile<NodeGenes>(string.Format(SolarCost50PctOptimumPath, k));
                layouts.Add(genes, string.Format("k={0}solar50pct.txt", k));
            }
            
            // Optimized STORAGE layouts.
            for (int k = 1; k < 4; k++)
            {
                var genes = FileUtils.FromJsonFile<NodeGenes>(string.Format(StorageOptimumPath, k, "Reference"));
                layouts.Add(genes, string.Format("VE50gadK={0}@Reference@unbiased.txt", k));
                genes = FileUtils.FromJsonFile<NodeGenes>(string.Format(StorageOptimumPath, k, "5h+35h"));
                layouts.Add(genes, string.Format("VE50gadK={0}@5h+35h@unbiased.txt", k));
            }

            // Save the data as JSON.
            foreach (var pair in layouts) pair.Key.Export().ToJsonFile(Path.Combine(basePath,pair.Value));
        }

        public static void ExportParameterOverviewData(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };

            var data = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\overviews\dataSync.txt");
        }

        public static void ExportParameterOverviewDataNone(List<double> kValues)
        {
            ExportParameterOverviewDataReal(kValues, false, false, "Reference");
        }

        public static void ExportParameterOverviewData5h(List<double> kValues)
        {
            ExportParameterOverviewDataReal(kValues, true, false, "5h");
        }

        public static void ExportParameterOverviewData35h(List<double> kValues)
        {
            ExportParameterOverviewDataReal(kValues, false, true, "35h");
        }

        public static void ExportParameterOverviewData5h35h(List<double> kValues)
        {
            ExportParameterOverviewDataReal(kValues, true, true, "5h+35h");
        }

        private static void ExportParameterOverviewDataReal(List<double> kValues, bool small, bool large, string tag)
        {
            var optPath = string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Layouts\VE50gadK={{0}}@{0}.txt", string.Format("{0}@unbiased",tag));
            var savePath = string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_prod\overviews\real{0}.txt", tag);

            var costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, small, large, false);
                return nodes;
            }, string.Format("{0} con sync",tag)))) { CacheEnabled = false };

            var data = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)),optPath);

            data.ToJsonFile(savePath);
        }

        public static void ExportCostDetailsData(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = false, Full = true };
            var geneMap = new Dictionary<string, Func<double, NodeGenes>>
            {
                {@"Beta@K={0}", k => NodeGenesFactory.SpawnBeta(0.84, 1, Stuff.FindBeta(k, 1e-3))},
                {@"CfMax@K={0}", k => NodeGenesFactory.SpawnCfMax(0.84, 1, k)},
                {@"CS@K={0}", k => FileUtils.FromJsonFile<NodeGenes>(string.Format(DefaultOptimumPath, k))}
            };
            var data = CalcCostDetails(kValues, geneMap, genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.DetailedSystemCosts(nodeGenes)));

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\costs\cost.txt");
        }

        //public static void ExportCostNoTransDetailsData(List<double> kValues)
        //{
        //    var costCalc = new ParallelNodeCostCalculator { CacheEnabled = true, Full = true };
        //    var geneMap = new Dictionary<string, Func<double, NodeGenes>>
        //    {
        //        {@"Beta@K={0}", k => NodeGenesFactory.SpawnBeta(1, 1, Stuff.FindBeta(k, 1e-3))},
        //        {@"CfMax@K={0}", k => NodeGenesFactory.SpawnCfMax(1, 1, k)},
        //        {@"CS@K={0}", k => FileUtils.FromJsonFile<NodeGenes>(string.Format(NoTransOptimumPath, k))}
        //    };
        //    var data = CalcCostDetails(kValues, geneMap, genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.DetailedSystemCosts(nodeGenes, true)));

        //    data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\costs\costNoTrans.txt");
        //}

        public static void ExportMismatchData(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = true, Full = true };
            var data = CalcBetaCurves(kValues, 0.0,
                genes =>
                    costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.Evaluator.Sigma(nodeGenes))
                        .ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => new Dictionary<string, double>
                {
                    {"CF", calculator.Evaluator.CapacityFactor(nodeGenes)},
                    {"LCOE", calculator.SystemCost(nodeGenes)}
                }).ToArray());

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\sigmas\sigma.txt");
        }

        // TODO: What grid data?
        public static void ExportGridData()
        {
            //var ntcData =
            //    ProtoStore.LoadLinkData("NtcMatrix")
            //        .Where(item => !item.From.Equals(item.To))
            //        .Where(item => item.LinkCapacity > 0)
            //        .Select(item => new LinkDataRow
            //        {
            //            From = CountryInfo.GetShortAbbrev(item.From),
            //            To = CountryInfo.GetShortAbbrev(item.To),
            //            LinkCapacity = item.LinkCapacity/1000
            //        });
            //ntcData.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\transmission\ntcEdges.txt");
        }

        #endregion

        #region Sensitivity analysis

        /// <summary>
        /// Solar cost sensitivity analysis.
        /// </summary>
        public static void ExportSolarCostAnalysisData(List<double> kValues)
        {
            var scales = new Dictionary<double, string> { { 0, DefaultOptimumPath }, { 0.25, SolarCost25PctOptimumPath }, { 0.50, SolarCost50PctOptimumPath }, { 0.75, SolarCost75PctOptimumPath } };
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = false, Full = true };
            var results = new Dictionary<string, Dictionary<double, BetaWrapper>>();

            foreach (var scale in scales)
            {
                costCalc.SolarCostModel = new ScaledSolarCostModel(1 - scale.Key);
                var data = CalcBetaCurves(kValues, 0.0,
                    genes => genes.Select(item => item.Alpha).ToArray(),
                    genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => new Dictionary<string, double>()
                    {
                        {scale.Key + "",calculator.SystemCost(nodeGenes)}
                    }), scale.Value);
                foreach (var key in data.Keys) results.Add(key, data[key]);
            }

            results.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\solar\solarAnalysis.txt");
        }

        public static void ExportOffshoreCostAnalysisData(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = true, Full = true };
            var geneMap = new Dictionary<string, Func<double, NodeGenes>>
            {
                {@"0%@K={0}", k =>
                {
                    var result = FileUtils.FromJsonFile<NodeGenes>(string.Format(DefaultOptimumPath, k));
                    GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.0);
                    GenePool.ApplyOffshoreFraction(result);
                    return result;
                }},
                {@"25%@K={0}", k => 
                                    {
                    var result = FileUtils.FromJsonFile<NodeGenes>(string.Format(Offshore25PctOptimumPath, k));
                    GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.25);
                    GenePool.ApplyOffshoreFraction(result);
                    return result;
                }},
                {@"50%@K={0}", k => 
                                    {
                    var result = FileUtils.FromJsonFile<NodeGenes>(string.Format(Offshore50PctOptimumPath, k));
                    GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.50);
                    GenePool.ApplyOffshoreFraction(result);
                    return result;
                }}
            };
            var data = CalcCostDetails(kValues, geneMap, genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.DetailedSystemCosts(nodeGenes)));
            // Reset offshore fractions.
            GenePool.OffshoreFractions = null;

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\costs\costOffshore.txt");
        }

        /// <summary>
        /// Transmission sensitivity analysis.
        /// </summary>
        public static void ExportTcCalcAnalysisData()
        {
            var evaluator = new ParameterEvaluator(true) {CacheEnabled = false};
            var layouts =
                Directory.GetFiles(@"C:\Users\Emil\Dropbox\BACKUP\Python\sandbox\Layouts")
                    .Where(item => !item.Contains(".pdf") && !item.Contains("LINKS"));
            //var layouts = new[]
            //{
            //    string.Format(DefaultOptimumPath, 1),
            //    string.Format(DefaultOptimumPath, 2),
            //    string.Format(DefaultOptimumPath, 3),
            //    //string.Format(NoTransOptimumPath, 1),
            //    //string.Format(NoTransOptimumPath, 2),
            //    //string.Format(NoTransOptimumPath, 3),
            //};

            foreach (var layout in layouts)
            {
                // What genes?  
                var genes = FileUtils.FromJsonFile<NodeGenes>(string.Format(layout));
                var capacities = evaluator.LinkCapacities(genes);
                var links = capacities.Select(MapLink);
                var fi = new FileInfo(layout);
                links.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\sandbox\Layouts\{0}LINKS.txt",
                    fi.Name.Substring(0, fi.Name.Length-4)));
            }

            for (int k = 1; k < 4; k++)
            {
                // What genes?
                var genes = NodeGenesFactory.SpawnCfMax(0.84, 1, k);
                var capacities = evaluator.LinkCapacities(genes);
                var links = capacities.Select(MapLink);
                links.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\sandbox\Layouts\cfMaxK={0}LINKS.txt", k));
            }

        }

        /// <summary>
        /// Transmission sensitivity analysis.
        /// </summary>
        public static void ExportTcCalcAnalysisDataTmp()
        {
            ParameterEvaluator evaluator;
            String name;
            var genes = new NodeGenes(0, 1);

            UncSyncScheme.Bias = 0.14;
            name = "storage";
            evaluator = new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                return nodes;
            }, "real storage v1.0 w5h")) { CacheEnabled = false };
            var capacities = evaluator.LinkCapacities(genes);
            var links = capacities.Select(MapLink);
            links.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Python\transmission\{0}LINKS.txt",
                name));


            UncSyncScheme.Bias = 0.30;
            name = "noStorage";
            evaluator = new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                //ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            }, "real storage v1.0 w5h")) { CacheEnabled = false };
            capacities = evaluator.LinkCapacities(genes);
            links = capacities.Select(MapLink);
            links.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Python\transmission\{0}LINKS.txt",
                name));
        }

        public static LinkDataRow MapLink(KeyValuePair<string, double> pair)
        {
            return new LinkDataRow
            {
                LinkCapacity = pair.Value,
                From = CountryInfo.GetShortAbbrev(pair.Key.Split('-')[0]),
                To = CountryInfo.GetShortAbbrev(pair.Key.Split('-')[1]),
                Type = Costs.LinkType[pair.Key]
            };
        }

        #endregion

        #region New data

        /// <summary>
        /// Simple alpha depence overview
        /// </summary>
        public static void ExportEuropeAggregateData()
        {
            var nodes = ConfigurationUtils.CreateNodesNew();
            var builder = new StringBuilder();

            var loads = nodes.Select(item => item.Model.LoadTimeSeries.GetAllValues()).ToArray();
            var winds = nodes.Select(item => item.Model.OnshoreWindTimeSeries.GetAllValues()).ToArray();
            var solars = nodes.Select(item => item.Model.SolarTimeSeries.GetAllValues()).ToArray();

            for (int i = 0; i < nodes[0].Model.SolarTimeSeries.Count(); i++)
            {
                var load = loads.Select(item => item[i]).Average();
                var wind = winds.Select(item => item[i]).Average();
                var solar = solars.Select(item => item[i]).Average();
                builder.AppendLine(string.Format("{0},{1},{2}", load, wind, solar));
            }
            File.WriteAllText(@"C:\Users\Emil\Dropbox\BACKUP\Python\eu_data.txt",builder.ToString());
        }

        /// <summary>
        /// Simple alpha depence overview
        /// </summary>
        public static void ExportSimpleOverview()
        {
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = false, Full = true };

            var data = CalcBetaCurves(new List<double>{1}, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)),"", true);

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\overviews\simpleSync.txt");
        }

        public static void ExportParameterOverviewData6hStorage(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator();
            // Build custom core to use storage.
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            }, "10h storage sync")));
            // Evaluate data.
            var data = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)), @"C:\proto\localK={0}sync10h.txt");

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data10hStorageSync.txt");
        }

        /// <summary>
        /// Transmission sensitivity analysis.
        /// </summary>
        public static void ExportTcCalcAnalysisData6hStorage()
        {
            var evaluator = new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            }, "6h storage")) {CacheEnabled = true};

            for (int k = 1; k < 4; k++)
            {
                // What genes?
                var genes = NodeGenesFactory.SpawnCfMax(0, 1, k);
                var capacities = evaluator.LinkCapacities(genes);
                var links = capacities.Select(MapLink);
                links.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Python\transmission\cfMaxK={0}6hLINKSalpha0.txt", k));
            }

        }

        #endregion

        #endregion

        private static Dictionary<string, Dictionary<double, BetaWrapper>> CalcBetaCurves(List<double> kValues, double alphaStart, Func<NodeGenes[], double[]> evalX, Func<NodeGenes[], Dictionary<string, double>[]> evalY, string optPath = DefaultOptimumPath, bool skipPoints = false)
        {
            // Prepare the data structures.
            var alphaRes = 25;
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
            var optGenes = new NodeGenes[betas.Length];
            var betaGenes = new NodeGenes[betas.Length * (alphaRes + 1)];
            var cfMaxGenes = new NodeGenes[betas.Length * (alphaRes + 1)];
            for (int j = 0; j < betas.Length; j++)
            {
                for (int i = 0; i <= alphaRes; i++)
                {
                    betaGenes[i + j * (alphaRes + 1)] = NodeGenesFactory.SpawnBeta(alphas[i], 1, betas[j]);
                    cfMaxGenes[i + j * (alphaRes + 1)] = NodeGenesFactory.SpawnCfMax(alphas[i], 1, kValues[j]);
                }
                if (skipPoints) continue;
                optGenes[j] = FileUtils.FromJsonFile<NodeGenes>(
                    string.Format(optPath,
                        kValues[j]));
            }
            // Do evaluation.
            var xValues = evalX(betaGenes);
            var betaValues = evalY(betaGenes);
            var cfMaxValues = evalY(cfMaxGenes);
            // Extract data.
            for (int j = 0; j < betas.Length; j++)
            {
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
                            MaxCfX = new double[alphaRes + 1],
                            MaxCfY = new double[alphaRes + 1],
                        });
                        data[pair.Key][kValues[j]].BetaY[i] = pair.Value;
                        data[pair.Key][kValues[j]].BetaX[i] = xValue;
                    }
                    var cfMaxValue = cfMaxValues[i + j * (alphaRes + 1)];
                    foreach (var pair in cfMaxValue)
                    {
                        data[pair.Key][kValues[j]].MaxCfY[i] = pair.Value;
                        data[pair.Key][kValues[j]].MaxCfX[i] = xValue;
                    }
                }
                // Should the optimum point be included?
                if (!skipPoints)
                {
                    var optXValues = evalX(optGenes);
                    var optYValues = evalY(optGenes);
                    var optXValue = optXValues[j];
                    var optYValue = optYValues[j];
                    foreach (var pair in optYValue)
                    {
                        data[pair.Key][kValues[j]].GeneticX = optXValue;
                        data[pair.Key][kValues[j]].GeneticY = pair.Value;
                    }   
                }

                Console.WriteLine("Beta {0} done", betas[j]);
            }

            return data;
        }

        private static CostWrapper CalcCostDetails(List<double> kValues, Dictionary<string, Func<double, NodeGenes>> geneMap, Func<NodeGenes[], Dictionary<string, double>[]> eval, string optPath = DefaultOptimumPath)
        {
            // Prepare the data structures.
            var data = new CostWrapper
            {
                Costs = new Dictionary<string, List<double>>(),
                Labels = new List<string>()
            };
            var allGenes = new NodeGenes[kValues.Count * geneMap.Keys.Count];
            var keys = geneMap.Keys.ToArray();
            // Build layouts.
            for (int index = 0; index < kValues.Count; index++)
            {
                var k = kValues[index];
                for (int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    data.Labels.Add(string.Format(key, k));
                    allGenes[i + index * geneMap.Keys.Count] = geneMap[key](k);
                }
            }
            // Do evaluation.
            var allResults = eval(allGenes);
            // Extract results.
            for (int index = 0; index < kValues.Count; index++)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    foreach (var pair in allResults[i + index * keys.Length])
                    {
                        if (!data.Costs.ContainsKey(pair.Key)) data.Costs.Add(pair.Key, new List<double>());
                        data.Costs[pair.Key].Add(pair.Value);
                    }
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
            (NodeGenesFactory.SpawnBeta(mix, 1, Stuff.FindBeta(1.5, 1e-3, mix))).Export().ToJsonFile(basePath + "k=15beta.txt");
            (NodeGenesFactory.SpawnBeta(mix, 1, Stuff.FindBeta(2, 1e-3, mix))).Export().ToJsonFile(basePath + "k=2beta.txt");
            (NodeGenesFactory.SpawnBeta(mix, 1, Stuff.FindBeta(3, 1e-3, mix))).Export().ToJsonFile(basePath + "k=3beta.txt");
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
            //var betaLayout = new NodeGenes(layout.Alpha, layout.Gamma, BusinessLogic.Stuff.Stuff.FindBeta(1, 1e-3));
            //view.SetData(new[] { layout, betaLayout}, false);
            //Save(view, "LayoutK=1.png");
            //layout = new NodeGenes(0.95, 1, 2);
            ////layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=2.txt");
            //betaLayout = new NodeGenes(layout.Alpha, layout.Gamma, BusinessLogic.Stuff.Stuff.FindBeta(2, 1e-3));
            //view.SetData(new[] { layout, betaLayout });
            //Save(view, "LayoutK=2.png");
            ////layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=5.txt");
            //layout = new NodeGenes(0.95, 1, 5);
            //betaLayout = new NodeGenes(layout.Alpha, layout.Gamma, BusinessLogic.Stuff.Stuff.FindBeta(5, 1e-3));
            //view.SetData(new[] { layout, betaLayout });
            //Save(view, "LayoutK=5.png");
            //var layout = FileUtils.FromJsonFile<NodeGenes>(basePath + "K=1mio.txt");
            //var betaLayout = new NodeGenes(0.95, 1, BusinessLogic.Stuff.Stuff.FindBeta(1e6, 1e-3));
            //view.SetData(new[] { betaLayout }, false);
            //view.MainChart.ChartAreas[0].AxisY.Interval = 20;

            //Save(view, "GeneticK=1mio.png");

            //main.Show(view);
        }

        public static void ParameterOverviewChart(MainForm main, List<double> betaValues, bool inclTrans = false)
        {
            var costCalc = new ParallelNodeCostCalculator()
            {
                CacheEnabled = false,
                Full = false,
            };
            var data = CalcBetaCurves(betaValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));

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

        //public static void ParameterOverviewChartGenetic(MainForm main, List<double> kValues, bool inclTrans = false)
        //{
        //    var costCalc = new NodeCostCalculator(new ParameterEvaluator(false));
        //    var data = CalcBetaCurves(kValues, 0.5, genes => genes.Alpha, genes => costCalc.ParameterOverview(genes, inclTrans));

        //    //// Calculate genetic points.
        //    //for (int j = 0; j < kValues.Count; j++)
        //    //{
        //    //    var genes =
        //    //        FileUtils.FromJsonFile<NodeGenes>(
        //    //            string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\geneticWithConstraintK={0}.txt",
        //    //                kValues[j]));
        //    //    var results = costCalc.ParameterOverview(genes, inclTrans);
        //    //    foreach (var pair in results)
        //    //    {
        //    //        data[pair.Key][kValues[j]].GeneticX = genes.Alpha;
        //    //        data[pair.Key][kValues[j]].GeneticY = pair.Value;
        //    //    }
        //    //}

        //    // Map to view.
        //    var view = new ParameterOverviewChart();
        //    foreach (var variables in data)
        //    {
        //        view.AddData(variables.Key, variables.Value.Values.ToList(), true);
        //    }

        //    // Adjust view.
        //    view.MainChart.ChartAreas["BC"].AxisX.Minimum = 0.5;
        //    view.MainChart.ChartAreas["BE"].AxisX.Minimum = 0.5;
        //    view.MainChart.ChartAreas["TC"].AxisX.Minimum = 0.5;
        //    view.MainChart.ChartAreas["CF"].AxisX.Minimum = 0.5;

        //    view.MainChart.ChartAreas["BC"].AxisY.Minimum = 0.7;
        //    view.MainChart.ChartAreas["BE"].AxisY.Minimum = 0.15;
        //    view.MainChart.ChartAreas["BE"].AxisY.Maximum = 0.35;
        //    //view.MainChart.ChartAreas["TC"].AxisY.Minimum = 0.5;
        //    view.MainChart.ChartAreas["CF"].AxisY.Minimum = 0.1;

        //    ChartUtils.SaveChart(view.MainChart, 1000, 1000, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\ParameterOverviewGenetic.png");

        //    main.Show(view);
        //}

        private static void Save(NodeGeneChart view, string name)
        {
            ChartUtils.SaveChart(view.MainChart, 1250, 325, Path.Combine(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\", name));
        }

        #endregion

    }
}
