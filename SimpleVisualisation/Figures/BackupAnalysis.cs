using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;
using BusinessLogic.Interfaces;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Controls.Charting;
using SimpleImporter;
using Utils;

namespace Main.Figures
{
    class BackupAnalysis
    {

        public static void BackupPerCountry(MainForm main)
        {
            var alpha = 0.8;
            var penetrations = new[] { 1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.75, 2.0, 2.25, 2.5 };
            var ctrl = new SimulationController
            {
                InvalidateCache = false,
                ExportStrategies = new List<ExportStrategyInput>
                {
                    new ExportStrategyInput{ExportStrategy = ExportStrategy.None},
                    //new ExportStrategyInput{ExportStrategy = ExportStrategy.Cooperative, DistributionStrategy = DistributionStrategy.SkipFlow}
                },
                Sources = new List<TsSourceInput>
                {
                    new TsSourceInput {Source = TsSource.VE, Offset = 0, Length = 32},
                    //new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 8},
                }
            };

            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("No storage", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });

            var timeSeries = ProtoStore.LoadCountries().ToDictionary(item => item, item => new double[penetrations.Length]);
            timeSeries.Add("All", new double[penetrations.Length]);
            timeSeries.Add("Average", new double[penetrations.Length]);

            for (int i = 0; i < penetrations.Length; i++)
            {
                var data = ctrl.EvaluateTs(penetrations[i], alpha)[0];
                var countries = data.TimeSeries.
                    Where(item => item.Name.Contains("Curtailment")).
                    Where(item => item.Properties.ContainsKey("Country"))
                    .ToDictionary(item => item.Properties["Country"],
                        item => item.GetAllValues().Where(dbl => dbl < 0).Select(val => -val).Sum());
                foreach (var pair in countries)
                {
                    timeSeries[pair.Key][i] = pair.Value / (CountryInfo.GetMeanLoad(pair.Key) * 8766 * 32);                    
                }
                timeSeries["Average"][i] = countries.Select(item => item.Value).Sum()/
                                           (CountryInfo.GetMeanLoadSum()*8766*32);
                timeSeries["All"][i] =
                    data.TimeSeries.Single(item => item.Name.Equals("Curtailment"))
                        .GetAllValues()
                        .Where(dbl => dbl < 0)
                        .Select(val => -val)
                        .Sum() / (CountryInfo.GetMeanLoadSum() * 8766 * 32);
                    
            }

            var view = main.DisplayPlot();
            foreach (var result in timeSeries)
            {
                view.AddData(penetrations, result.Value, result.Key, false);
            }

            // Setup view.
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            view.MainChart.ChartAreas[0].AxisY.Title = "Backup energy divided by mean load";

            // Result filtering.
            var max = timeSeries.First(item => item.Value[0].Equals(timeSeries.Select(dbl => dbl.Value[0]).Max()));
            foreach (var series in view.MainChart.Series)
            {
                if (!series.Name.Equals("All") && !series.Name.Equals("Average") && !series.Name.Equals(max.Key)) continue;
                series.BorderWidth = 5;
            }

            ChartUtils.SaveChart(view.MainChart, 1500, 800, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\CountryBackups.png");
        }

        public static void Full(MainForm main)
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
                    .Select(item => new IndexedSparseTimeSeries((SparseTimeSeries)item))
                    .ToArray();
            var lastSum = 150000.0 * length;

            while (idx < length * (8766 / avgInterval))
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

    }
}
