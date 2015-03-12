using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using BusinessLogic;
using BusinessLogic.Interfaces;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Controls.Charting;
using SimpleImporter;
using Utils;

namespace Main.Configurations
{
    public class BackupAnalysis
    {

        #region Backup energy

        public static void BackupEnergyRelative(MainForm main)
        {
            var alpha = 0.8;
            var penetrations = new[] { 1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.75, 2.0, 2.25, 2.5 };
            var ctrl = new SimulationController
            {
                InvalidateCache = false,
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput{Scheme = ExportScheme.UnconstrainedSynchronized}
                },
                Sources = new List<TsSourceInput>
                {
                    new TsSourceInput {Source = TsSource.VE, Offset = 0, Length = 32},
                    new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 8},
                }
            };

            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("Mega storage", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            var results = DoBackupStuff(ctrl, penetrations, alpha);
            var view = main.DisplayPlot();
            foreach (var result in results) view.AddData(penetrations, result.Value, result.Key, false);
            // Setup view.
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            view.MainChart.ChartAreas[0].AxisY.Title = "Backup energy [load]";

            foreach (var series in view.MainChart.Series)
            {
                series.BorderWidth = 3;
            }
            ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\BackupEnergyRelative.png");
        }

        public static void BackupEnergyAbsolute(MainForm main)
        {
            var alpha = 0.8;
            var penetrations = new[] { 1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.8, 2, 2.25, 2.50, 2.75, 3, 3.25, 3.5, 4, 4.5, 5, 6, 7, 8, 9, 10 };
            var ctrlFlow = new SimulationController
            {
                InvalidateCache = false,
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput(){Scheme = ExportScheme.UnconstrainedSynchronized}
                },
                Sources = new List<TsSourceInput> { new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 }, }
            };
            ctrlFlow.NodeFuncs.Clear();
            ctrlFlow.NodeFuncs.Add("Mega storage, no storage", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            var ctrlNoFlow = new SimulationController
            {
                InvalidateCache = false,
                ExportStrategies = new List<ExportSchemeInput> { new ExportSchemeInput() { Scheme = ExportScheme.None }, },
                Sources = new List<TsSourceInput> { new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 }, }
            };
            ctrlNoFlow.NodeFuncs.Clear();
            ctrlNoFlow.NodeFuncs.Add("Mega storage, no storage", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });

            var flowResults = DoBackupStuff(ctrlNoFlow, penetrations, alpha, false, true);
            var noFlowResults = DoBackupStuff(ctrlFlow, penetrations, alpha, false);
            var view = main.DisplayPlot();
            foreach (var result in flowResults) view.AddData(penetrations, result.Value, result.Key, false);
            foreach (var result in noFlowResults) view.AddData(penetrations, result.Value, result.Key, false);
            var backup = new Series("150 TWh");
            backup.Points.AddXY(penetrations[0], 150);
            backup.Points.AddXY(penetrations[penetrations.Length - 1], 150);
            backup.ChartType = SeriesChartType.Line;
            backup.BorderDashStyle = ChartDashStyle.Dash;
            view.MainChart.Series.Add(backup);
            // Setup view.
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            view.MainChart.ChartAreas[0].AxisY.Title = "Backup energy [TWh]";
            foreach (var series in view.MainChart.Series) series.BorderWidth = 3;
            ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\BackupEnergyAbsolute.png");
        }

        public static void BackupEnergyPlay(MainForm main)
        {
            var penetrations = new[] { 1.01, 1.02, 1.03, 1.04, 1.05 };
            var alphas = new[] { 0.60, 0.65, 0.70, 0.75, 0.80 };

            var ctrlFlow = new SimulationController
            {
                InvalidateCache = false,
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput(){Scheme = ExportScheme.ConstrainedLocalized}
                },
                Sources = new List<TsSourceInput> { new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 }, }
            };
            ctrlFlow.NodeFuncs.Clear();
            ctrlFlow.NodeFuncs.Add("Mega storage, battery storage", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            //var ctrlNoFlow = new SimulationController
            //{
            //    InvalidateCache = false,
            //    ExportStrategies = new List<ExportStrategyInput> { new ExportStrategyInput { ExportScheme = ExportScheme.None }, },
            //    Sources = new List<TsSourceInput> { new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 }, }
            //};
            //ctrlNoFlow.NodeFuncs.Clear();
            //ctrlNoFlow.NodeFuncs.Add("Mega storage, no storage", s =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
            //    ConfigurationUtils.SetupMegaStorage(nodes);
            //    return nodes;
            //});

            //var flowResults = DoBackupStuff(ctrlNoFlow, penetrations, alpha, false, true);

            var view = main.DisplayPlot();
            var backup = new Series("150 TWh");
            backup.Points.AddXY(penetrations[0], 150);
            backup.Points.AddXY(penetrations[penetrations.Length - 1], 150);
            backup.ChartType = SeriesChartType.Line;
            backup.BorderDashStyle = ChartDashStyle.Dash;
            view.MainChart.Series.Add(backup);

            foreach (var alpha in alphas)
            {
                var flowResults = DoBackupStuff(ctrlFlow, penetrations, alpha, false);
                //foreach (var result in flowResults) view.AddData(penetrations, result.Value, result.Key, false);
                foreach (var result in flowResults) view.AddData(penetrations, result.Value, result.Key + " " + alpha, false);
                // Setup view.
                view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
                view.MainChart.ChartAreas[0].AxisY.Title = "Backup energy [TWh]";
                foreach (var series in view.MainChart.Series) series.BorderWidth = 3;
                ChartUtils.SaveChart(view.MainChart, 1000, 500, string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\BackupEnergyAbsolute{0}.png", alpha));
            }
        }

        public static void BackupAnalysisNoLinks(MainForm main)
        {
            var ctrlFlow = new SimulationController
            {
                InvalidateCache = true,
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput{Scheme = ExportScheme.None}
                },
                Sources = new List<TsSourceInput> { new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 }, }
            };

            // With no storage.
            ctrlFlow.NodeFuncs.Clear();
            ctrlFlow.NodeFuncs.Add("Mega storage, no storage", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, false, false, false);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            var opts = OptimalDistributions(ctrlFlow.EvaluateTs(4.25, 0.80)[0]);
            opts.ToFile(@"C:\proto\OptimalNoStorageNoLinks.txt");

            // With battery storage.
            ctrlFlow.NodeFuncs.Clear();
            ctrlFlow.NodeFuncs.Add("Mega storage, batt", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, false, false);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            opts = OptimalDistributions(ctrlFlow.EvaluateTs(1.80, 0.70)[0]);
            opts.ToFile(@"C:\proto\OptimalBatteryNoLinks.txt");

            // With battery + hydrogen storage.
            ctrlFlow.NodeFuncs.Clear();
            ctrlFlow.NodeFuncs.Add("Mega storage, batt, hydrogen", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            opts = OptimalDistributions(ctrlFlow.EvaluateTs(1.20, 0.60)[0]);
            opts.ToFile(@"C:\proto\OptimalBatteryHydrogenNoLinks.txt");

            //var flowResults = DoBackupStuff(ctrlFlow, penetrations, alpha, false);
            //var view = main.DisplayPlot();
            //foreach (var result in flowResults) view.AddData(penetrations, result.Value, result.Key, false);
            //var backup = new Series("150 TWh");
            //backup.Points.AddXY(penetrations[0], 150);
            //backup.Points.AddXY(penetrations[penetrations.Length - 1], 150);
            //backup.ChartType = SeriesChartType.Line;
            //backup.BorderDashStyle = ChartDashStyle.Dash;
            //view.MainChart.Series.Add(backup);
            //// Setup view.
            //view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            //view.MainChart.ChartAreas[0].AxisY.Title = "Backup energy [TWh]";
            //foreach (var series in view.MainChart.Series) series.BorderWidth = 3;
            //ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\BackupEnergyAbsoluteWithStorage.png");
        }

        public static void BackupAnalysisWithLinks(MainForm main)
        {
            var ctrlFlow = new SimulationController
            {
                InvalidateCache = true,
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput{Scheme = ExportScheme.ConstrainedLocalized}
                },
                Sources = new List<TsSourceInput> { new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 }, }
            };

            // With no storage.
            ctrlFlow.NodeFuncs.Clear();
            ctrlFlow.NodeFuncs.Add("Mega storage, no storage", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, false, false, false);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            var opts = OptimalDistributions(ctrlFlow.EvaluateTs(1.725, 0.80)[0]);
            opts.ToFile(@"C:\proto\OptimalNoStorage.txt");

            // With battery storage.
            ctrlFlow.NodeFuncs.Clear();
            ctrlFlow.NodeFuncs.Add("Mega storage, batt", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, false, false);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            opts = OptimalDistributions(ctrlFlow.EvaluateTs(1.15, 0.70)[0]);
            opts.ToFile(@"C:\proto\OptimalBattery.txt");

            // With battery + hydrogen storage.
            ctrlFlow.NodeFuncs.Clear();
            ctrlFlow.NodeFuncs.Add("Mega storage, batt, hydrogen", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            opts = OptimalDistributions(ctrlFlow.EvaluateTs(1.025, 0.60)[0]);
            opts.ToFile(@"C:\proto\OptimalBatteryHydrogen.txt");

            //var flowResults = DoBackupStuff(ctrlFlow, penetrations, alpha, false);
            //var view = main.DisplayPlot();
            //foreach (var result in flowResults) view.AddData(penetrations, result.Value, result.Key, false);
            //var backup = new Series("150 TWh");
            //backup.Points.AddXY(penetrations[0], 150);
            //backup.Points.AddXY(penetrations[penetrations.Length - 1], 150);
            //backup.ChartType = SeriesChartType.Line;
            //backup.BorderDashStyle = ChartDashStyle.Dash;
            //view.MainChart.Series.Add(backup);
            //// Setup view.
            //view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            //view.MainChart.ChartAreas[0].AxisY.Title = "Backup energy [TWh]";
            //foreach (var series in view.MainChart.Series) series.BorderWidth = 3;
            //ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\BackupEnergyAbsoluteWithStorage.png");
        }

        public static void BackupAnalysisWithLinksWitDelta(MainForm main)
        {
            var ctrlFlow = new SimulationController
            {
                InvalidateCache = true,
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput{Scheme = ExportScheme.ConstrainedLocalized}
                },
                Sources = new List<TsSourceInput> { new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 }, }
            };

            // With battery + hydrogen storage.
            ctrlFlow.NodeFuncs.Clear();
            ctrlFlow.NodeFuncs.Add("Mega storage, batt (delta), hydrogen (delta)", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            var opts = OptimalDistributions(ctrlFlow.EvaluateTs(1.026, 0.65)[0]);
            opts.ToFile(@"C:\proto\OptimalBatteryHydrogenDelta.txt");

            //var flowResults = DoBackupStuff(ctrlFlow, penetrations, alpha, false);
            //var view = main.DisplayPlot();
            //foreach (var result in flowResults) view.AddData(penetrations, result.Value, result.Key, false);
            //var backup = new Series("150 TWh");
            //backup.Points.AddXY(penetrations[0], 150);
            //backup.Points.AddXY(penetrations[penetrations.Length - 1], 150);
            //backup.ChartType = SeriesChartType.Line;
            //backup.BorderDashStyle = ChartDashStyle.Dash;
            //view.MainChart.Series.Add(backup);
            //// Setup view.
            //view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            //view.MainChart.ChartAreas[0].AxisY.Title = "Backup energy [TWh]";
            //foreach (var series in view.MainChart.Series) series.BorderWidth = 3;
            //ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\BackupEnergyAbsoluteWithStorage.png");
        }

        private static Dictionary<string, double[]> DoBackupStuff(SimulationController ctrl, double[] penetrations, double alpha, bool normalize = true, bool includeHomo = false)
        {
            var results = new Dictionary<string, double[]>();

            //Parallel.For(0,penetrations.Length, idx =>
            for (int idx = 0; idx < penetrations.Length; idx++)
            {
                var res = ctrl.EvaluateTs(penetrations[idx], alpha);
                foreach (var sim in res)
                {
                    var backups = sim.TimeSeries.Where(item => item.Name.Contains("Backup")).ToArray();
                    var deltaFraction = OptimalDistribution(backups);
                    var key = ((TsSource)int.Parse(sim.Properties["TsSource"]) + ", " +
                               ((ExportScheme)int.Parse(sim.Properties["ExportScheme"])).GetDescription());
                    if (includeHomo)
                    {
                        var homoKey = key + ", homo";
                        var optKey = key + ", opt";
                        if (!results.ContainsKey(homoKey)) results.Add(homoKey, new double[penetrations.Length]);
                        if (!results.ContainsKey(optKey)) results.Add(optKey, new double[penetrations.Length]);
                        if (normalize)
                        {
                            results[homoKey][idx] = HomogeneousDistribution(backups);
                            results[optKey][idx] = deltaFraction;
                        }
                        else
                        {
                            // Backup per year in TWh  
                            results[homoKey][idx] = (HomogeneousDistribution(backups) *
                                                     backups.Select(item => item.First().Value).Sum()) /
                                                    double.Parse(sim.Properties["Length"]) / 1000;
                            results[optKey][idx] = (deltaFraction * backups.Select(item => item.First().Value).Sum()) /
                                                   double.Parse(sim.Properties["Length"]) / 1000;
                        }
                    }
                    else
                    {
                        if (!results.ContainsKey(key)) results.Add(key, new double[penetrations.Length]);
                        if (normalize) results[key][idx] = deltaFraction;
                        else
                        {
                            // Backup per year in TWh  
                            results[key][idx] = (deltaFraction * backups.Select(item => item.First().Value).Sum()) /
                                                double.Parse(sim.Properties["Length"]) / 1000;
                        }
                    }
                }
            }//);

            return results;
        }

        private static Dictionary<string, double> OptimalDistributions(SimulationOutput output, bool normalise = true)
        {
            var result = new Dictionary<string, double>();

            var backups = output.TimeSeries.Where(item => item.Name.Contains("Backup"));
            foreach (var backup in backups)
            {
                var start = backup.Select(item => item.Value).First();
                var end = backup.Select(item => item.Value).Last();
                result.Add(backup.Properties["Country"], start - end);
            }

            if (!normalise) return result;

            var max = result.Values.Max();
            foreach (var key in result.Keys.ToArray())
            {
                result[key] = result[key] / max;
            }

            return result;
        }

        private static double OptimalDistribution(ITimeSeries[] backups)
        {
            var start = backups.Select(item => item.First().Value).Sum();
            var end = backups.Select(item => item.Last().Value).Sum();
            return (start - end) / start;
        }

        private static double HomogeneousDistribution(ITimeSeries[] backups)
        {
            var deltas = new double[backups.Length];
            for (int i = 0; i < backups.Length; i++)
            {
                var values = backups[i].GetAllValues();
                deltas[i] = (values[0] - values[values.Count - 1]) / values[0];
            }
            return deltas.Max();
        }

        #endregion


        #region Backup analysis

        public static void BackupAnalysisWithStorage(MainForm main)
        {
            var tsView = new TimeSeriesView();
            tsView.MainChart.ChartAreas[0].AxisY.Title = "Consumption [yearly load]";
            tsView.MainChart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy";

            var results = new double[31];

            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.ISET, Offset = 0, Length = 8 });
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportSchemeInput
                {
                    Scheme = ExportScheme.UnconstrainedSynchronized
                });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("Infinite Backup WITH battery + hydrogen", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });

            var alphas = new[] { 0.0, 0.25, 0.5, 0.75, 1.0 };
            foreach (var alpha in alphas)
            {
                var data = ctrl.EvaluateTs(1, alpha);
                // Derive yearly relevant data.
                var derivedDataYearly = new List<ITimeSeries>
                {
                    //CreateTimeSeries(data[0], 8, "ISET@" + alpha, 21*8766, 8766),
                    CreateTimeSeries(data[1], 32, "VE@" + alpha, 0, 8766)
                };
                tsView.AddData(derivedDataYearly[0]);
                // Record results.
                var idx = 0; ;
                foreach (var point in derivedDataYearly[0])
                {
                    results[idx] += Math.Abs(point.Value - 1);
                    idx++;
                }
                //tsView.AddData(derivedDataYearly[1]);
            }

            ChartUtils.SaveChart(tsView.MainChart, 1000, 500,
    @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\BackupTsAlpha.png");

            tsView.MainChart.Series.Clear();
            var gammas = new[] { 0.8, 0.9, 1.0, 1.1, 1.2 };
            foreach (var gamma in gammas)
            {
                var data = ctrl.EvaluateTs(gamma, 0.65); // Maybe 0.65?
                // Derive yearly relevant data.
                var derivedDataYearly = new List<ITimeSeries>
                {
                    //CreateTimeSeries(data[0], 8, "ISET@" + alpha, 21*8766, 8766),
                    CreateTimeSeries(data[1], 32, "VE@" + gamma, 0, 8766)
                };
                tsView.AddData(derivedDataYearly[0]);
                // Record results.
                var idx = 0; ;
                foreach (var point in derivedDataYearly[0])
                {
                    results[idx] += Math.Abs(point.Value - 1);
                    idx++;
                }
                //tsView.AddData(derivedDataYearly[1]);
            }

            ChartUtils.SaveChart(tsView.MainChart, 1000, 500,
                @"C:\Users\Emil\Dropbox\Master Thesis\Thesis\Figures\BackupTsGamma.png");

            // Find the best year.
            var minVal = results.Min();
            for (int i = 0; i < 31; i++)
            {
                if (results[i].Equals(minVal))
                {
                    Console.WriteLine("The optimal year is {0}", 1979 + i);
                }
            }
        }

        private static SparseTimeSeries CreateTimeSeries(SimulationOutput sim, int length, string name, int offset, int avgInterval)
        {
            var idx = 1;
            var backup = new SparseTimeSeries(name);
            var backupTs = sim.TimeSeries.Where(item => item.Name.Contains("Backup"))
                .Select(item => (DenseTimeSeries)item)
                .ToArray();
            var start = backupTs.Select(item => item.First().Value).Sum();
            var end = backupTs.Select(item => item.Last().Value).Sum();
            var endSum = start - end;
            var avg = endSum / length;

            while (idx < length * (8766 / avgInterval))
            {
                var sum = backupTs.Select(item => item.GetValue(idx * avgInterval)).Sum();
                var delta = start - sum;
                start = sum;
                backup.AddData(idx * avgInterval + offset, delta / avg);
                // Go to next year;
                idx++;
            }

            return backup;
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
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput()
                    {
                        Scheme = ExportScheme.UnconstrainedSynchronized
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
                ExportStrategies = new List<ExportSchemeInput>
                {
                    new ExportSchemeInput()
                    {
                        Scheme = ExportScheme.UnconstrainedSynchronized
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
