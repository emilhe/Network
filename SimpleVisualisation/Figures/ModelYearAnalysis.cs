using System;
using System.Collections.Generic;
using System.IO;
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

        private const string basePath = @"C:\Users\Emil\Dropbox\BACKUP\Python\modelYear";

        #region Error analysis

        // Check the error depends on alpha/gamma.
        public static void ErrorAnalysis(MainForm main)
        {

            // Read year config.
            var config = FileUtils.FromJsonFile<ModelYearConfig>(Path.Combine(basePath, @"Alpha0.5to1Gamma0.5to2Sync.txt"));
            var gammaRes = 10;
            var alphaRes = 10;

            #region Simulation setup

            // Setup REFERENCE control.
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportSchemeInput()
                {
                    Scheme = ExportScheme.UnconstrainedSynchronized
                });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("Clean nodes", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                //ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            ctrl.LogFlows = true;

            // Setup ESTIMATION control.
            var estCtrl = new SimulationController { InvalidateCache = false };
            estCtrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = config.Offset, Length = 1 });
            estCtrl.ExportStrategies.Add(
                 new ExportSchemeInput()
                 {
                     Scheme = ExportScheme.UnconstrainedSynchronized
                 });
            estCtrl.NodeFuncs.Clear();
            estCtrl.NodeFuncs.Add("Clean nodes", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                //ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
            estCtrl.LogFlows = true;

            //// Setup BE ESTIMATION control.
            //var beEstCtrl = new SimulationController { InvalidateCache = false };
            //beEstCtrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = config.Parameters["be"].Key, Length = 1 });
            //beEstCtrl.ExportStrategies.Add(
            //    new ExportSchemeInput()
            //    {
            //        Scheme = ExportScheme.UnconstrainedSynchronized
            //    });
            //beEstCtrl.NodeFuncs.Clear();
            //beEstCtrl.NodeFuncs.Add("Clean nodes", s =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
            //    //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
            //    //ConfigurationUtils.SetupMegaStorage(nodes);
            //    return nodes;
            //});

            //// Setup TC ESTIMATION control.
            //var tcEstCtrl = new SimulationController { InvalidateCache = false };
            //if (inclTrans)
            //{
            //    tcEstCtrl.Sources.Add(new TsSourceInput
            //    {
            //        Source = TsSource.VE,
            //        Offset = config.Parameters["tc"].Key,
            //        Length = 1
            //    }); // 14
            //    tcEstCtrl.ExportStrategies.Add(
            //    new ExportSchemeInput()
            //    {
            //        Scheme = ExportScheme.UnconstrainedSynchronized
            //    });
            //    tcEstCtrl.NodeFuncs.Clear();
            //    tcEstCtrl.NodeFuncs.Add("Clean nodes", s =>
            //    {
            //        var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
            //        //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
            //        //ConfigurationUtils.SetupMegaStorage(nodes);
            //        return nodes;
            //    });
            //    tcEstCtrl.LogFlows = true;
            //}

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
                    var estData = estCtrl.EvaluateTs(gamma, alpha);
                    // Calculate the interesting variables.
                    var backupCapacity = ParameterEvaluator.BackupCapacity(data[0]);
                    var transmissionCapacity = ParameterEvaluator.TransmissionCapacity(data[0]);
                    var energy = ParameterEvaluator.BackupEnergy(data[0])/32;
                    var estBackupCapacity = ParameterEvaluator.BackupCapacity(estData[0]);
                    var estEnergy = ParameterEvaluator.BackupEnergy(estData[0]);
                    // Record result in matrix (to be plotted); scaling applied.
                    beMatrix[i, j] = (estEnergy*config.Parameters["be"] - energy)/energy;
                    bcMatrix[i, j] = (estBackupCapacity*config.Parameters["bc"] - backupCapacity)/backupCapacity;
                    gammaMatrix[i, j] = gamma;
                    alphaMatrix[i, j] = alpha;
                    // Do transmission only if necessary.
                    var estTransmissionCapacity = ParameterEvaluator.TransmissionCapacity(estData[0]);
                    tcMatrix[i, j] = (estTransmissionCapacity*config.Parameters["tc"] - transmissionCapacity)/
                                     transmissionCapacity;
                }
            }

            //// Save results.
            //beMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\beMatrixIIHD.txt");
            //bcMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\bcMatrixIIHD.txt");
            //tcMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\tcMatrixIIHD.txt");
            //gammaMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\gammaMatrixIIHD.txt");
            //alphaMatrix.ToFile(@"C:\Users\Emil\Dropbox\Master Thesis\Matlab\alphaMatrixIIHD.txt");

            // Save in dict too.
            var dict = new Dictionary<string, double[,]>
            {
                {"BE", beMatrix},
                {"BC", bcMatrix},
                {"TC", tcMatrix},
                {"Alpha", alphaMatrix},
                {"Gamma", gammaMatrix},
            };
            dict.ToJsonFile(Path.Combine(basePath, @"errorAnalysis.txt"));
        }

        #endregion

        #region Locating the model year

        public static void PrintModelYearStuff(MainForm main)
        {
            var aMin = 0.5;
            var aMax = 1;
            var gMin = 0.5;
            var gMax = 2;
            ModelYearStuff(aMin, aMax, gMin, gMax, data =>
            {
                PrintData(CalculateBackupEnergy, data, "be.txt");
                PrintData(CalculateBackupCapacity, data, "bc.txt");
                PrintData(CalculateTransmissionCapacity, data, "tc.txt");
            });
        }

        private static void ModelYearStuff(double aMin, double aMax, double gMin, double gMax, Action<List<SimulationOutput>> action)
        {
            // Prepare simulation.
            var ctrl = new SimulationController { InvalidateCache = false };
            ctrl.Sources.Add(new TsSourceInput { Source = TsSource.VE, Offset = 0, Length = 32 });
            ctrl.ExportStrategies.Add(
                new ExportSchemeInput()
                {
                    Scheme = ExportScheme.UnconstrainedSynchronized
                });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("Clean nodes", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                return nodes;
            });
            ctrl.LogFlows = true;

            var alpha = (aMax + aMin) / 2;
            //var gamma = (gMax + gMin) / 2;
            var gamma = (gMax/2 + gMin*2) / 2;            
            // Create data.
            var centerData = ctrl.EvaluateTs(gamma, alpha);
            var aMinData = ctrl.EvaluateTs(gamma, aMin);
            var aMaxData = ctrl.EvaluateTs(gamma, aMax);
            var gMinData = ctrl.EvaluateTs(gMin, alpha);
            var gMaxData = ctrl.EvaluateTs(gMax, alpha);
            var data = new List<SimulationOutput> { centerData[0], aMinData[0], aMaxData[0], gMinData[0], gMaxData[0], };

            action(data);
        }

        public static void DetermineModelYear(MainForm main)
        {
            var aMin = 0.5;
            var aMax = 1;
            var gMin = 0.5;
            var gMax = 2;
            ModelYearStuff(aMin, aMax, gMin, gMax, data =>
            {
                var pair = FindBestYear(new Dictionary<string, Action<SimulationOutput, Dictionary<int, double>>>
                {
                    {"be", CalculateBackupEnergy},
                    {"bc", CalculateBackupCapacity},
                    {"tc", CalculateTransmissionCapacity},
                }, data);
                var result = new ModelYearConfig
                {
                    AlphaMin = aMin,
                    AlphaMax = aMax,
                    GammaMin = gMin,
                    GammaMax = gMax,
                    Offset = pair.Key,
                    Parameters = pair.Value
                };
                result.ToJsonFile(Path.Combine(basePath,string.Format("Alpha{0}to{1}Gamma{2}to{3}Sync.txt",aMin, aMax, gMin, gMax)));
            });
        }

        private static KeyValuePair<int,Dictionary<string, double>> FindBestYear(Dictionary<string,Action<SimulationOutput, Dictionary<int, double>>> fill, List<SimulationOutput> data)
        {
            var dTot = new Dictionary<int, double>();
            var cenYearsDict = fill.Keys.ToDictionary(item => item, item => new Dictionary<int, double>());
            foreach (var key in fill.Keys)
            {
                var aMinYears = new Dictionary<int, double>();
                var aMaxYears = new Dictionary<int, double>();
                var gMinYears = new Dictionary<int, double>();
                var gMaxYears = new Dictionary<int, double>();
                var cenYears = new Dictionary<int, double>();
                var dicts = new List<Dictionary<int, double>> { cenYears, aMinYears, aMaxYears, gMinYears, gMaxYears };
                for (int i = 0; i < 5; i++) fill[key](data[i], dicts[i]);
                var dAlpha = cenYears.Keys.ToDictionary(item => item, item => Math.Pow(aMinYears[item] - aMaxYears[item], 2));
                var dGamma = cenYears.Keys.ToDictionary(item => item, item => Math.Pow(gMinYears[item] - gMaxYears[item], 2));
                var dAll = cenYears.Keys.ToDictionary(item => item, item => dAlpha[item] + dGamma[item]);

                cenYearsDict[key] = cenYears;
                foreach (var item in dAll.Keys)
                {
                    if(!dTot.ContainsKey(item)) dTot.Add(item, 0);
                    dTot[item] = dTot[item] + dAll[item];
                }
            }
            var year = dTot.Where(item0 => item0.Value == dTot.Select(item1 => item1.Value).Min()).Select(item => item.Key).First();
            return new KeyValuePair<int, Dictionary<string, double>>(year,fill.Keys.ToDictionary(key => key, key => 1.0/cenYearsDict[key][year]));
        }

        //private static KeyValuePair<int, double> FindBcYear(Action<SimulationOutput, Dictionary<int, double>> fill, List<SimulationOutput> data)
        //{
        //    var cenYears = new Dictionary<int, double>();
        //    var aMinYears = new Dictionary<int, double>();
        //    var aMaxYears = new Dictionary<int, double>();
        //    var gMinYears = new Dictionary<int, double>();
        //    var gMaxYears = new Dictionary<int, double>();
        //    var dicts = new List<Dictionary<int, double>> { cenYears, aMinYears, aMaxYears, gMinYears, gMaxYears };
        //    for (int i = 0; i < 5; i++) fill(data[i], dicts[i]);
        //    var dAlpha = cenYears.Keys.ToDictionary(key => key, key => Math.Pow(aMinYears[key] - aMaxYears[key], 2));
        //    var dGamma = cenYears.Keys.ToDictionary(key => key, key => Math.Pow(gMinYears[key] - gMaxYears[key], 2));
        //    var dAll = cenYears.Keys.ToDictionary(key => key, key => dAlpha[key] + dGamma[key]);
        //    var year = dAll.Where(item0 => item0.Value == dAll.Select(item1 => item1.Value).Min()).Select(item => item.Key).First();
        //    return new KeyValuePair<int, double>(year, 1 / cenYears[year]);
        //}

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
            all.ToJsonFile(Path.Combine(basePath, name));
        }

        #endregion

        private static void CalculateBackupEnergy(SimulationOutput sim, Dictionary<int, double> energyMap)
        {
            var years = (int)Math.Ceiling((double)(sim.TimeSeries[0].Count / Stuff.HoursInYear));
            var energy = ParameterEvaluator.BackupEnergy(sim) / years;
            for (int i = 0; i < years; i++)
            {
                energyMap.Add(i, ParameterEvaluator.BackupEnergy(sim, 1, i * Stuff.HoursInYear, Stuff.HoursInYear) / energy);
            }
        }

        private static void CalculateBackupCapacity(SimulationOutput sim, Dictionary<int, double> capacityMap)
        {
            var years = (int)Math.Ceiling((double)(sim.TimeSeries[0].Count / Stuff.HoursInYear));
            var energy = ParameterEvaluator.BackupCapacity(sim);
            for (int i = 0; i < years; i++)
            {
                capacityMap.Add(i, ParameterEvaluator.BackupCapacity(sim, 1, i * Stuff.HoursInYear, Stuff.HoursInYear) / energy);
            }
        }

        private static void CalculateTransmissionCapacity(SimulationOutput sim, Dictionary<int, double> capacityMap)
        {
            var years = (int)Math.Ceiling((double)(sim.TimeSeries[0].Count / Stuff.HoursInYear));
            var flow = ParameterEvaluator.TransmissionCapacity(sim);
            for (int i = 0; i < years; i++)
            {
                capacityMap.Add(i, ParameterEvaluator.TransmissionCapacity(sim, 1, i * Stuff.HoursInYear, Stuff.HoursInYear) / flow);
            }
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
                new ExportSchemeInput()
                {
                    Scheme = ExportScheme.UnconstrainedSynchronized
                });
            ctrl.NodeFuncs.Clear();
            ctrl.NodeFuncs.Add("Clean nodes", s =>
            {
                var nodes = ConfigurationUtils.CreateNodes(s.Source, s.Offset);
                //ConfigurationUtils.SetupHomoStuff(nodes, s.Length, true, true, false);
                //ConfigurationUtils.SetupMegaStorage(nodes);
                return nodes;
            });
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
