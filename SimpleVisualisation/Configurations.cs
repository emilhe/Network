using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic.TimeSeries;
using Controls.Charting;
using SimpleImporter;
using BusinessLogic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Utils;
using Utils;
using Utils.Statistics;
using Utils = BusinessLogic.Utils.Utils;

namespace Main
{
    class Configurations
    {

        #region Contour control test

        public static void TestContourControl(MainForm main)
        {
            var gridParams = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 8,
                PenetrationFrom = 1.00,
                PenetrationTo = 1.15,
                PenetrationSteps = 4
            };

            var grid = new[,]
            {
                {false, false, false, true, true, false, false , false},
                {false, false, true, true, true, true, false , false},
                {false, true, true, true, true, true, true , false},
                {true, true, true, true, true, true, true , true},
            };

            var view = main.DisplayContour();
            view.AddData(gridParams.Rows, gridParams.Cols, grid, "TEST Data");            
        }

        #endregion

        #region Contour control test

        public static void TestPlotControl(MainForm main)
        {
            var view = main.DisplayPlot();
            var data = new Dictionary<double, double>();
            data.Add(1,2);
            data.Add(3, 4);
            data.Add(7, 5);
            view.AddData(data, "Test");
        }

        #endregion

        #region PerformanceTest

        public static void OneYearVE(MainForm main)
        {
            var ctrl = new SimulationController { InvalidateCache = true };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.ISET, Offset = 0, Length = 1 });
            ctrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.ConstrainedFlow,
            });

            var data = ctrl.EvaluateTs(1.029, 0.65);
        }

        #endregion

        #region ShowTimeSeris

        public static void ShowTimeSeris(MainForm main)
        {
            var ctrl = new SimulationController {InvalidateCache = false};
            ctrl.Sources.Add(new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 1});
            ctrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.Cooperative,
                DistributionStrategy = DistributionStrategy.MinimalFlow
            });
            ctrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.ConstrainedFlow
            });

            var data = ctrl.EvaluateTs(1.029, 0.65);

            foreach (var item in data)
            {
                foreach (var ts in item.TimeSeries)
                {
                    ts.Properties.Add("ExportStrategy", ((ExportStrategy)byte.Parse(item.Properties["ExportStrategy"])).GetDescription());
                    ts.DisplayProperties.Add("ExportStrategy");
                }
            }

            main.DisplayTimeSeries().SetData(data.SelectMany(item => item.TimeSeries).ToList());
        }

        public static void ShowEcnTimeSeris(MainForm main)
        {
            var ctrl = new SimulationController();
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.ISET, Offset = 0, Length = 1 });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("Ecn data", (input) =>
            {
                var nodes = ConfigurationUtils.CreateNodes(input.Source);
                ConfigurationUtils.SetupNodesFromEcnData(nodes, ProtoStore.LoadEcnData());
                return nodes;
            });
            ctrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.Cooperative,
                DistributionStrategy = DistributionStrategy.SkipFlow
            });

            main.DisplayTimeSeries().SetData(ctrl.EvaluateTs(1.029, 0.65).SelectMany(item => item.TimeSeries).ToList());
        }

        #endregion

        #region Use ECN data; with and without 6 hour storage (so far, not important)

        //public static void TryEcnData(MainForm main, bool save = false)
        //{
        //    var ecnData = ProtoStore.LoadEcnData();
        //    var withBattery = ConfigurationUtils.CreateNodes(TsSource.ISET, true);
        //    var withoutBattery = ConfigurationUtils.CreateNodes(TsSource.ISET);
        //    ConfigurationUtils.SetupNodesFromEcnData(withBattery, ecnData);
        //    ConfigurationUtils.SetupNodesFromEcnData(withoutBattery, ecnData);

        //    var gridParams = new GridScanParameters
        //    {
        //        MixingFrom = 0.45,
        //        MixingTo = 0.85,
        //        MixingSteps = 40,
        //        PenetrationFrom = 0.80,
        //        PenetrationTo = 1.10,
        //        PenetrationSteps = 100
        //    };

        //    var view = main.DisplayContour();
        //    view.AddData(gridParams.Rows, gridParams.Columns, RunSimulation(gridParams, withBattery), "Incl. 6-hour battery");
        //    view.AddData(gridParams.Rows, gridParams.Columns, RunSimulation(gridParams, withoutBattery), "Excl. 6-hour battery");

        //    if(save) ChartUtils.SaveChart(view.MainChart, 800, 400, @"C:\Users\xXx\Dropbox\Master Thesis\Notes\Figures\ECN.png");
        //}

        //private static bool[,] RunSimulation(GridScanParameters gridParams, List<Node> nodes)
        //{
        //    return RunSimulation(gridParams, new CooperativeExportStrategy(new SkipFlowStrategy()), nodes);
        //}

        #endregion

        #region Compare the ISET and the VE data

        public static void CompareSources(MainForm main, bool save = false)
        {
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.60,
                MixingTo = 0.70,
                MixingSteps = 10,
                PenetrationFrom = 1.05,
                PenetrationTo = 1.10,
                PenetrationSteps = 50
            };
            var ctrl = new SimulationController
            {
                ExportStrategies = new List<ExportStrategyInput>
                {
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.Cooperative
                    }
                },
                Sources = new List<TsSourceInput>
                {
                    // Simulate 8 years; the ISET data are 8 years long. Offset VE data by 21; VE are 1979-2010 while ISET are 2000-2007.
                    //new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 8},
                    //new TsSourceInput {Source = TsSource.VE, Offset = 21, Length = 8},
                                        //new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 8},
                    new TsSourceInput {Source = TsSource.VE, Offset = 0, Length = 1},
                }
            };

            foreach (var result in ctrl.EvaluateGrid(grid)) view.AddData(grid.Rows, grid.Cols, result.Grid, result.Description);            
        }

        #endregion

        #region Compare the different export schemes

        public static void CompareExportSchemes(MainForm main, bool save = false)
        {
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 40,
                PenetrationFrom = 1.00,
                PenetrationTo = 1.75,
                PenetrationSteps = 100
            };
            var ctrl = new SimulationController
            {
                ExportStrategies = new List<ExportStrategyInput>
                {
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.Cooperative
                    },
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.Selfish
                    },
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.None
                    }
                },
                Sources = new List<TsSourceInput>
                {
                    // Simulate 8 years; the ISET data are 8 years long. Offset VE data by 21; VE are 1979-2010 while ISET are 2000-2007.
                    //new TsSourceInput {Source = TsSource.VE, Offset = 21, Length = 1},
                    new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 1},
                }
            };

            var legends = new[] {"Cooperative", "Selfish", "No Export"};
            var data = ctrl.EvaluateGrid(grid);
            for (int index = 0; index < data.Count; index++)
            {
                var result = data[index];
                view.AddData(grid.Rows, grid.Cols, result.Grid, legends[index]);
            }

            //if (save) ChartUtils.SaveChart(view.MainChart, 800, 400, @"C:\Users\xXx\Dropbox\Master Thesis\Notes\Figures\ExportSchemes.png");
        }

        #endregion

        #region Flow

        public static void FlowStuff(MainForm main, bool reCalculate = false)
        {
            var ctrl = new SimulationController {InvalidateCache = true};
            ctrl.Sources.Add(new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 1});
            //ctrl.ExportStrategies.Add(
            //    new ExportStrategyInput
            //    {
            //        ExportStrategy = ExportStrategy.Cooperative,
            //        DistributionStrategy = DistributionStrategy.MinimalFlow
            //    });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.ConstrainedFlow
                });
            ctrl.NodeFuncs.Add("6h batt (homo), 25TWh hydrogen (homo), 150 TWh hydro (hetero)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, (int) s.Length, true, true, false);
                ConfigurationUtils.SetupHeterogeneousBackup(nodes, (int)s.Length);
                return nodes;
            });
            ctrl.NodeFuncs.Add("6h batt (homo), 25TWh hydrogen (hetero), 150 TWh hydro (homo)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, (int)s.Length, true, false, true);
                ConfigurationUtils.SetupHeterogeneousStorage(nodes, (int)s.Length);
                return nodes;
            });
            ctrl.NodeFuncs.Add("6h batt (homo), 25TWh hydrogen (hetero), 150 TWh hydro (hetero)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, (int)s.Length, true, false, false);
                ConfigurationUtils.SetupHeterogeneousStorage(nodes, (int)s.Length);
                ConfigurationUtils.SetupHeterogeneousBackup(nodes, (int)s.Length);
                return nodes;
            });
            var outputs = ctrl.EvaluateTs(1.029, 0.65);
            
            var view = main.DisplayHistogram();
            int idx = 0;

            foreach (var output in outputs)
            {
                var capacities =
                    output.TimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
                        .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
                            flowTimeSeries => StatUtils.CalcCapacity(flowTimeSeries.GetAllValues()));
                //var maxVals =
                //    output.TimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
                //        .Select(flowTimeSeries => flowTimeSeries.GetAllValues().Select(item => Math.Abs(item)).Max())
                //        .ToArray();

                if (idx == 0) view.Setup(capacities.Keys.ToList());

                var key = output.Properties["NodeTag"];
                var exp = ((ExportStrategy) byte.Parse(output.Properties["ExportStrategy"])).GetDescription();
                view.AddData(capacities.Values.ToArray(), key + " : " + exp );
                //view.AddData(maxVals,key + " : " + exp + "@MAX");

                idx++;
            }

            //ChartUtils.SaveChart(view.MainChart, 1500, 750,
            //    @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\Flowz.png");
        }

        #endregion

        #region Compare average and yearly backup

        public static void CompareAverageAndYearlyBackupVE(MainForm main, bool reCalculate = false, bool save = true)
        {
            var keys = new[] { "Average backup = 150TWh", "Yearly (best)", "Yearly (worst)" };
            //if (!reCalculate && LoadAndViewGridResults(main, keys)) { return; }

            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 40,
                PenetrationFrom = 0.925,
                PenetrationTo = 1.15,
                PenetrationSteps = 100
            };
            var ctrl = new SimulationController
            {
                ExportStrategies = new List<ExportStrategyInput>
                {
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.Cooperative
                    }
                },
                Sources = new List<TsSourceInput>
                {
                    // Simulate 8 years; the ISET data are 8 years long. Offset VE data by 21; VE are 1979-2010 while ISET are 2000-2007.
                    //new TsSourceInput {Source = TsSource.VE, Offset = 19, Length = 1, Description = "Yearly (best), VE"},
                    //new TsSourceInput {Source = TsSource.VE, Offset = 16, Length = 1, Description = "Yearly (worst), VE"},
                    new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 8, Description = "Avg. backup = 150TWh, VE"},
                }
            };

            var results = ctrl.EvaluateGrid(grid);
            for (int index = 0; index < results.Count; index++)
            {
                var result = results[index];
                view.AddData(grid.Rows, grid.Cols, result.Grid, keys[index]);
            }
        }

        public static void CompareAverageAndYearlyBackupISET(MainForm main, bool reCalculate = false, bool save = true)
        {
            var keys = new[] { "Avg. backup = 150TWh ISET", "Yearly (best) ISET", "Yearly (worst) ISET" };
            if (!reCalculate && LoadAndViewGridResults(main, keys)) { return; }
            
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 40,
                PenetrationFrom = 0.925,
                PenetrationTo = 1.15,
                PenetrationSteps = 100
            };
            var ctrl = new SimulationController
            {
                ExportStrategies = new List<ExportStrategyInput>
                {
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.Cooperative
                    }
                },
                Sources = new List<TsSourceInput>
                {
                    // Simulate 8 years; the ISET data are 8 years long. Offset VE data by 21; VE are 1979-2010 while ISET are 2000-2007.
                    new TsSourceInput {Source = TsSource.ISET, Offset = 6, Length = 1},
                    new TsSourceInput {Source = TsSource.ISET, Offset = 7, Length = 1},
                    new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 8},
                }
            };

            var results = ctrl.EvaluateGrid(grid);
            for (int index = 0; index < results.Count; index++)
            {
                var result = results[index];
                view.AddData(grid.Rows, grid.Cols, result.Grid, keys[index]);
            }
        }


        #endregion

        #region Flow stuff

        public static void CompareHydroFlows(MainForm main, bool reCalculate = false, bool save = true)
        {
            var ctrl = new SimulationController();
            ctrl.Sources.Add(new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 1});
            ctrl.ExportStrategies.Add(new ExportStrategyInput
            {
                ExportStrategy = ExportStrategy.Cooperative,
                DistributionStrategy = DistributionStrategy.MinimalFlow
            });
            ctrl.NodeFuncs.Add("6h batt (homogeneous), 25TWh hydrogen (homogeneous), 150 TWh hydro-bio (heterogeneous)", s =>
                {
                    var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                    ConfigurationUtils.SetupHomoStuff(nodes, (int)s.Length, true, true, false);
                    ConfigurationUtils.SetupHeterogeneousBackup(nodes, (int)s.Length);
                    return nodes;
                });
            var data = ctrl.EvaluateTs(1.029, 0.65);

            // Do statistics stuff.
            var capacityHomo =
                data[0].TimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
                    .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
                        flowTimeSeries => StatUtils.CalcCapacity(flowTimeSeries.GetAllValues().ToList()));
            var capacityHetro =
                data[1].TimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
                    .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
                        flowTimeSeries => StatUtils.CalcCapacity(flowTimeSeries.GetAllValues().ToList()));

            // View data.
            var view = main.DisplayHistogram();
            view.Setup(capacityHomo.Keys.ToList());
            view.AddData(capacityHomo.Values.ToArray(), "Homogeneous");
            view.AddData(capacityHetro.Values.ToArray(), "Heterogeneous");

            //ChartUtils.SaveChart(view.MainChart, 1500, 750,
            //    @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\Flowz.png");
        }

        #endregion

        #region Optimization

        public static void Optimize(MainForm main)
        {
            var lineParams = new LineScanParameters
            {
                PenetrationFrom = 1.00,
                PenetrationTo = 1.15,
                PenetrationSteps = 100
            };

            // Find optimum.
            var nodes = ConfigurationUtils.CreateNodesWithBackup(TsSource.ISET, 8);
            var opt = new MixOptimizer(nodes);
            opt.OptimizeIndividually(0.05, 8);

            // Find out how good it is.
            var model = new NetworkModel(nodes, new CooperativeExportStrategy(new SkipFlowStrategy()));
            var simulation = new Simulation(model);
            var mCtrl = new MixController(nodes);
            LineEvaluator.EvalSimulation(lineParams, simulation, mCtrl, 8);
        }

        public static Simulation Optimization(List<Node> nodes, MixController mCtrl)
        {
            var opt = new MixOptimizer(nodes);
            opt.OptimizeIndividually();

            //opt.ReadMixCahce();
            //opt.OptimizeLocally();

            var model = new NetworkModel(nodes, new CooperativeExportStrategy(new SkipFlowStrategy()));
            var simulation = new Simulation(model);
            for (var pen = 1.02; pen <= 1.10; pen += 0.0025)
            {
                mCtrl.SetPenetration(pen);
                mCtrl.Execute();
                simulation.Simulate(8766);
                Console.WriteLine("Penetation " + pen + ", " + (simulation.Output.Success ? "SUCCESS" : "FAIL"));
            }
            return simulation;
        }


        #endregion

        #region NoStorage

        public static void NoStoragePlot(MainForm main)
        {
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.60,
                MixingTo = 0.70,
                MixingSteps = 10,
                PenetrationFrom = 1.10,
                PenetrationTo = 1.20,
                PenetrationSteps = 50
            };
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.Cooperative,
                    DistributionStrategy = DistributionStrategy.SkipFlow
                });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("6h batt (homo), 150 TWh hydro (homo)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, (int)s.Length, true, false, true);
                return nodes;
            });
            var outputs = ctrl.EvaluateGrid(grid);
            outputs[0].Description = "6h batt (homo), 150 TWh hydro (homo)";

            foreach (var result in outputs) view.AddData(grid.Rows, grid.Cols, result.Grid, result.Description);            
        }

        #endregion

        #region Help methods

        private static bool LoadAndViewTsResults(MainForm main, string[] keys)
        {
            var res = AccessClient.LoadSimulationOutput(keys[0]);
            if (res == null) return false;

            var view = main.DisplayTimeSeries();
            view.SetData(res.TimeSeries);

            return true;
        }

        private static bool LoadAndViewGridResults(MainForm main, string[] keys)
        {
            var res = ProtoStore.LoadGridResult(keys[0]);
            if (res == null) return false;

            var labels = new[] { "Average backup = 150TWh", "Yearly (best)", "Yearly (worst)" };

            var view = main.DisplayContour();
            //for (int i = 0; i < keys.Length; i++)
            //{
            //    var data = ProtoStore.LoadGridResult(keys[i]);
            //    view.AddData(data.Rows, data.Columns, (bool[,])data.Grid.ToArray(), labels[i]);
            //}

            var data = ProtoStore.LoadGridResult(keys[0]);
            view.AddData(data.Rows, data.Columns, (bool[,])data.Grid.ToArray(), labels[0]);
            data = ProtoStore.LoadGridResult(keys[2]);
            view.AddData(data.Rows, data.Columns, (bool[,])data.Grid.ToArray(), labels[1]);
            data = ProtoStore.LoadGridResult(keys[1]);
            view.AddData(data.Rows, data.Columns, (bool[,])data.Grid.ToArray(), labels[2]);

            //ChartUtils.SaveChart(view.MainChart, 800, 400, @"C:\Users\xXx\Dropbox\Master Thesis\Notes\Figures\AvgVsYearlyISET.png");

            return true;
        }

        #endregion

    }
}
