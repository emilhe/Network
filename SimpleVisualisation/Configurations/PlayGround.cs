using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic;
using BusinessLogic.Cost;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.Utils;
using SimpleImporter;
using Utils;

namespace Main.Configurations
{
    class PlayGround
    {

        #region ShowTimeSeris

        public static void ShowTimeSeris(MainForm main)
        {
            var config = FileUtils.FromJsonFile<ModelYearConfig>(@"C:\Users\Emil\Dropbox\Master Thesis\OneYearAlpha0.5to1Gamma0.5to2Local.txt");
            var core = new ModelYearCore(config);
            //var param = new ParameterEvaluator(core);
            //var calc = new NodeCostCalculator(param);
            //var mCf = new NodeGenes(1, 1);
            //Console.WriteLine("Homo = " + calc.DetailedSystemCosts(mCf, true).ToDebugString());

            var ctrl = (core.BeController as SimulationController); // new SimulationController(); 
            //ctrl.InvalidateCache = true;
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("6h storage", input =>
            {
                var nodes = ConfigurationUtils.CreateNodes();
                ConfigurationUtils.SetupHomoStuff(nodes, 1, true, true, false);
                return nodes;
            });
            ctrl.LogFlows = true;
            ctrl.LogAllNodeProperties = true;
            //ctrl.ExportStrategies.Add(new ExportSchemeInput
            //{
            //    Scheme = ExportScheme.ConstrainedLocalized
            //});
            ctrl.Sources.Clear();
            ctrl.Sources.Add(new TsSourceInput { Length = 7, Offset = 0 });

            var param = new ParameterEvaluator(core);
            var calc = new NodeCostCalculator(param);
            var mCf = new NodeGenes(0.56, 1.03);
            Console.WriteLine("Homo = " + calc.DetailedSystemCosts(mCf, true).ToDebugString());
            
            var data = ctrl.EvaluateTs(0.56, 1.03);
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
            ctrl.ExportStrategies.Add(new ExportSchemeInput
            {
                Scheme = ExportScheme.ConstrainedLocalized,
            });


            main.DisplayTimeSeries().SetData(ctrl.EvaluateTs(1.029, 0.65).SelectMany(item => item.TimeSeries).ToList());
        }

        #endregion

        #region Compare stuff

        public static void CompareSources(MainForm main, bool save = false)
        {
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.50,
                MixingTo = 0.80,
                MixingSteps = 15,
                PenetrationFrom = 1.05,
                PenetrationTo = 1.10,
                //PenetrationFrom = 1.00,
                //PenetrationTo = 1.05,
                PenetrationSteps = 50
            };
            var ctrl = new SimulationController
            {
                InvalidateCache = true,
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput()
                    {
                        Scheme = ExportScheme.UnconstrainedSynchronized
                    },
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
            //ctrl.FailFuncs.Add("32 blackouts", () => new AllowBlackoutsStrategy(32));

            foreach (var result in ctrl.EvaluateGrid(grid)) view.AddData(grid.Rows, grid.Cols, result.Grid, result.Description);
        }

        public static void CompareFlows(MainForm main, bool save = false)
        {
            var ctrl = new SimulationController();
            ctrl.InvalidateCache = true;
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 1 });
            ctrl.ExportStrategies.Add(new ExportSchemeInput()
            {
                Scheme = ExportScheme.ConstrainedLocalized,
            });
            //ctrl.FailFuncs.Add("32 blackouts", () => new AllowBlackoutsStrategy(32));

            var data = ctrl.EvaluateTs(1.021, 0.62);

            //// Do statistics stuff.
            //var capacity =
            //    data[0]._mTimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
            //        .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
            //            flowTimeSeries => MathUtils.CalcCapacity(flowTimeSeries.GetAllValues().ToList()));
            //var capacityNoExt =
            //    data[1]._mTimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
            //        .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
            //            flowTimeSeries => MathUtils.CalcEmpCapacity(flowTimeSeries.GetAllValues().ToList()));

            //// View data.
            //var view = main.DisplayHistogram();
            //view.Setup(capacity.Keys.ToList());
            //view.AddData(capacity.Values.ToArray(), "NominalEnergy");
            //view.AddData(capacityNoExt.Values.ToArray(), "CapacityNoExt");

            var view = main.DisplayTimeSeries();
            view.SetData(data[0]);
        }

        #endregion

        #region Compare the different export schemes

        //public static void CompareExportSchemes(MainForm main, bool save = false)
        //{
        //    var view = main.DisplayContour();
        //    var grid = new GridScanParameters
        //    {
        //        MixingFrom = 0.45,
        //        MixingTo = 0.85,
        //        MixingSteps = 40,
        //        PenetrationFrom = 1.00,
        //        PenetrationTo = 1.75,
        //        PenetrationSteps = 100
        //    };
        //    var ctrl = new SimulationController
        //    {
        //        ExportStrategies = new List<ExportStrategyInput>
        //        {
        //            new ExportStrategyInput
        //            {
        //                DistributionStrategy = DistributionStrategy.SkipFlow,
        //                ExportScheme = ExportScheme.Cooperative
        //            },
        //            new ExportStrategyInput
        //            {
        //                DistributionStrategy = DistributionStrategy.SkipFlow,
        //                ExportScheme = ExportScheme.Selfish
        //            },
        //            new ExportStrategyInput
        //            {
        //                DistributionStrategy = DistributionStrategy.SkipFlow,
        //                ExportScheme = ExportScheme.None
        //            }
        //        },
        //        Sources = new List<TsSourceInput>
        //        {
        //            // Simulate 8 years; the ISET data are 8 years long. Offset VE data by 21; VE are 1979-2010 while ISET are 2000-2007.
        //            //new TsSourceInput {Source = TsSource.VE, Offset = 21, Length = 1},
        //            new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 1},
        //        }
        //    };

        //    var legends = new[] { "Cooperative", "Selfish", "No Export" };
        //    var data = ctrl.EvaluateGrid(grid);
        //    for (int index = 0; index < data.Count; index++)
        //    {
        //        var result = data[index];
        //        view.AddData(grid.Rows, grid.Cols, result.Grid, legends[index]);
        //    }

        //    //if (save) ChartUtils.SaveChart(view.MainChart, 800, 400, @"C:\Users\xXx\Dropbox\Master Thesis\Notes\Figures\ExportSchemes.png");
        //}

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
            //var opt = new MixOptimizer(nodes);
            //opt.OptimizeIndividually(0.05, 8);

            // Find out how good it is.
            var model = new NetworkModel(nodes, new UncSyncScheme(nodes, ConfigurationUtils.GetEuropeEdgeObject(nodes)));
            var simulation = new SimulationCore(model);
            //var mCtrl = new MixController(nodes);
            LineEvaluator.EvalSimulation(lineParams, simulation, 8);
        }

        public static SimulationCore Optimization(CountryNode[] nodes)
        {
            //var opt = new MixOptimizer(nodes);
            //opt.OptimizeIndividually();

            //opt.ReadMixCahce();
            //opt.OptimizeLocally();

            var model = new NetworkModel(nodes, new UncSyncScheme(nodes, ConfigurationUtils.GetEuropeEdgeObject(nodes)));
            var simulation = new SimulationCore(model);
            for (var pen = 1.02; pen <= 1.10; pen += 0.0025)
            {
                //mCtrl.SetPenetration(pen);
                //mCtrl.Execute();
                foreach (var node in simulation.Nodes)
                {
                    ((CountryNode)node).Model.Gamma = pen;
                }
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
                new ExportSchemeInput
                {
                    Scheme = ExportScheme.UnconstrainedSynchronized
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

        class MockSimulationController : ISimulationController
        {

            public List<SimulationOutput> Results { get; set; } 

            public bool CacheEnabled
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool InvalidateCache
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public List<SimulationOutput> EvaluateTs(NodeGenes genes)
            {
                return Results;
            }

            public List<SimulationOutput> EvaluateTs(double penetration, double mixing)
            {
                return Results;
            }

        }

    }
}
