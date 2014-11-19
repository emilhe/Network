using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;
using BusinessLogic.FailureStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Controls.Charting;
using SimpleImporter;
using Utils;
using Utils.Statistics;

namespace Main
{
    class Figures
    {

        #region DistributionMaps

        public static void DrawDistributions()
        {
            // Mean load distribution.
            var nodes = ConfigurationUtils.CreateNodes(TsSource.VE);
            var ctrl = new MixController(nodes);
            ctrl.SetMix(0.65);
            ctrl.SetPenetration(1.026);
            ctrl.Execute();

            var loadScaling = ConfigurationUtils.LoadScaling(nodes);
            var chart = EuropeChart.DrawEurope(loadScaling, Color.Black, Color.Yellow, Color.DarkGreen);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\MeanLoadDistribution.png");

            //// Mismatch distribution.
            //var scaling = ConfigurationUtils.MismatchScaling(nodes);
            //DeltaScaling(loadScaling, scaling);
            //chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            //chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\MeanMismatchDistribution.png");

            #region Realistic distributions

            var scaling = ConfigurationUtils.HeterogeneousBackupScaling(nodes);
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -20, 45);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\HeterogeneousBackupDistribution.png");

            scaling = ConfigurationUtils.HeterogeneousStorageScaling(nodes);
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -15, 85);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\HeterogeneousStorageDistribution.png");

            #endregion


            #region Optimal distributions

            // Optimal distribution [TESt].
            scaling = Parsing.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryHydrogenDelta.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryHydrogenDelta.png");

            // Optimal distribution.
            scaling = Parsing.DictionaryFromFile<string, double>(@"C:\proto\OptimalNoStorageNoLinks.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -8, 8);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalNoStorageNoLinksNorm.png");

            // Optimal distribution without links.
            scaling = Parsing.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryNoLinks.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -8, 8);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryNoLinksNorm.png");

            // Optimal distribution without links.
            scaling = Parsing.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryHydrogenNoLinks.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -8, 8);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryHydrogenNoLinksNorm.png");

            // Optimal distribution.
            scaling = Parsing.DictionaryFromFile<string, double>(@"C:\proto\OptimalNoStorage.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalNoStorageNorm.png");

            // Optimal distribution without links.
            scaling = Parsing.DictionaryFromFile<string, double>(@"C:\proto\OptimalBattery.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryNorm.png");

            // Optimal distribution without links.
            scaling = Parsing.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryHydrogen.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryHydrogenNorm.png");

            #endregion

        }

        private static void DeltaScaling(Dictionary<string, double> loadScaling, Dictionary<string, double> scaling)
        {
            var sum = scaling.Values.Sum();
            foreach (var key in loadScaling.Keys.ToArray())
            {
                if(scaling.ContainsKey(key)) scaling[key] = (scaling[key] / sum - loadScaling[key])* 100;
                else scaling.Add(key,-loadScaling[key] * 100);
            }
        }

        #endregion

        #region Backup analysis

        public static void BackupAnalysis(MainForm main)
        {
            // Set time zero.
            TimeManager.Instance().StartTime = new DateTime(1979, 1, 1);
            TimeManager.Instance().Interval = 60;
            // Create data.
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.ISET, Offset = 0, Length = 8 });
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.Cooperative,
                    DistributionStrategy = DistributionStrategy.SkipFlow
                });
            var data = ctrl.EvaluateTs(1.0225, 0.65);
            // Derive yearly relevant data.
            var derivedDataYearly = new List<ITimeSeries>
            {
                CreateTimeSeries(data[0], 8, "ISET", 21*8766, 8766),
                CreateTimeSeries(data[1], 32, "VE", 0, 8766)
            };
            var derivedDataDaily = new List<ITimeSeries>
            {
                CreateTimeSeries(data[0], 8, "ISET", 21*8766, 24),
                CreateTimeSeries(data[1], 32, "VE", 0, 24)
            };

            // Create full time series graphics.
            var fullTsView = new TimeSeriesView();
            fullTsView.AddData(derivedDataDaily[0]);
            fullTsView.AddData(derivedDataDaily[1]);
            fullTsView.MainChart.ChartAreas[0].AxisY.Title = "Consumption [TWh]";
            fullTsView.MainChart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy";
            foreach (var series in fullTsView.MainChart.Series)
            {
                series.BorderWidth = 2;
            }
            ChartUtils.SaveChart(fullTsView.MainChart, 1000, 500,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\FullBackupTs.png");

            // Create time series graphics.
            var tsView = new TimeSeriesView();
            tsView.AddData(derivedDataYearly[0]);
            tsView.AddData(derivedDataYearly[1]);
            tsView.MainChart.ChartAreas[0].AxisY.Title = "Consumption [TWh]";
            tsView.MainChart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy";

            foreach (var series in tsView.MainChart.Series)
            {
                series.BorderWidth = 5;
            }
            ChartUtils.SaveChart(tsView.MainChart, 1000, 500,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\BackupTs.png");

            // Create full bar chart graphics, NB: All zero values are filtered out!
            var fullHistView = new HistogramView();
            fullHistView.AddData(derivedDataDaily[0].GetAllValues().Where(item => item > 1e-12).ToList().ToDataBinTable(), derivedDataDaily[0].Name);
            fullHistView.AddData(derivedDataDaily[1].GetAllValues().Where(item => item > 1e-12).ToList().ToDataBinTable(), derivedDataDaily[1].Name);
            fullHistView.MainChart.ChartAreas[0].AxisX.Title = "Consumption [TWh]";
            ChartUtils.SaveChart(fullHistView.MainChart, 1000, 500,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\FullBackupHist.png");

            // Create bar chart graphics.
            var histView = new HistogramView();
            histView.AddData(derivedDataYearly[0].ToDataBinTable(new[] { 50, 75, 100, 125, 150, 175, 200, 225.0, 250 }), derivedDataYearly[0].Name);
            histView.AddData(derivedDataYearly[1].ToDataBinTable(new[] { 50, 75, 100, 125, 150, 175, 200, 225.0, 250 }), derivedDataYearly[1].Name);
            histView.MainChart.ChartAreas[0].AxisX.Title = "Consumption [TWh]";
            ChartUtils.SaveChart(histView.MainChart, 1000, 500,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\BackupHist.png");

        }

        private static SparseTimeSeries CreateTimeSeries(SimulationOutput sim, int length, string name, int offset, int avgInterval)
        {
            var idx = 1;
            var backup = new SparseTimeSeries(name);
            var backupTs =
                sim.TimeSeries.Where(item => item.Name.Contains("Hydro-bio backup"))
                    .Select(item => new IndexedSparseTimeSeries((SparseTimeSeries) item))
                    .ToArray();
            var lastSum = 150000.0*length;

            while (idx < length*(8766/avgInterval))
            {
                var sum = backupTs.Select(item => item.GetLastValue(idx * avgInterval)).Sum();
                var delta = lastSum - sum;
                lastSum = sum;
                backup.AddData(idx * avgInterval + offset, delta / 1000);
                // Go to next year;
                idx++;
            }

            return backup;
        }

        #endregion

        #region Flow analysis

        public static void FlowAnalysis(MainForm main)
        {
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.ConstrainedFlow
                });
            ctrl.NodeFuncs.Add("6h batt (homo), 25TWh hydrogen (homo), 150 TWh hydro (hetero)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                ConfigurationUtils.SetupHeterogeneousBackup(nodes, s.Length);
                return nodes;
            });
            ctrl.NodeFuncs.Add("6h batt (homo), 25TWh hydrogen (hetero), 150 TWh hydro (homo)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, false, true);
                ConfigurationUtils.SetupHeterogeneousStorage(nodes, s.Length);
                return nodes;
            });
            ctrl.NodeFuncs.Add("6h batt (homo), 25TWh hydrogen (hetero), 150 TWh hydro (hetero)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, false, false);
                ConfigurationUtils.SetupHeterogeneousStorage(nodes, s.Length);
                ConfigurationUtils.SetupHeterogeneousBackup(nodes, s.Length);
                return nodes;
            });
            var outputs = ctrl.EvaluateTs(1.026, 0.65);
            var view = main.DisplayHistogram();
           
            // Create reference histogram.
            var homo = Capacities(outputs[0]);
            view.Setup(homo.Keys.ToList());
            view.AddData(homo.Values.ToArray(), "Homogeneous");
            ChartUtils.SaveChart(view.MainChart, 1500, 750,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\Homogeneous.png");

            // Create reference histogram.
            homo = Capacities(outputs[0]);
            var hetBac = Capacities(outputs[1]);
            FilterValues(homo, hetBac);
            view.Setup(homo.Keys.ToList());
            view.AddData(homo.Values.ToArray(), "Homogeneous");
            view.AddData(hetBac.Values.ToArray(), "Heterogeneous Backup");
            ChartUtils.SaveChart(view.MainChart, 1000, 500,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\HeterogeneousBackup.png");

            // Create reference histogram.
            homo = Capacities(outputs[0]);
            var hetSto = Capacities(outputs[2]);
            FilterValues(homo, hetSto);
            view.Setup(homo.Keys.ToList());
            view.AddData(homo.Values.ToArray(), "Homogeneous");
            view.AddData(hetSto.Values.ToArray(), "Heterogeneous Storage");
            ChartUtils.SaveChart(view.MainChart, 1000, 500,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\HeterogeneousStorage.png");

            // Create reference histogram.
            homo = Capacities(outputs[0]);
            var het = Capacities(outputs[3]);
            FilterValues(homo, het);
            view.Setup(homo.Keys.ToList());
            view.AddData(homo.Values.ToArray(), "Homogeneous");
            view.AddData(het.Values.ToArray(), "Heterogeneous");
            ChartUtils.SaveChart(view.MainChart, 1000, 500,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\Heterogeneous.png");
        }

        public static void FlowAnalysisNext(MainForm main)
        {
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.ConstrainedFlow,
                });
            //ctrl.NodeFuncs.Clear();
            //ctrl.NodeFuncs.Add("2.2 TWh batt (delta), 25 TWh hydrogen (delta), 150 TWh hydro (delta)", s =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
            //    ConfigurationUtils.SetupStuff(nodes, s.Length, true, true, true, ConfigurationUtils.MismatchScaling(nodes));
            //    return nodes;
            //});
            ctrl.NodeFuncs.Add("6h batt (delta), 25 TWh hydrogen (delta), 150 TWh hydro (optimalDelta)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupStuff(nodes, s.Length, true, true, false, ConfigurationUtils.MismatchScaling(nodes));
                ConfigurationUtils.SetupOptimalBackupDelta(nodes, s.Length);
                return nodes;
            });
            var outputs = ctrl.EvaluateTs(1.026, 0.65);
            var view = main.DisplayHistogram();
            var labels = new[] { "Homo", "DeltaOptimal"};
            int idx;

            // Create histogram.
            idx = 0;
            foreach (var output in outputs)
            {
                var data = Capacities(output);
                if (idx == 0) view.Setup(data.Keys.ToList());
                view.AddData(data.Values.ToArray(), string.Format("{0}, sum = {1}", labels[idx], data.Values.Sum().ToString("0.0")));
                idx++;
            }
            ChartUtils.SaveChart(view.MainChart, 1500, 750,
                @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\Percentile.png");

            // Create histogram.
            idx = 0;
            foreach (var output in outputs)
            {
                var data = FullCapacities(output);
                if (idx == 0) view.Setup(data.Keys.ToList());
                view.AddData(data.Values.ToArray(), string.Format("{0}, sum = {1}", labels[idx], data.Values.Sum().ToString("0.0")));
                idx++;
            }
            ChartUtils.SaveChart(view.MainChart, 1500, 750,
                @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\FullCapacity.png");
        }

        public static void FlowAnalysisNoStorage(MainForm main)
        {
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.ConstrainedFlow
                });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("6h batt (homo), 150 TWh hydro (homo)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, (int)s.Length, true, false, true);
                ConfigurationUtils.SetupHeterogeneousStorage(nodes, (int)s.Length);
                return nodes;
            });
            ctrl.NodeFuncs.Add("6h batt (homo), 150 TWh hydro (hetero)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, (int)s.Length, true, false, false);
                ConfigurationUtils.SetupHeterogeneousBackup(nodes, (int)s.Length);
                return nodes;
            });
            var outputs = ctrl.EvaluateTs(1.126, 0.65);
            var view = main.DisplayHistogram();

            // Create reference histogram.
            var homo = Capacities(outputs[0]);
            view.Setup(homo.Keys.ToList());
            view.AddData(homo.Values.ToArray(), "Homogeneous");
            ChartUtils.SaveChart(view.MainChart, 1500, 750,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\HomogeneousNoStorage.png");

            // Create reference histogram.
            var het = Capacities(outputs[1]);
            FilterValues(homo, het);
            view.Setup(homo.Keys.ToList());
            view.AddData(homo.Values.ToArray(), "Homogeneous");
            view.AddData(het.Values.ToArray(), "Heterogeneous Backup");
            ChartUtils.SaveChart(view.MainChart, 1000, 500,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\HeterogeneousNoStorage.png");
        }

        private static Dictionary<string, double> Capacities(SimulationOutput output)
        {
            return output.TimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
                        .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
                            flowTimeSeries => StatUtils.CalcCapacity(flowTimeSeries.GetAllValues()));
        }

        private static Dictionary<string, double> FullCapacities(SimulationOutput output)
        {
            return output.TimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
                        .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
                            flowTimeSeries => StatUtils.CalcFullCapacity(flowTimeSeries.GetAllValues()));
        }

        private static void FilterValues(Dictionary<string, double> val1, Dictionary<string, double> val2, double tolerance = 2)
        {
            foreach (var key in val1.Keys.ToArray())
            {
                if(Math.Abs(val1[key] - val2[key]) > tolerance) continue;
                val1.Remove(key);
                val2.Remove(key);
            }            
        }

        #endregion

        #region Constrained flow analysis

        public static void ConstrainedFlowAnalysisCache(MainForm main)
        {
            // What flow fractions should be investigated?
            var fractions = new[] { 1, 0.75, 0.5, 0.4, 0.39, 0.38, 0.37, 0.36, 0.35, 0.34, 0.33, 0.32, 0.31 };
            var penetrations = new[] { 1.026, 1.026, 1.026, 1.026, 1.026, 1.026, 1.026, 1.026, 1.026, 1.026, 1.046, 1.106, 1.321 };

            var view = main.DisplayPlot();
            view.AddData(fractions.Zip(penetrations, (x, y) => new {x, y }).ToDictionary(item => item.x, item => item.y), "α = 0.65");

            // Prepare chart printing.
            view.MainChart.Series[0].BorderWidth = 4;
            view.MainChart.Series[1].MarkerSize = 8;
            view.MainChart.ChartAreas[0].AxisY.Minimum = 0.9;
            view.MainChart.ChartAreas[0].AxisX.Title = "Normalised link capacity";
            view.MainChart.ChartAreas[0].AxisY.Title = "Penetration, γ";
            ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\FlowAnalysis.png");
        }

        public static void Comparison(MainForm main)
        {
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.ConstrainedFlow
                });
            var outputs = ctrl.EvaluateTs(1.026, 0.65);
            var view = main.DisplayHistogram();

            // Create reference histogram.
            var homo = Capacities(outputs[0]);
            var homo2 = outputs[0].TimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
                        .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
                            flowTimeSeries => StatUtils.CalcEmpCapacity(flowTimeSeries.GetAllValues()));
            view.Setup(homo.Keys.ToList());
            view.AddData(homo.Values.ToArray(), "Homogeneous, 0.5/99.5 percentile");
            view.AddData(homo2.Values.ToArray(), "Homogeneous, 33% maximum");
            ChartUtils.SaveChart(view.MainChart, 1500, 750,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\HomogeneousComp.png");
        }

        public static void ConstrainedFlowAnalysis(MainForm main)
        {
            CalculateFlowData();

            // What flow fractions should be investigated?
            //var fractions = new[] { 1, 0.75, 0.5, 0.4, 0.3, 0.29, 0.28, 0.27, 0.26, 0.25, 0.24, 0.23, 0.22, 0.21, 0.20 };
            var fractions = new[] { 1, 0.30, 0.29, 0.28, 0.27, 0.26, 0.25, 0.24, 0.23, 0.22, 0.21, 0.20, 0.15, 0.10 };
            //var fractions = new[] { 0.31 };
            //var originalMix = 0.65;
            //var originalPen = 1.026;      
            var originalMix = 0.62;
            var originalPen = 1.021;
            //var originalMix = 0.60;
            //var originalPen = 1.06;

            // Prepare the setup.
            var dataPoints = new Dictionary<double, double>();
            var grid = new GridScanParameters
            {
                StartFromMin = true,
                // Fix the mix.
                MixingFrom = originalMix,
                MixingTo = originalMix+0.01,
                MixingSteps = 1,
                // Vary the penetration.
                PenetrationFrom = originalPen,
                PenetrationTo = originalPen + 0.20,
                PenetrationSteps = 10
            };
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.ConstrainedFlow
                });
            ctrl.FailFuncs = new Dictionary<string, Func<IFailureStrategy>>();
            ctrl.FailFuncs.Add("Allow blackouts", () => new AllowBlackoutsStrategy(100));

            // Do the crunching.
            var fracIdx = 0;
            while (true)
            {
                if (fracIdx == fractions.Length) break;

                var fraction = fractions[fracIdx];
                // Update edge functions.
                ctrl.EdgeFuncs.Clear();
                ctrl.EdgeFuncs.Add(string.Format("Europe edges, constrained {0}%", fraction * 100), list => ConfigurationUtils.GetEdges(list, "flowTest", fraction)); // OK
                // Evaluate.    
                var result = ctrl.EvaluateGrid(grid)[0].Grid;
                // Find last success.
                var idx = result.GetLength(0)-1;
                while (result[idx, 0])
                {
                    idx--;
                    if (idx < 0) break;
                }
                if (idx == result.GetLength(0) - 1) break;
                idx++;
                // Save and prepare next iteration.
                dataPoints.Add(fraction, grid.Cols[idx]);   
                grid.PenetrationFrom = grid.Cols[idx];
                grid.PenetrationSteps = result.GetLength(0)-idx;
                fracIdx++;
            }

            var hest = 2;

            var view = main.DisplayPlot();
            view.AddData(dataPoints, "Real");
            view.MainChart.ChartAreas[0].AxisY.Minimum = 1;

            //ctrl.EdgeFuncs.Add("Europe edges, constrained 100%", list => ConfigurationUtils.GetEdges(list, "flowTest", 1.00)); // OK
            //ctrl.EdgeFuncs.Add("Europe edges, constrained 75%", list => ConfigurationUtils.GetEdges(list, "flowTest", 0.75)); // OK
            //ctrl.EdgeFuncs.Add("Europe edges, constrained 50%", list => ConfigurationUtils.GetEdges(list, "flowTest", 0.50)); // FAIL
            //ctrl.EdgeFuncs.Add("Europe edges, constrained 40%", list => ConfigurationUtils.GetEdges(list, "flowTest", 0.40)); // FAIL
            //ctrl.EdgeFuncs.Add("Europe edges, constrained 30%", list => ConfigurationUtils.GetEdges(list, "flowTest", 0.30)); // FAIL
            //ctrl.EdgeFuncs.Add("Europe edges, constrained 20%", list => ConfigurationUtils.GetEdges(list, "flowTest", 0.20)); // FAIL
            //ctrl.EdgeFuncs.Add("Europe edges, constrained 30%", list => ConfigurationUtils.GetEdges(list, "flowTest", 0.30)); // FAIL
            //ctrl.EdgeFuncs.Add("Europe edges, constrained 25%", list => ConfigurationUtils.GetEdges(list, "flowTest", 0.25)); // FAIL
            //var outputs = ctrl.EvaluateTs(1.067, 0.60);

            //var grid = new GridScanParameters
            //{
            //    MixingFrom = 0.60,
            //    MixingTo = 0.61,
            //    MixingSteps = 1,
            //    PenetrationFrom = 1.05,
            //    PenetrationTo = 1.25,
            //    PenetrationSteps = 40
            //};

            //var result = ctrl.EvaluateGrid(grid);
            //var view = main.DisplayContour();
            //view.AddData(grid.Rows, grid.Cols, result[0].Grid, "10%");
            //view.AddData(grid.Rows, grid.Cols, result[1].Grid, "75%");
            //view.AddData(grid.Rows, grid.Cols, result[2].Grid, "50%");
            //view.AddData(grid.Rows, grid.Cols, result[3].Grid, "40%");
            //view.AddData(grid.Rows, grid.Cols, result[4].Grid, "30%");
            //view.AddData(grid.Rows, grid.Cols, result[5].Grid, "20%");
            //view.AddData(grid.Rows, grid.Cols, result[6].Grid, "10%");

            //// Prepare chart printing.
            //view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            //view.MainChart.ChartAreas[0].AxisY.Title = "Mixing, α";
            //view.MainChart.ChartAreas[0].AxisY.Minimum = 0.59;
            //view.MainChart.ChartAreas[0].AxisY.Maximum = 0.61;
            //ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\ConstrainedFlow.png");
        }

        private static void CalculateFlowData()
        {
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.ConstrainedFlow
                });
            //var outputs = ctrl.EvaluateTs(1.026, 0.65);
            var outputs = ctrl.EvaluateTs(1.021, 0.62);

            // Create reference histogram.
            var flows = new List<LinkDataRow>();
            foreach (var pair in FullCapacities(outputs[0]))
            {
                var countries = pair.Key.Split(new[] { "\r\n" }, StringSplitOptions.None);
                flows.Add(new LinkDataRow
                {
                    CountryFrom = CountryInfo.GetName(countries[0]),
                    CountryTo = CountryInfo.GetName(countries[1]),
                    LinkCapacity = pair.Value
                });
            }

            ProtoStore.SaveLinkData(flows, "flow32Year"); //flowReal
        }

        //private static Dictionary<string, double> FullCapacities(SimulationOutput output)
        //{
        //    return output._mTimeSeries.Where(item => item.Properties.ContainsKey("Flow"))
        //        .ToDictionary(flowTimeSeries => flowTimeSeries.Name,
        //            flowTimeSeries =>
        //                Math.Max(Math.Abs(flowTimeSeries.GetAllValues().Min()), flowTimeSeries.GetAllValues().Max()));
        //}

        #endregion

        public static void CompareData(MainForm main)
        {
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 40,
                PenetrationFrom = 1.00,
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
                    new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 8},
                    new TsSourceInput {Source = TsSource.VE, Offset = 21, Length = 8},
                }
            };

            var data = ctrl.EvaluateGrid(grid);
            view.AddData(grid.Rows, grid.Cols, data[0].Grid, "ISET");
            view.AddData(grid.Rows, grid.Cols, data[1].Grid, "VE");

            // Prepare chart printing.
            view.MainChart.ChartAreas[0].AxisX.Maximum = 1.12;
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            view.MainChart.ChartAreas[0].AxisY.Title = "Mixing, α";
            ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\CompareSources.png");
        }

        public static void CompareExportSchemes(MainForm main)
        {
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 40,
                PenetrationFrom = 1.00,
                PenetrationTo = 1.70,
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
                    new TsSourceInput {Source = TsSource.VE, Offset = 0, Length = 32},
                }
            };

            var data = ctrl.EvaluateGrid(grid);
            view.AddData(grid.Rows, grid.Cols, data[0].Grid, "Cooperative");
            view.AddData(grid.Rows, grid.Cols, data[1].Grid, "Selfish");
            view.AddData(grid.Rows, grid.Cols, data[2].Grid, "None");

            // Prepare chart printing.
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            view.MainChart.ChartAreas[0].AxisY.Title = "Mixing, α";
            ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\ExportSchemes.png");
        }

    }
}
