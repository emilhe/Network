using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using BusinessLogic.Cost;
using BusinessLogic.Cost.CostModels;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Nodes;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Controls;
using SimpleImporter;
using Utils;
using Utils.Statistics;

namespace Main.Configurations
{
    class StorageAnalysis
    {

        public static void ExportStorageOverview(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator() { CacheEnabled = false, Full = true };
            // No storage
            var proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data0hSyncProj.txt");
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
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data5hSyncProj.txt");
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
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data35hSyncProj.txt");
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
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data5h35hSyncProj.txt");
            Console.WriteLine("35h + 5h storage done.");

            //data.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data10hStorageSync.txt");
        }

        public static void ExportStrategyOverview(List<double> kValues)
        {
            ParallelNodeCostCalculator costCalc;
            Dictionary<string, Dictionary<double, BetaWrapper>> proj;
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
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data5h35hStrat1.txt");
            Console.WriteLine("35h + 5h storage done.");
            // 35h + 5h storage: Power
            UncSyncScheme.Power = 1;
            costCalc = new ParallelNodeCostCalculator() { CacheEnabled = false };
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                return nodes;
            }, "5h35h storage sync"))) { CacheEnabled = false };
            proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data5h35hStrat2.txt");
            Console.WriteLine("35h + 5h storage done.");
            // 35h + 5h storage: Bias
            UncSyncScheme.Power = 0;
            UncSyncScheme.Bias = 0.24;
            costCalc = new ParallelNodeCostCalculator() { CacheEnabled = false };
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                return nodes;
            }, "5h35h storage sync"))) { CacheEnabled = false };
            proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
            proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data5h35hStrat3.txt");
            Console.WriteLine("35h + 5h storage done.");
        }

        public static void ExportStrategyPower(List<double> kValues)
        {
            ParallelNodeCostCalculator costCalc;
            Dictionary<string, Dictionary<double, BetaWrapper>> proj;

            UncSyncScheme.Bias = 0;
            var n = 5;
            for (int i = 0; i < n; i++)
            {
                var pow = i * 0.5;
                UncSyncScheme.Power = pow;
                costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
                costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
                {
                    var nodes = ConfigurationUtils.CreateNodesNew();
                    ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                    return nodes;
                }, "real storage v1.0 w5h"))) { CacheEnabled = false };
                // Storage
                proj = CalcBetaCurves(new List<double> { 1 }, 0.0,
                    genes => genes.Select(item => item.Alpha).ToArray(),
                    genes =>
                        costCalc.ParallelEval(genes,
                            (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
                proj.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data5h35hPower{0}.txt", pow));
                var min = proj["BC"].Values.Select(item => item.BetaY).Min().Min();
                Console.WriteLine("Power {0} with min {1}.", UncSyncScheme.Power, min);
            }
        }

        public static void ExportBias(List<double> kValues)
        {
            ParallelNodeCostCalculator costCalc;
            Dictionary<string, Dictionary<double, BetaWrapper>> proj;

            UncSyncScheme.Power = 0;
            var biasVec = new double[] { 0.245 };
            for (int i = 0; i < biasVec.Length; i++)
            {
                var bias = biasVec[i];
                UncSyncScheme.Bias = bias;
                costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
                costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
                {
                    var nodes = ConfigurationUtils.CreateNodesNew();
                    ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                    return nodes;
                }, "real storage v1.0 w5h"))) { CacheEnabled = false };
                // Storage
                proj = CalcBetaCurves(new List<double> { 1 }, 0.0,
                    genes => genes.Select(item => item.Alpha).ToArray(),
                    genes =>
                        costCalc.ParallelEval(genes,
                            (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
                proj.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data5h35hBias{0}.txt", bias));
                var min = proj["BC"].Values.Select(item => item.BetaY).Min().Min();
                Console.WriteLine("Bias {0} with min {1}.", UncSyncScheme.Bias, min);
            }
        }

        public static void ExportStorageReal(List<double> kValues)
        {
            UncSyncScheme.Bias = 0;
            UncSyncScheme.Power = 0;
            // Default strategy.
            Eval("drSyncVEhydroBio.txt", () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                return nodes;
            });
            // Power strategy.
            UncSyncScheme.Bias = 0;
            UncSyncScheme.Power = 0.5;
            Eval("drSyncVEhydroBioPow.txt", () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                return nodes;
            });
            // Bias strategy.
            UncSyncScheme.Bias = 0.19;
            UncSyncScheme.Power = 0;
            Eval("drSyncVEhydroBioBias.txt", () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                return nodes;
            });
        }

        public static void RealDifferentLevelsOfHydroBio()
        {
            UncSyncScheme.Bias = 0;
            UncSyncScheme.Power = 0;
            // None.
            Eval("drSyncVEref.txt", () =>
            {
                    var nodes = ConfigurationUtils.CreateNodesNew();
                //ConfigurationUtils.SetupRealHydro(nodes);
                //ConfigurationUtils.SetupRealBiomass(nodes);
                    return nodes;
            });
            // Hydro excl. pump.
            Eval("drSyncVEhydroNoPump.txt", () =>
                {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes, false);
                //ConfigurationUtils.SetupRealBiomass(nodes);
                return nodes;
            });
            // Hydro incl. pump
            Eval("drSyncVEhydro.txt", () =>
        {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                return nodes;
            });
            // Hydro incl. pump
            Eval("drSyncVEhydroBio.txt", () =>
                {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                return nodes;
            });
                }

        public static void RealDifferentLevelsOfExtraStorage()
        {
            UncSyncScheme.Bias = 0;
            UncSyncScheme.Power = 0;
            // None.
            Eval("drSyncVEhydroBio.txt", () =>
                {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                //ConfigurationUtils.SetupHomoStuff(nodes, 32, false, false, false);
                return nodes;
            });
            // 5h.
            Eval("drSyncVEhydroBio5h.txt", () =>
                    {
                        var nodes = ConfigurationUtils.CreateNodesNew();
                        ConfigurationUtils.SetupRealHydro(nodes);
                        ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            });
            // 35h.
            Eval("drSyncVEhydroBio35h.txt", () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                        ConfigurationUtils.SetupHomoStuff(nodes, 32, false, true, false);
                        return nodes;
            });
            // 5h+35h.
            Eval("drSyncVEhydroBio5h35h.txt", () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                return nodes;
            });
                }

        public static void RealDifferentLeveldOfExtraStoargeBiased()
        {
            UncSyncScheme.Bias = 19;
            UncSyncScheme.Power = 0;
            // None.
            Eval("drSyncVEhydroBioBias.txt", () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                //ConfigurationUtils.SetupHomoStuff(nodes, 32, false, false, false);
                return nodes;
            });
            UncSyncScheme.Bias = 0.14;
            // 5h.
            Eval("drSyncVEhydroBioBias5h.txt", () =>
                                    {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
                                    });
            UncSyncScheme.Bias = 0.13;
            // 35h.
            Eval("drSyncVEhydroBioBias35h.txt", () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, false, true, false);
                                return nodes;
            });
            UncSyncScheme.Bias = 0.12;
            // 5h+35h.
            Eval("drSyncVEhydroBioBias5h35h.txt", () =>
                {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                return nodes;
            });
        }

        public static void StorageCost()
        {
            var scenarios = new Dictionary<string, Action<CountryNode[]>>
            {
                {"Reference", nodes => { }},
                {"5h", nodes => ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false)},
                {"35h", nodes => ConfigurationUtils.SetupHomoStuff(nodes, 32, false, true, false)},
                {"5h+35h", nodes => ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false)}

            };
            var bias = new Dictionary<string, double>
            {
                {"Reference", 0.19},
                {"5h", 0.14},
                {"35h", 0.13},
                {"5h+35h", 0.12}
            };
            var noBias = DoMagic(scenarios, bias, false);
            var withbias = DoMagic(scenarios, bias, true);
            noBias.ToJsonFile(Path.Combine(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\storage_cost", "noBias.txt"));
            withbias.ToJsonFile(Path.Combine(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\storage_cost",
                "withBias.txt"));
        }

        public static void StorageOptimization()
        {
            var scenarios = new Dictionary<string, Action<CountryNode[]>>
            {
                {"Reference", nodes => { }},
                {"5h", nodes => ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false)},
                {"35h", nodes => ConfigurationUtils.SetupHomoStuff(nodes, 32, false, true, false)},
                {"5h+35h", nodes => ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false)}

            };
            var bias = new Dictionary<string, double>
            {
                {"Reference", 0.19},
                {"5h", 0.14},
                {"35h", 0.13},
                {"5h+35h", 0.12}
            };

            // Storage optimizations
            for (var k = 1; k < 4; k++)
            {
                foreach (var scenario in scenarios)
                {
                    UncSyncScheme.Bias = 0;
                    var costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = false };
                    costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(1, () =>
                    {
                        var nodes = ConfigurationUtils.CreateNodesNew();
                        ConfigurationUtils.SetupRealHydro(nodes);
                        ConfigurationUtils.SetupRealBiomass(nodes);
                        scenario.Value(nodes);
                        return nodes;
                    }, scenario.Key))) { CacheEnabled = false };
                    Optimization.Sequential(k, scenario.Key + "@unbiased", costCalc);
                    costCalc.ResetEvals();
                    UncSyncScheme.Bias = bias[scenario.Key];
                    Optimization.Sequential(k, scenario.Key + "@biased", costCalc);
                }
            }
        }

        public static void StorageTransmission()
        {
            var genes = NodeGenesFactory.SpawnCfMax(0, 1, 1);
            // hydro and bio
            var evaluator = new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                //ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            }, "stuff")) { CacheEnabled = false };
            var capacities = evaluator.LinkCapacities(genes);
            var links = capacities.Select(Figures.PlayGround.MapLink);
            links.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\transmission\HydroBioLINKS.txt"));
            // 5h + 35h
            evaluator = new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                return nodes;
            }, "stuff")) { CacheEnabled = false };  
            capacities = evaluator.LinkCapacities(genes);
            links = capacities.Select(Figures.PlayGround.MapLink);
            links.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\transmission\HydroBio5h35hLINKS.txt"));
        }

        // NOTE: Requires manual adjustment
        public static void OptimizeStuff()
        {
            double[] powVec;
            double[] biasVec;
            KeyValuePair<double, double[]> opt;

            // No storage.
            powVec = new[] { 0.0 };
            biasVec = new[] { 0.19 };
            EvalOpt(powVec, biasVec, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                //ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            });
            // 5h.
            powVec = new[] { 0.0 };
            biasVec = new[] { 0.14 };
            opt = EvalOpt(powVec, biasVec, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            });
            Console.WriteLine("Optimum for 5h is bias = {0}", opt.Value[1]);
            // 35h.
            powVec = new[] { 0.0 };
            biasVec = new[] { 0.13 };
            opt = EvalOpt(powVec, biasVec, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, false, true, false);
                return nodes;
            });
            Console.WriteLine("Optimum for 35h is bias = {0}", opt.Value[1]);
            // 5h35h.
            powVec = new[] { 0.0 };
            biasVec = new[] { 0.12 };
            opt = EvalOpt(powVec, biasVec, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
                return nodes;
            });
            Console.WriteLine("Optimum for 5h35h is bias = {0}", opt.Value[1]);
        }

        #region Helper methods

        private static void Eval(string path, Func<CountryNode[]> nodeFunc, List<double> kValues = null)
        {
            var basePath = @"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\";
            if (kValues == null) kValues = new List<double>() { 1 };
            var costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, nodeFunc, "stuff"))) { CacheEnabled = false };
            // Storage
            var proj = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes =>
                    costCalc.ParallelEval(genes,
                        (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
            proj.ToJsonFile(Path.Combine(basePath, path));
            Console.WriteLine("Real(ly?) done.");
        }

        private static KeyValuePair<double, double[]> EvalOpt(double[] powVec, double[] biasVec, Func<CountryNode[]> nodeFunc)
        {
            var opt = new KeyValuePair<double, double[]>(double.MaxValue, new double[0]);
            for (int i = 0; i < powVec.Length; i++)
            {
                for (int j = 0; j < biasVec.Length; j++)
                {
                    UncSyncScheme.Power = powVec[i];
                    UncSyncScheme.Bias = biasVec[j];
                    var costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
                    costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, nodeFunc, "stuff"))) { CacheEnabled = false };
                    // Storage
                    var proj = CalcBetaCurves(new List<double> { 1 }, 0.0,
                        genes => genes.Select(item => item.Alpha).ToArray(),
                        genes =>
                            costCalc.ParallelEval(genes,
                                (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
                    // Dump data?
                    //proj.ToJsonFile(
                    //    string.Format(
                    //        @"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\drSyncVEhydroBioBias5h{0}.txt",
                    //        UncSyncScheme.Bias, UncSyncScheme.Power));
                    //proj.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\optBias{0}Power{1}.txt", UncSyncScheme.Bias, UncSyncScheme.Power));
                    var min = proj["BC"].Values.Select(item => item.BetaY).Min().Min();
                    if (min < opt.Key)
                    {
                        opt = new KeyValuePair<double, double[]>(min, new[] { UncSyncScheme.Power, UncSyncScheme.Bias });
                    }
                    Console.WriteLine("Bias {0} & Power {1} => Min = {2}.", UncSyncScheme.Bias, UncSyncScheme.Power, min);
                }
            }
            return opt;
        }

        private static Dictionary<string, double[]> DoMagic(Dictionary<string, Action<CountryNode[]>> scenarios, Dictionary<string, double> biasVec, bool bias)
        {
            const int res = 10;
            var alphas = new double[res + 1].Linspace(0, 1);
            var genes = alphas.Select(item => new NodeGenes(item, 1));
            var results = new Dictionary<string, double[]>();
            results.Add("alpha", alphas);
            ParallelNodeCostCalculator costCalc;
            foreach (var scenario in scenarios)
            {
                UncSyncScheme.Power = 0;
                UncSyncScheme.Bias = bias ? biasVec[scenario.Key] : 0;
                costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
                costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
                {
                    var nodes = ConfigurationUtils.CreateNodesNew();
                    ConfigurationUtils.SetupRealHydro(nodes);
                    ConfigurationUtils.SetupRealBiomass(nodes);
                    scenario.Value(nodes);
                    return nodes;
                }, scenario.Key))) { CacheEnabled = false };
                results.Add(scenario.Key, costCalc.ParallelEval(genes.ToList(), (c, g) => c.SystemCost(g)));
                Console.WriteLine(scenario.Key + " is done " + (bias ? "with" : "without") + " bias");
            }
            return results;
        }

        private static Dictionary<string, Dictionary<double, BetaWrapper>> CalcBetaCurves(List<double> kValues, double alphaStart, Func<NodeGenes[], double[]> evalX, Func<NodeGenes[], Dictionary<string, double>[]> evalY) //, string optPath = DefaultOptimumPath, bool skipPoints = false)
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

        #endregion

        #region Unused

        public static void ExportParameterOverviewData5hStorage(List<double> kValues)
        {
            var costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false };
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
                return nodes;
            }, "5h storage con sync", ExportScheme.ConstrainedSynchronized))) { CacheEnabled = false };
            var opt = CalcBetaCurves(kValues, 0.0,
                genes => genes.Select(item => item.Alpha).ToArray(),
                genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
            opt.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data5hSyncOpt.txt");
            Console.WriteLine("Optimization strategy 1 done");

            //data.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data10hStorageSync.txt");
        }


        public static void ExportStorageReal(List<double> kValues)
        {
            //ParallelNodeCostCalculator costCalc;
            //Dictionary<string, Dictionary<double, BetaWrapper>> proj;

            #region Original

            //costCalc = new ParallelNodeCostCalculator(1) { CacheEnabled = false, Full = true };
            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(1, () =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodesNew();
            //    return nodes;
            //}, "no storage"))) { CacheEnabled = false };
            //// No storage
            //proj = CalcBetaCurves(kValues, 0.0,
            //    genes => genes.Select(item => item.Alpha).ToArray(),
            //    genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            //proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data0hSyncProj1.txt");
            //Console.WriteLine("Real(ly?) done.");

            //costCalc = new ParallelNodeCostCalculator(1) { CacheEnabled = false, Full = true };
            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(1, () =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodesNew();
            //    ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
            //    return nodes;
            //}, "5h storage"))) { CacheEnabled = false };
            //// Storage
            //proj = CalcBetaCurves(kValues, 0.0,
            //    genes => genes.Select(item => item.Alpha).ToArray(),
            //    genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            //proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\data0hSyncProj15h.txt");
            //Console.WriteLine("Real(ly?) done.");

            //costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodesNew();
            //    ConfigurationUtils.SetupRealHydro(nodes);
            //    return nodes;
            //}, "real storage v1.0"))) { CacheEnabled = false };
            //// Storage
            //proj = CalcBetaCurves(kValues, 0.0,
            //    genes => genes.Select(item => item.Alpha).ToArray(),
            //    genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            //proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\dataRealSyncProj.txt");
            //Console.WriteLine("Real(ly?) done.");

            //costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodesNew();
            //    ConfigurationUtils.SetupRealHydro(nodes);
            //    ConfigurationUtils.SetupHomoStuff(nodes, 32, true, false, false);
            //    return nodes;
            //}, "real storage v1.0 w5h"))) { CacheEnabled = false };
            //// Storage
            //proj = CalcBetaCurves(kValues, 0.0,
            //    genes => genes.Select(item => item.Alpha).ToArray(),
            //    genes => costCalc.ParallelEval(genes, (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            //proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\dataRealSyncProj5h.txt");
            //Console.WriteLine("Real(ly?) done.");

            #endregion

            #region Compare VE/ISET

            //costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodesNew();
            //    //ConfigurationUtils.SetupRealHydro(nodes);
            //    //ConfigurationUtils.SetupRealBiomass(nodes);
            //    return nodes;
            //}, "real storage v1.0 w5h"))) { CacheEnabled = false };
            //// Storage
            //proj = CalcBetaCurves(kValues, 0.0,
            //    genes => genes.Select(item => item.Alpha).ToArray(),
            //    genes =>
            //        costCalc.ParallelEval(genes,
            //            (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            //proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\drSyncVE.txt");
            //Console.WriteLine("Real(ly?) done.");

            //costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(8, () =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodes();
            //    //ConfigurationUtils.SetupRealHydro(nodes);
            //    //ConfigurationUtils.SetupRealBiomass(nodes);
            //    return nodes;
            //}, "real storage v1.0 w5h"))) { CacheEnabled = false };
            //// Storage
            //proj = CalcBetaCurves(kValues, 0.0,
            //    genes => genes.Select(item => item.Alpha).ToArray(),
            //    genes =>
            //        costCalc.ParallelEval(genes,
            //            (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            //proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\drSyncISET.txt");
            //Console.WriteLine("Real(ly?) done.");

            //costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodesNew();
            //    ConfigurationUtils.SetupRealHydro(nodes);
            //    ConfigurationUtils.SetupRealBiomass(nodes);
            //    return nodes;
            //}, "real storage v1.0 w5h"))) { CacheEnabled = false };
            //// Storage
            //proj = CalcBetaCurves(kValues, 0.0,
            //    genes => genes.Select(item => item.Alpha).ToArray(),
            //    genes =>
            //        costCalc.ParallelEval(genes,
            //            (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            //proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\drSyncVEall.txt");
            //Console.WriteLine("Real(ly?) done.");

            //costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(8, () =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodes();
            //    ConfigurationUtils.SetupRealHydro(nodes);
            //    ConfigurationUtils.SetupRealBiomass(nodes);
            //    return nodes;
            //}, "real storage v1.0 w5h"))) { CacheEnabled = false };
            //// Storage
            //proj = CalcBetaCurves(kValues, 0.0,
            //    genes => genes.Select(item => item.Alpha).ToArray(),
            //    genes =>
            //        costCalc.ParallelEval(genes,
            //            (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
            //proj.ToJsonFile(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\drSyncISETall.txt");
            //Console.WriteLine("Real(ly?) done.");

            #endregion

        }

        //public static void OptimizeStuff()
        //{
        //    ParallelNodeCostCalculator costCalc;
        //    Dictionary<string, Dictionary<double, BetaWrapper>> proj;

        //    var powVec = new[] { 0 };
        //    //var biasVec = new[] { 0 };
        //    var biasVec = new[] { 0, 0.25 };

        //    for (int i = 0; i < powVec.Length; i++)
        //        for (int j = 0; j < biasVec.Length; j++)
        //        {
        //            UncSyncScheme.Power = powVec[i];
        //            UncSyncScheme.Bias = biasVec[j];
        //            //costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
        //            //costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
        //            //{
        //            //    var nodes = ConfigurationUtils.CreateNodesNew();
        //            //    ConfigurationUtils.SetupRealHydro(nodes);
        //            //    ConfigurationUtils.SetupRealBiomass(nodes);
        //            //    ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
        //            //    return nodes;
        //            //}, "real storage v1.0 w5h"))) { CacheEnabled = false };
        //            //// Storage
        //            //proj = CalcBetaCurves(new List<double> { 1 }, 0.0,
        //            //    genes => genes.Select(item => item.Alpha).ToArray(),
        //            //    genes =>
        //            //        costCalc.ParallelEval(genes,
        //            //            (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
        //            //proj.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\drSyncVEhydroBioBias35h5h{0}.txt", UncSyncScheme.Bias, UncSyncScheme.Power));
        //            ////proj.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\optBias{0}Power{1}.txt", UncSyncScheme.Bias, UncSyncScheme.Power));
        //            //var min = proj["BC"].Values.Select(item => item.BetaY).Min().Min();
        //            //Console.WriteLine("Bias {0} & Power {1} => Min = {2}.", UncSyncScheme.Bias, UncSyncScheme.Power, min);
        //            // Dump data
        //            var core = new FullCore(32, () =>
        //            {
        //                var nodes = ConfigurationUtils.CreateNodesNew();
        //                ConfigurationUtils.SetupRealHydro(nodes);
        //                ConfigurationUtils.SetupRealBiomass(nodes);
        //                ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
        //                return nodes;
        //            }, "real storage v1.0 w5h" + UncSyncScheme.Bias);
        //            core.Controller.CacheEnabled = false;
        //            var data = core.Controller.EvaluateTs(1, 0.7);
        //            var balancingTs = data[0].TimeSeries.Where(item => item.Name.Contains("Balancing")).ToArray();
        //            var t = balancingTs[0].Count();
        //            var pos = new double[t];
        //            var neg = new double[t];
        //            for (int k = 0; k < t; k++)
        //            {
        //                var val = balancingTs.Select(item => item.GetValue(k)).Sum();
        //                if (val < 0) neg[k] += val;
        //                else pos[k] += val;
        //                //foreach (var ts in balancingTs)
        //                //{
        //                //    var val = ts.GetValue(k);
        //                //    if (val < 0) neg[k] += val;
        //                //    else pos[k] += val;
        //                //}
        //            }
        //            //pos.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_prod\storage_ts\curtailmentBias{0}.txt", UncSyncScheme.Bias));
        //            neg.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_prod\storage_ts\backupGenerationBias{0}.txt", UncSyncScheme.Bias));
        //        }

        //}

        //public static void OptimizeStuff3()
        //{
        //    ParallelNodeCostCalculator costCalc;
        //    Dictionary<string, Dictionary<double, BetaWrapper>> proj;

        //    var powVec = new[] { 0 };
        //    //var biasVec = new[] { 0 };
        //    var biasVec = new[] { 0.16, 0.15 };

        //    for (int i = 0; i < powVec.Length; i++)
        //        for (int j = 0; j < biasVec.Length; j++)
        //        {
        //            UncSyncScheme.Power = powVec[i];
        //            UncSyncScheme.Bias = biasVec[j];
        //            costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
        //            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(32, () =>
        //            {
        //                var nodes = ConfigurationUtils.CreateNodesNew();
        //                ConfigurationUtils.SetupRealHydro(nodes);
        //                ConfigurationUtils.SetupRealBiomass(nodes);
        //                ConfigurationUtils.SetupHomoStuff(nodes, 32, false, true, false);
        //                return nodes;
        //            }, "real storage v1.0 w5h"))) { CacheEnabled = false };
        //            // Storage
        //            proj = CalcBetaCurves(new List<double> { 1 }, 0.0,
        //                genes => genes.Select(item => item.Alpha).ToArray(),
        //                genes =>
        //                    costCalc.ParallelEval(genes,
        //                        (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes)));
        //            proj.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\drSyncVEhydroBioBias35h{0}.txt", UncSyncScheme.Bias, UncSyncScheme.Power));
        //            //proj.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\optBias{0}Power{1}.txt", UncSyncScheme.Bias, UncSyncScheme.Power));
        //            var min = proj["BC"].Values.Select(item => item.BetaY).Min().Min();
        //            Console.WriteLine("Bias {0} & Power {1} => Min = {2}.", UncSyncScheme.Bias, UncSyncScheme.Power, min);
        //            //// Dump data
        //            //var core = new FullCore(32, () =>
        //            //{
        //            //    var nodes = ConfigurationUtils.CreateNodesNew();
        //            //    ConfigurationUtils.SetupRealHydro(nodes);
        //            //    ConfigurationUtils.SetupRealBiomass(nodes);
        //            //    //ConfigurationUtils.SetupHomoStuff(nodes, 32, true, true, false);
        //            //    return nodes;
        //            //}, "real storage v1.0 w5h");
        //            //core.Controller.CacheEnabled = false;
        //            //var data = core.Controller.EvaluateTs(1, 0.9);
        //            //var balancingTs = data[0].TimeSeries.Where(item => item.Name.Contains("Balancing")).ToArray();
        //            //var t = balancingTs[0].Count();
        //            //var pos = new double[t];
        //            //var neg = new double[t];
        //            //for (int k = 0; k < t; k++)
        //            //{
        //            //    var val = balancingTs.Select(item => item.GetValue(k)).Sum();
        //            //    if (val < 0) neg[k] += val;
        //            //    else pos[k] += val;
        //            //    //foreach (var ts in balancingTs)
        //            //    //{
        //            //    //    var val = ts.GetValue(k);
        //            //    //    if (val < 0) neg[k] += val;
        //            //    //    else pos[k] += val;
        //            //    //}
        //            //}
        //            //pos.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\ts\realCurtailmentBias{0}.txt", UncSyncScheme.Bias));
        //            //neg.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\ts\realBackupGenerationBias{0}.txt", UncSyncScheme.Bias));
        //        }

        //}

        //public static void Groenne()
        //{
        //    ParallelNodeCostCalculator costCalc;
        //    Dictionary<string, Dictionary<double, BetaWrapper>> proj;
        //    costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
        //    costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(8, () =>
        //    {
        //        return ConfigurationUtils.CreateNodes(TsSource.ISET);
        //    }, "ISET tmp"))) { CacheEnabled = false };
        //    // Storage
        //    proj = CalcBetaCurves(new List<double> { 1, 3}, 0.0,
        //        genes => genes.Select(item => item.Alpha).ToArray(),
        //        genes =>
        //            costCalc.ParallelEval(genes,
        //                (calculator, nodeGenes) => calculator.ParameterOverview(nodeGenes, true)));
        //    proj.ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\ISETk={0}.txt", k));
        //}

        //public static void Groenne2(int beta)
        //{
        //    var scenarios = new Dictionary<string, Func<CountryNode, NodeGene, double>>
        //    {
        //        {"Localized (fixed K^B)", (node, gene) => node.Model.AvgLoad},
        //        {"Power=1", (node, gene) => Math.Pow(gene.Gamma,1) * node.Model.AvgLoad},
        //        {"Power=-1", (node, gene) => Math.Pow(gene.Gamma,-1) * node.Model.AvgLoad},
        //        {"Eta=3", (node, gene) => (1 + 3 * Math.Max(gene.Gamma-1, 0)) * node.Model.AvgLoad},
        //        {"invEta=3", (node, gene) => (1 + 3 * Math.Max(1-gene.Gamma, 0)) * node.Model.AvgLoad},
        //    };
        //    // Setup data structures.
        //    const int res = 10;
        //    var alphas = new double[res + 1].Linspace(0, 1);
        //    var genes = alphas.Select(item => NodeGenesFactory.SpawnBeta(item, 1, beta)).ToArray();
        //    var results = new Dictionary<string, double[]>();
        //    var linkResults = new Dictionary<string, Dictionary<string,double>[]>();
        //    results.Add("alpha", alphas);
        //    // Determine the backup capacity.
        //    var costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
        //    costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(8, ConfigurationUtils.CreateNodes))) { CacheEnabled = false };
        //    var e = costCalc.ParallelEval(genes.ToList(), (c, g) => c.ParameterOverview(g));
        //    results.Add("Synchronized-tc", e.Select(item => item["TC"]).ToArray());
        //    results.Add("Synchronized-bc", e.Select(item => item["BC"]).ToArray());
        //    results.Add("Synchronized-be", e.Select(item => item["BE"]).ToArray());
        //    linkResults.Add("Synchronized", costCalc.ParallelEval(genes.ToList(), (c, g) => c.Evaluator.LinkCapacities(g)));
        //    var avgLoad = ConfigurationUtils.CreateNodes().Select(item => item.Model.AvgLoad).Sum();
        //    var bcs = results["Synchronized-bc"].Copy().Mult(avgLoad);
        //    var bes = results["Synchronized-be"];
        //    // Temporary test stuff.
        //    costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = true };
        //    costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(8, ConfigurationUtils.CreateNodes, "", ExportScheme.ConstrainedLocalized))) { CacheEnabled = false };
        //    //e = costCalc.ParallelEval(genes.ToList(), (c, g) => c.ParameterOverview(g));
        //    //results.Add("Localized-tc", e.Select(item => item["TC"]).ToArray());
        //    //results.Add("Localized-bc", e.Select(item => item["BC"]).ToArray());
        //    //results.Add("Localized-be", e.Select(item => item["BE"]).ToArray());
        //    linkResults.Add("Localized", costCalc.ParallelEval(genes.ToList(), (c, g) => c.Evaluator.LinkCapacities(g)));         
        //    // Do stuff.
        //    foreach (var scenario in scenarios)
        //    {
        //        var data = new double[alphas.Length];
        //        var bcData = new double[alphas.Length];
        //        var beData = new double[alphas.Length];
        //        var linkData = new Dictionary<string, double>[alphas.Length];
        //        try
        //        {
        //            for (int i = 0; i < alphas.Length; i++)
        //            {
        //                // Calculate TC.
        //                costCalc = new ParallelNodeCostCalculator(4) {CacheEnabled = false, Full = true};
        //                costCalc.SpawnCostCalc =
        //                    () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(8, () =>
        //                    {
        //                        var nodes = ConfigurationUtils.CreateNodes(TsSource.ISET);
        //                        // Assign proper backup facilities.
        //                        var sum = nodes.Select(item => scenario.Value(item, genes[i][item.Name])).Sum();
        //                        foreach (var node in nodes)
        //                        {
        //                            node.Storages.Add(new BasicBackup("kb", 1e9)
        //                            {
        //                                Capacity = scenario.Value(node, genes[i][node.Name])/sum*bcs[i]
        //                            });
        //                        }
        //                        return nodes;
        //                    }, scenario.Key, ExportScheme.ConstrainedLocalized))) {CacheEnabled = false};
        //                // Append data.

        //                //var eval = costCalc.ParallelEval(new List<NodeGenes> {genes[i]},
        //                //    (c, g) => c.ParameterOverview(g));
        //                //data[i] = eval[0]["TC"];
        //                //bcData[i] = eval[0]["BC"] + bcs[i]/avgLoad;
        //                //beData[i] = eval[0]["BE"] + bes[i];
        //                linkData[i] = costCalc.ParallelEval(new List<NodeGenes> { genes[i] },
        //                    (c, g) => c.Evaluator.LinkCapacities(g))[0];
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Evaluation failed: " + scenario.Key);
        //        }
        //        //results.Add(scenario.Key + "-tc", data);
        //        //results.Add(scenario.Key + "-bc", bcData);
        //        //results.Add(scenario.Key + "-be", beData);
        //        linkResults.Add(scenario.Key, linkData);
        //    }
        //    //results.ToJsonFile(Path.Combine(@"C:\Users\Emil\Dropbox\BACKUP\Python\groenne", string.Format("newRes-beta-all{0}.txt",beta)));
        //    linkResults.ToJsonFile(Path.Combine(@"C:\Users\Emil\Dropbox\BACKUP\Python\groenne", string.Format("newRes-beta-links{0}.txt", beta)));
        //}

        #endregion

    }
}
