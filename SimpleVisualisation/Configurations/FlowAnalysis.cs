using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;
using BusinessLogic.Simulation;
using BusinessLogic.Utils;
using SimpleImporter;
using Utils;
using Utils.Statistics;
using TsSourceInput = BusinessLogic.Simulation.TsSourceInput;

namespace Main.Configurations
{
    class FlowAnalysis
    {

        #region Constrained Flow, no storage

        public static void ConstrainedFlowNoStorage(MainForm main, bool save = false)
        {
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.50,
                MixingTo = 0.55,
                MixingSteps = 5,
                PenetrationFrom = 1.45,
                PenetrationTo = 1.50,
                PenetrationSteps = 5
            };
            var ctrl = new SimulationController
            {
                InvalidateCache = true,
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput{Scheme = ExportScheme.None}
                },
                Sources = new List<TsSourceInput>
                {
                    new TsSourceInput {Source = TsSource.VE, Offset = 0, Length = 32},
                }
            };
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("No storage", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, (int)s.Length, true, true, true);
                return nodes;
            });

            foreach (var result in ctrl.EvaluateGrid(grid)) view.AddData(grid.Rows, grid.Cols, result.Grid, result.Description);
        }

        #endregion

        #region Flow stuff

        public static void CompareHydroFlows(MainForm main, bool reCalculate = false, bool save = true)
        {
            var ctrl = new SimulationController();
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.ISET, Offset = 0, Length = 1 });
            ctrl.ExportStrategies.Add(new ExportSchemeInput()
            {
                Scheme = ExportScheme.ConstrainedLocalized
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
                        flowTimeSeries => MathUtils.CalcCapacity(flowTimeSeries.GetAllValues().ToList()));
            var capacityHetro =
                data[1].TimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
                    .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
                        flowTimeSeries => MathUtils.CalcCapacity(flowTimeSeries.GetAllValues().ToList()));

            // View data.
            var view = main.DisplayHistogram();
            view.Setup(capacityHomo.Keys.ToList());
            view.AddData(capacityHomo.Values.ToArray(), "Homogeneous");
            view.AddData(capacityHetro.Values.ToArray(), "Heterogeneous");

            //ChartUtils.SaveChart(view.MainChart, 1500, 750,
            //    @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\Flowz.png");
        }

        #endregion

        #region Flow

        public static void FlowStuff(MainForm main, bool reCalculate = false)
        {
            var ctrl = new SimulationController { InvalidateCache = true };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.ISET, Offset = 0, Length = 1 });
            //ctrl.ExportStrategies.Add(
            //    new ExportStrategyInput
            //    {
            //        ExportScheme = ExportScheme.Cooperative,
            //        DistributionStrategy = DistributionStrategy.MinimalFlow
            //    });
            ctrl.ExportStrategies.Add(
                new ExportSchemeInput()
                {
                    Scheme = ExportScheme.ConstrainedLocalized
                });
            ctrl.NodeFuncs.Add("6h batt (homo), 25TWh hydrogen (homo), 150 TWh hydro (hetero)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, (int)s.Length, true, true, false);
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
                            flowTimeSeries => MathUtils.CalcCapacity(flowTimeSeries.GetAllValues()));
                //var maxVals =
                //    output._mTimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
                //        .Select(flowTimeSeries => flowTimeSeries.GetAllValues().Select(item => Math.Abs(item)).Max())
                //        .ToArray();

                if (idx == 0) view.Setup(capacities.Keys.ToList());

                var key = output.Properties["NodeTag"];
                var exp = ((ExportScheme)byte.Parse(output.Properties["ExportScheme"])).GetDescription();
                view.AddData(capacities.Values.ToArray(), key + " : " + exp);
                //view.AddData(maxVals,key + " : " + exp + "@MAX");

                idx++;
            }

            //ChartUtils.SaveChart(view.MainChart, 1500, 750,
            //    @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\Flowz.png");
        }

        #endregion

    }
}
