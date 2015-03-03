using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;
using BusinessLogic.Cost;
using BusinessLogic.Interfaces;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Controls.Charting;
using Newtonsoft.Json;
using SimpleImporter;
using Utils;
using Utils.Statistics;

namespace Main.Figures
{
    class ModelYearAnalysis
    {

        #region Error analysis

        // Check the error depends on alpha/gamma.
        public static void ErrorAnalysis(MainForm main, bool inclTrans = false)
        {

            // Read year config.
            var config = FileUtils.FromJsonFile<ModelYearConfig>(@"C:\Users\Emil\Dropbox\Master Thesis\noStorageAlpha0.5to1Gamma0.5to2.txt");
            var gammaRes = 10;
            var alphaRes = 10;

            #region Simulation setup

            // Setup REFERENCE control.
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = inclTrans ? ExportStrategy.ConstrainedFlow : ExportStrategy.Cooperative,
                });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("Clean nodes", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                //ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            ctrl.LogSystemProperties = true;
            ctrl.LogNodalBalancing = true;
            ctrl.LogFlows = inclTrans;

            // Setup BC ESTIMATION control.
            var bcEstCtrl = new SimulationController { InvalidateCache = false };
            bcEstCtrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = config.Parameters["bc"].Key, Length = 1 }); // 23
            bcEstCtrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.Cooperative, DistributionStrategy = DistributionStrategy.SkipFlow
                });
            bcEstCtrl.NodeFuncs.Clear();
            bcEstCtrl.NodeFuncs.Add("Clean nodes", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                //ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            bcEstCtrl.LogNodalBalancing = true;

            // Setup BE ESTIMATION control.
            var beEstCtrl = new SimulationController { InvalidateCache = false };
            beEstCtrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = config.Parameters["be"].Key, Length = 1 }); // 25
            beEstCtrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.Cooperative,
                    DistributionStrategy = DistributionStrategy.SkipFlow
                });
            beEstCtrl.NodeFuncs.Clear();
            beEstCtrl.NodeFuncs.Add("Clean nodes", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                //ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            beEstCtrl.LogSystemProperties = true;

            // Setup TC ESTIMATION control.
            var tcEstCtrl = new SimulationController { InvalidateCache = false };
            if (inclTrans)
            {
                tcEstCtrl.Sources.Add(new TsSourceInput
                {
                    Source = TsSource.VE,
                    Offset = config.Parameters["tc"].Key,
                    Length = 1
                }); // 14
                tcEstCtrl.ExportStrategies.Add(
                    new ExportStrategyInput
                    {
                        ExportStrategy = ExportStrategy.ConstrainedFlow
                    });
                tcEstCtrl.NodeFuncs.Clear();
                tcEstCtrl.NodeFuncs.Add("Clean nodes", s =>
                {
                    var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                    //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                    //ConfigurationUtils.SetupMegaStorage(nodes);
                    return nodes;
                });
                tcEstCtrl.LogFlows = true;
            }

            #endregion

            // Prepare data structures.
            var gammas = MathUtils.Linspace(config.GammaMin, config.GammaMax, gammaRes);
            var alphas = MathUtils.Linspace(config.AlphaMin, config.AlphaMax, alphaRes);
            var beMatrix = new double[gammas.Length, alphas.Length];
            var bcMatrix = new double[gammas.Length, alphas.Length];
            var tcMatrix = new double[gammas.Length, alphas.Length];
            var gammaMatrix = new double[gammas.Length, alphas.Length];
            var alphaMatrix = new double[gammas.Length, alphas.Length];

            // Fill data structures.
            for (int i = 0; i < gammas.Length; i++)
            {
                var gamma = gammas[i];
                for (int j = 0; j < alphas.Length; j++)
                {
                    var alpha = alphas[j];
                    var data = ctrl.EvaluateTs(gamma, alpha);
                    var bcEstData = bcEstCtrl.EvaluateTs(gamma, alpha);
                    var beEstData = beEstCtrl.EvaluateTs(gamma, alpha);
                    // Calculate the interesting variables.
                    var backupCapacity = CalculateBackupCapacity(data[0]);
                    var transmissionCapacity = CalculateFlowCapacity(data[0]);
                    var energy = CalculateBackupEnergy(data[0])/32;
                    var estBackupCapacity = CalculateBackupCapacity(bcEstData[0]);
                    var estEnergy = CalculateBackupEnergy(beEstData[0]);
                    // Record result in matrix (to be plotted); scaling applied.
                    beMatrix[i, j] = (estEnergy * config.Parameters["be"].Value - energy) / energy;
                    bcMatrix[i, j] = (estBackupCapacity * config.Parameters["bc"].Value - backupCapacity) / backupCapacity;
                    gammaMatrix[i, j] = gamma;
                    alphaMatrix[i, j] = alpha;
                    // Do transmission only if necessary.
                    if (inclTrans)
                    {
                        var tcEstData = tcEstCtrl.EvaluateTs(gamma, alpha);
                        var estTransmissionCapacity = CalculateFlowCapacity(tcEstData[0]);
                        tcMatrix[i, j] = (estTransmissionCapacity * config.Parameters["tc"].Value - transmissionCapacity) /
                                         transmissionCapacity;
                    }
                }
            }

            // Save results.
            beMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\beMatrixIIHD.txt");
            bcMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\bcMatrixIIHD.txt");
            tcMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\tcMatrixIIHD.txt");
            gammaMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\gammaMatrixIIHD.txt");
            alphaMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\alphaMatrixIIHD.txt");

            // Save in dict too.
            var dict = new Dictionary<string, double[,]>
            {
                {"BE", beMatrix},
                {"BC", bcMatrix},
                {"TC", tcMatrix},
                {"Alpha", alphaMatrix},
                {"Gamma", gammaMatrix},
            };
            dict.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\modelYear\errorAnalysis.txt");
        }

        private static double CalculateFlowCapacity(SimulationOutput sim)
        {
            var flows = sim.TimeSeries.Where(item => item.Properties.ContainsKey("Flow"));
            var allFlow = 0.0;
            // Fill in data.
            foreach (var flow in flows)
            {
                var length = Costs.LinkLength[Costs.GetKey(flow.Properties["From"], flow.Properties["To"])];
                var capacity = MathUtils.CalcCapacity(flow.GetAllValues());
                allFlow += length * capacity;
            }
            return allFlow;
        }

        private static double CalculateBackupCapacity(SimulationOutput sim)
        {
            var data = sim.TimeSeries.Where(item => item.Name.Equals("Curtailment"))
                .Select(item => (DenseTimeSeries)item)
                .Single().GetAllValues().Where(item => item < 0).Select(item => -item).ToList();
            return MathUtils.Percentile(data, 99);
        }

        private static double CalculateBackupEnergy(SimulationOutput sim)
        {
            var data = sim.TimeSeries.Where(item => item.Name.Equals("Curtailment"))
                .Select(item => (DenseTimeSeries)item)
                .Single().GetAllValues().Where(item => item < 0).Select(item => -item).ToList();
            return data.Sum();
        }

        #endregion

        #region Locating the model year

        public static void PrintModelYearStuff(MainForm main, bool inclTrans = false)
        {
            var aMin = 0.5;
            var aMax = 1;
            var gMin = 0.5;
            var gMax = 1;
            ModelYearStuff(aMin, aMax, gMin, gMax, inclTrans, data =>
            {
                PrintData(CalculateBackupEnergy, data, "be.txt");
                PrintData(CalculateBackupCapacity, data, "bc.txt");
                PrintData(CalculateTransmissionCapacity, data, "tc.txt");
            });
        }

        public static void DetermineModelYears(MainForm main, bool inclTrans = false)
        {
            var aMin = 0.5;
            var aMax = 1;
            var gMin = 0.5;
            var gMax = 2;
            ModelYearStuff(aMin, aMax, gMin, gMax, inclTrans, data =>
            {
                var result = new ModelYearConfig
                {
                    AlphaMin = aMin,
                    AlphaMax = aMax,
                    GammaMin = gMin,
                    GammaMax = gMax,
                    Parameters = new Dictionary<string, KeyValuePair<int, double>>()
                };
                result.Parameters.Add("be", FindBcYear(CalculateBackupEnergy, data));
                result.Parameters.Add("bc", FindBcYear(CalculateBackupCapacity, data));
                if (inclTrans) result.Parameters.Add("tc", FindBcYear(CalculateTransmissionCapacity, data));

                result.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\noStorageAlpha0.5to1Gamma0.5to2.txt");
            });
        }

        private static void ModelYearStuff(double aMin, double aMax, double gMin, double gMax, bool inclTrans, Action<List<SimulationOutput>> action)
        {
            // Prepare simulation.
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = inclTrans ? ExportStrategy.ConstrainedFlow : ExportStrategy.Cooperative,
                    DistributionStrategy = DistributionStrategy.SkipFlow
                });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("Clean nodes", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                //ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            ctrl.LogSystemProperties = true;
            ctrl.LogNodalBalancing = true;
            ctrl.LogFlows = inclTrans;

            var alpha = (aMax + aMin) / 2;
            var gamma = (gMax + gMin) / 2;
            // Create data.
            var centerData = ctrl.EvaluateTs(gamma, alpha);
            var aMinData = ctrl.EvaluateTs(gamma, aMin);
            var aMaxData = ctrl.EvaluateTs(gamma, aMax);
            var gMinData = ctrl.EvaluateTs(gMin, alpha);
            var gMaxData = ctrl.EvaluateTs(gMax, alpha);
            var data = new List<SimulationOutput> { centerData[0], aMinData[0], aMaxData[0], gMinData[0], gMaxData[0], };

            action(data);
        }

        private static KeyValuePair<int, double> FindBcYear(Action<SimulationOutput, Dictionary<int, double>> fill, List<SimulationOutput> data)
        {
            var cenYears = new Dictionary<int, double>();
            var aMinYears = new Dictionary<int, double>();
            var aMaxYears = new Dictionary<int, double>();
            var gMinYears = new Dictionary<int, double>();
            var gMaxYears = new Dictionary<int, double>();
            var dicts = new List<Dictionary<int, double>> { cenYears, aMinYears, aMaxYears, gMinYears, gMaxYears };
            for (int i = 0; i < 5; i++) fill(data[i], dicts[i]);
            var dAlpha = cenYears.Keys.ToDictionary(key => key, key => Math.Pow(aMinYears[key] - aMaxYears[key], 2));
            var dGamma = cenYears.Keys.ToDictionary(key => key, key => Math.Pow(gMinYears[key] - gMaxYears[key], 2));
            var dAll = cenYears.Keys.ToDictionary(key => key, key => dAlpha[key] + dGamma[key]);
            var year = dAll.Where(item0 => item0.Value == dAll.Select(item1 => item1.Value).Min()).Select(item => item.Key).First();
            return new KeyValuePair<int, double>(year, 1 / cenYears[year]);
        }

        private static void PrintData(Action<SimulationOutput, Dictionary<int, double>> fill, List<SimulationOutput> data, string name)
        {
            var cenYears = new Dictionary<int, double>();
            var aMinYears = new Dictionary<int, double>();
            var aMaxYears = new Dictionary<int, double>();
            var gMinYears = new Dictionary<int, double>();
            var gMaxYears = new Dictionary<int, double>();
            var dicts = new List<Dictionary<int, double>> { cenYears, aMinYears, aMaxYears, gMinYears, gMaxYears };
            for (int i = 0; i < 5; i++) fill(data[i], dicts[i]);
            var all = new Dictionary<string, Dictionary<int, double>>
            {
                {"centre", cenYears},
                {"aMin", aMinYears},
                {"aMax", aMaxYears},
                {"gMin", gMinYears},
                {"gMax", gMaxYears},
            };
            all.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\modelYear\" + name);
        }

        #endregion

        private static void CalculateBackupEnergy(SimulationOutput sim, Dictionary<int, double> energyMap)
        {
            var curtailment = sim.TimeSeries.Where(item => item.Name.Equals("Curtailment"))
                .Select(item => (DenseTimeSeries)item)
                .Single();
            var years = (int) Math.Ceiling((double) (curtailment.Count/8766));
            // Prepare data structures.
            var yearBins = new Dictionary<int, List<double>>();
            var allData = new List<double>(years * 5000);
            for (int i = 0; i < years; i++) yearBins.Add(i, new List<double>(5000));
            // Fill in data.
            var idx = -1;
            foreach (var value in curtailment.GetAllValues())
            {
                idx++;
                if (value > 0) continue;
                allData.Add(-value);
                yearBins[idx / 8766].Add(-value);
            }
            // Calculate the interesting variables.
            var energy = allData.Sum() / years;
            energyMap.Clear();
            for (int i = 0; i < years; i++)
            {
                energyMap.Add(i, yearBins[i].Sum() / energy);
            }
        }

        private static void CalculateBackupCapacity(SimulationOutput sim, Dictionary<int, double> capacityMap)
        {
            var curtailment = sim.TimeSeries.Where(item => item.Name.Equals("Curtailment"))
                .Select(item => (DenseTimeSeries) item)
                .Single();
            var years = (int)Math.Ceiling((double)(curtailment.Count / 8766));
            // Prepare data structures.
            var yearBins = new Dictionary<int, List<double>>();
            var allData = new List<double>(years * 5000);
            for (int i = 0; i < years; i++) yearBins.Add(i, new List<double>(5000));
            // Fill in data.
            var idx = -1;
            foreach (var value in curtailment.GetAllValues())
            {
                idx++;
                if(value > 0) continue;
                allData.Add(-value);
                yearBins[idx/8766].Add(-value);
            }
            // Calculate the interesting variables.
            var capacity = MathUtils.Percentile(allData, 99);
            capacityMap.Clear();
            for (int i = 0; i < years; i++)
            {
                capacityMap.Add(i, MathUtils.Percentile(yearBins[i], 99)/capacity);
            }
        }

        private static void CalculateTransmissionCapacity(SimulationOutput sim, Dictionary<int, double> capacityMap)
        {
            var flows = sim.TimeSeries.Where(item => item.Properties.ContainsKey("Flow"));
            var years = (int)Math.Ceiling((double)(flows.First().Count / 8766));
            // Prepare data structures.
            var allFlow = 0.0;
            capacityMap.Clear();
            for (int i = 0; i < years; i++) capacityMap.Add(i, 0);
            // Fill in data.
            foreach (var flow in flows)
            {
                var length = Costs.LinkLength[Costs.GetKey(flow.Properties["From"], flow.Properties["To"])];
                var capacity = MathUtils.CalcCapacity(flow.GetAllValues());
                allFlow += capacity*length;
                for (int i = 0; i < years; i++)
                {
                    capacityMap[i] += MathUtils.CalcCapacity(flow.Where(item =>
                        item.Tick < (i + 1)*8766).Where(item => item.Tick > i*8766).Select(item => item.Value))*length;
                }
            }
            for (int i = 0; i < years; i++) capacityMap[i] = capacityMap[i] / allFlow;
        }

        #region Backup analysis

        public static void BackupAnalysis(MainForm main)
        {
            // Prepare charts.
            var beView = CreateTsView("Backup energy");
            var bcView = CreateTsView("Backup capacity");
            var tcView = CreateTsView("Transmission capacity");
            // Prepare simulation.
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportStrategyInput
                {
                    ExportStrategy = ExportStrategy.ConstrainedFlow
                });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("Clean nodes", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                //ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            ctrl.LogSystemProperties = true;
            ctrl.LogNodalBalancing = true;
            ctrl.LogFlows = true;

            // Try different alphas.
            var alphas = new[] { 0.0, 0.25, 0.5, 0.75, 1.0 };
            var gammas = new[] { 1.0 };
            FillCharts(ctrl, new[] { beView, bcView, tcView }, alphas, gammas);
            ChartUtils.SaveChart(beView.MainChart, 1000, 500,
            @"C:\Users\Emil\Dropbox\Master Thesis\ModelYear\Figures\beTsAlpha.png");
            ChartUtils.SaveChart(bcView.MainChart, 1000, 500,
            @"C:\Users\Emil\Dropbox\Master Thesis\ModelYear\Figures\bcTsAlpha.png");
            ChartUtils.SaveChart(tcView.MainChart, 1000, 500,
            @"C:\Users\Emil\Dropbox\Master Thesis\ModelYear\Figures\tcTsAlpha.png");

            // Reset views.
            beView.MainChart.Series.Clear();
            bcView.MainChart.Series.Clear();
            tcView.MainChart.Series.Clear();

            // Try different gammas.
            gammas = new[] { 0.9, 1.0, 1.1 };
            alphas = new[] { 0.65 };
            FillCharts(ctrl, new[] { beView, bcView, tcView }, alphas, gammas);
            ChartUtils.SaveChart(beView.MainChart, 1000, 500,
            @"C:\Users\Emil\Dropbox\Master Thesis\ModelYear\Figures\beTsGamma.png");
            ChartUtils.SaveChart(bcView.MainChart, 1000, 500,
            @"C:\Users\Emil\Dropbox\Master Thesis\ModelYear\Figures\bcTsGamma.png");
            ChartUtils.SaveChart(tcView.MainChart, 1000, 500,
            @"C:\Users\Emil\Dropbox\Master Thesis\ModelYear\Figures\tcTsGamma.png");
        }

        private static void FillCharts(SimulationController ctrl, TimeSeriesView[] views, double[] alphas, double[] gammas)
        {
            foreach (var gamma in gammas)
            {
                foreach (var alpha in alphas)
                {
                    var name = string.Format("VE@({0},{1})", alpha, gamma);
                    var data = ctrl.EvaluateTs(gamma, alpha);
                    // Derive yearly relevant data.
                    var transmissionCapacity = new Dictionary<int, double>();
                    var backupCapacity = new Dictionary<int, double>();
                    var backupEnergy = new Dictionary<int, double>();
                    CalculateTransmissionCapacity(data[0], transmissionCapacity);
                    CalculateBackupCapacity(data[0], backupCapacity);
                    CalculateBackupEnergy(data[0], backupEnergy);
                    views[0].AddData(ToTimeSeries(name, backupCapacity));
                    views[1].AddData(ToTimeSeries(name, backupEnergy));
                    views[2].AddData(ToTimeSeries(name, transmissionCapacity));
                    // Just for debugging.
                    transmissionCapacity.ToFile(@"C:\proto\transTmp");
                    backupCapacity.ToFile(@"C:\proto\backupCapTmp");
                    backupEnergy.ToFile(@"C:\proto\backupEnergyTmp");
                }
            }

            foreach (var view in views) ScaleAxis(view);
        }

        #endregion

        #region Utils

        private static SparseTimeSeries ToTimeSeries(string name, Dictionary<int, double> map)
        {
            var result = new SparseTimeSeries(name);
            foreach (var pair in map)
            {
                result.AddData(pair.Key*8766, pair.Value);
            }
            return result;
        }

        private static TimeSeriesView CreateTsView(string name)
        {
            var view = new TimeSeriesView();
            view.MainChart.ChartAreas[0].AxisY.Title = name;
            view.MainChart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy";
            return view;
        }

        private static void ScaleAxis(TimeSeriesView view, double min = 0.4, double max = 1.4)
        {
            view.MainChart.ChartAreas[0].AxisY.Minimum = min;
            view.MainChart.ChartAreas[0].AxisY.Maximum = max;
        }

        #endregion

    }

}
