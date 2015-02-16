using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.Utils;
using Controls;
using Controls.Article;
using Controls.Charting;
using SimpleImporter;
using Utils;

namespace Main.Figures
{
    class PlayGround
    {

        // These layouts are NOT well defined.
        private const string DefaultOptimumPath = @"C:\proto\VE50cukooK={0}@default.txt";

        private const string SolarCost25PctOptimumPath = @"C:\proto\VE50cukooK={0}@solar25pct.txt";
        private const string SolarCost50PctOptimumPath = @"C:\proto\VE50cukooK={0}@solar50pct.txt";

        private const string Offshore25PctOptimumPath = @"C:\proto\VE50cukooK={0}@offshore25pct.txt";
        private const string Offshore50PctOptimumPath = @"C:\proto\VE50cukooK={0}@offshore50pct.txt";
        
        #region Data export to JSON for external rendering

        #region Primary data

        public static void ExportChromosomeData()
        {
            var mix = 1;
            var basePath = @"C:\Users\Emil\Dropbox\Master Thesis\Python\chromosomes\";
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
                layouts.Add(FileUtils.FromJsonFile<NodeGenes>(string.Format(DefaultOptimumPath, k)), string.Format("k={0}optimized.txt.",k));
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

            // Save the data as JSON.
            foreach (var pair in layouts) pair.Key.Export().ToJsonFile(basePath + pair.Value);  
        }

        public static void ExportParameterOverviewData(List<double> kValues, bool inclTrans = false)
        {
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = true, Full = true };
            var data = CalcBetaCurves(kValues, 0.0, 
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, inclTrans)), @"C:\proto\VE50cukooK={0}@TRANS10k.txt");

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\overviews\data.txt");
        }
        
        public static void ExportCostDetailsData(List<double> kValues, bool inclTrans = false)
        {
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = true, Full = true};
            var geneMap = new Dictionary<string, Func<double, NodeGenes>>
            {
                {@"Beta@K={0}", k => NodeGenesFactory.SpawnBeta(1, 1, Stuff.FindBeta(k, 1e-3))},
                {@"CfMax@K={0}", k => NodeGenesFactory.SpawnCfMax(1, 1, k)},
                {@"CS@K={0}", k => FileUtils.FromJsonFile<NodeGenes>(string.Format(@"C:\proto\VE50cukooK={0}@TRANS10k.txt", k))}
            };
            var data = CalcCostDetails(kValues, geneMap, genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.DetailedSystemCosts(nodeGenes, inclTrans)));

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\costs\cost.txt");
        }

        public static void ExportCostTransDetailsData(List<double> kValues, bool inclTrans = false)
        {
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = true, Full = true};
            var geneMap = new Dictionary<string, Func<double, NodeGenes>>
            {
                {@"Beta@K={0}", k => NodeGenesFactory.SpawnBeta(1, 1, Stuff.FindBeta(k, 1e-3))},
                {@"CfMax@K={0}", k => NodeGenesFactory.SpawnCfMax(1, 1, k)},
                {@"CS@K={0}", k => FileUtils.FromJsonFile<NodeGenes>(string.Format(@"C:\proto\VE50cukooK={0}@TRANS10k.txt", k))}
            };
            var data = CalcCostDetails(kValues, geneMap, genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.DetailedSystemCosts(nodeGenes, inclTrans)));

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\costs\costTrans.txt");
        }

        public static void ExportMismatchData(List<double> kValues, bool inclTrans = false)
        {
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = true, Full = true, Transmission = inclTrans };
            var data = CalcBetaCurves(kValues, 0.0,
                genes =>
                    costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.Evaluator.Sigma(nodeGenes))
                        .ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => new Dictionary<string, double>
                {
                    {"CF", calculator.Evaluator.CapacityFactor(nodeGenes)},
                    {"LCOE", calculator.SystemCost(nodeGenes, inclTrans)}
                }).ToArray());

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\sigmas\sigma.txt");
        }

        // TODO: What grid data?
        public static void ExportGridData()
        {
            //var ntcData =
            //    ProtoStore.LoadLinkData("NtcMatrix")
            //        .Where(item => !item.CountryFrom.Equals(item.CountryTo))
            //        .Where(item => item.LinkCapacity > 0)
            //        .Select(item => new LinkDataRow
            //        {
            //            CountryFrom = CountryInfo.GetShortAbbrev(item.CountryFrom),
            //            CountryTo = CountryInfo.GetShortAbbrev(item.CountryTo),
            //            LinkCapacity = item.LinkCapacity/1000
            //        });
            //ntcData.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\transmission\ntcEdges.txt");
        }

        #endregion

        #region Sensitivity analysis

        /// <summary>
        /// Solar cost sensitivity analysis.
        /// </summary>
        public static void ExportSolarCostAnalysisData(List<double> kValues, bool inclTrans = false)
        {
            var scales = new Dictionary<double, string> {{1.0, DefaultOptimumPath}, {2.0, SolarCost50PctOptimumPath}, {4.0, SolarCost25PctOptimumPath} };
            var costCalc = new ParallelNodeCostCalculator { CacheEnabled = true, Full = true };
            var results = new Dictionary<string, Dictionary<double, BetaWrapper>>();

            foreach (var scale in scales)
            {
                costCalc.SolarCostModel = new ScaledSolarCostModel(1 / scale.Key);
                var data = CalcBetaCurves(kValues, 0.0,
                    genes => genes.Select(item => item.Alpha).ToArray(),
                    genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => new Dictionary<string, double>()
                    {
                        {scale + "",calculator.SystemCost(nodeGenes, inclTrans)}
                    }), scale.Value);
                foreach (var key in data.Keys) results.Add(key, data[key]);
            }

            results.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\solar\solarAnalysis.txt");
        }

        public static void ExportOffshoreCostAnalysisData(List<double> kValues, bool inclTrans = false)
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
            var data = CalcCostDetails(kValues, geneMap, genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.DetailedSystemCosts(nodeGenes, inclTrans)));
            // Reset offshore fractions.
            GenePool.OffshoreFractions = null;

            data.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\costs\costOffshore.txt");
        }

        /// <summary>
        /// Transmission sensitivity analysis.
        /// </summary>
        public static void ExportTcCalcAnalysisData()
        {
            var evaluator = new ParameterEvaluator(true);
            var layouts = new[]
            {
                "VE50cukooK=1@TRANS10k",
                "VE50cukooK=2@TRANS10k",
                "VE50cukooK=3@TRANS10k",
                "VE50cukooK=1@default",
                "VE50cukooK=2@default",
                "VE50cukooK=3@default"
            };
            
            foreach (var layout in layouts)
            {
                // What genes?
                var genes = FileUtils.FromJsonFile<NodeGenes>(string.Format(@"C:\proto\{0}.txt", layout));
                var capacities = evaluator.LinkCapacities(genes);
                var links = capacities.Select(MapLink);
                links.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Python\transmission\{0}LINKS.txt", layout));
            }

        }

        private static LinkDataRow MapLink(KeyValuePair<string, double> pair)
        {
            return new LinkDataRow
            {
                LinkCapacity = pair.Value,
                CountryFrom = CountryInfo.GetShortAbbrev(pair.Key.Split('-')[0]),
                CountryTo = CountryInfo.GetShortAbbrev(pair.Key.Split('-')[1]),
                Type = Costs.LinkType[pair.Key]
            };
        }

        #endregion

        #endregion

        private static Dictionary<string, Dictionary<double, BetaWrapper>> CalcBetaCurves(List<double> kValues, double alphaStart, Func<NodeGenes[], double[]> evalX, Func<NodeGenes[], Dictionary<string, double>[]> evalY, string optPath = DefaultOptimumPath)
        {
            // Prepare the data structures.
            var alphaRes = 15;
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
            var betaGenes = new NodeGenes[betas.Length*(alphaRes+1)];
            var cfMaxGenes = new NodeGenes[betas.Length * (alphaRes+1)];
            for (int j = 0; j < betas.Length; j++)
            {
                for (int i = 0; i <= alphaRes; i++)
                {
                    betaGenes[i + j * (alphaRes+1)] = NodeGenesFactory.SpawnBeta(alphas[i], 1, betas[j]);
                    cfMaxGenes[i + j * (alphaRes + 1)] = NodeGenesFactory.SpawnCfMax(alphas[i], 1, kValues[j]);
                }
                optGenes[j] = FileUtils.FromJsonFile<NodeGenes>(
                    string.Format(optPath,
                        kValues[j]));
            }
            // Do evaluation.
            var optXValues = evalX(optGenes);            
            var optYValues = evalY(optGenes);
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
                var optXValue = optXValues[j];
                var optYValue = optYValues[j];
                foreach (var pair in optYValue)
                {
                    data[pair.Key][kValues[j]].GeneticX = optXValue;
                    data[pair.Key][kValues[j]].GeneticY = pair.Value;
                }

                Console.WriteLine("Beta {0} done",betas[j]);
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
            var allGenes = new NodeGenes[kValues.Count*geneMap.Keys.Count];
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
                Transmission = false
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
