using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BusinessLogic;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using BusinessLogic.Utils;
using Controls;
using Controls.Charting;
using Main.Configurations;
using Main.Documentation;
using Main.Figures;
using NUnit.Framework.Constraints;
using SimpleImporter;
using Utils;

namespace Main
{
    public partial class MainForm : Form
    {

        private TimeSeriesControl _timeSeriesControl;
        private GroupHistogramView _histogramView;
        private ContourView _contourView;
        private CostView _costView;
        private PlotView _plotView;

        public MainForm()
        {
            InitializeComponent();

            // Time manger start/interval MUST match time series!
            TimeManager.Instance().StartTime = new DateTime(1979, 1, 1);
            TimeManager.Instance().Interval = 60;

            //Article.ExportFigureData();

            UncSyncScheme.Bias = 0;
            var costCalc = new ParallelNodeCostCalculator(4) { CacheEnabled = false, Full = false };
            costCalc.SpawnCostCalc = () => new NodeCostCalculator(new ParameterEvaluator(new FullCore(1, () =>
            {
                var nodes = ConfigurationUtils.CreateNodesNew();
                ConfigurationUtils.SetupRealHydro(nodes);
                ConfigurationUtils.SetupRealBiomass(nodes);
                return nodes;
            }, "ReferenceNEW"))) { CacheEnabled = false };
            Optimization.Sequential(1, "ReferenceNEW" + "@unbiased", costCalc);

            //var nodes = ConfigurationUtils.CreateNodesNew();
            //ConfigurationUtils.SetupRealBiomass(nodes);
            //ConfigurationUtils.SetupRealHydro(nodes);

            //var genes = NodeGenesFactory.SpawnBeta(0.5,1,0);
            //foreach (var gene in genes)
            //{
            //    gene.Value.UpdateGamma(1);
            //    //gene.Value.BiomassFraction = 0.1;
            //    //gene.Value.HydroFraction = 0.2;
            //}
            //genes.Export().ToJsonFile(@"C:\Temp\genes.txt");

            Tables.PrintLinkInfo(3);

            //Tables.PrintOverviewTable();

            //foreach (var file in Directory.GetFiles(@"C:\Users\Emil\Dropbox\BACKUP\Python\sandbox\Layouts2"))
            //{
            //    if (file.Contains(".pdf")) continue;
            //    var data = FileUtils.FromJsonFile<NodeGenes>(file);
            //    data.Export().ToJsonFile(file);
            //}

            //return;

            //StorageAnalysis.StorageTransmission();

            //Figures.PlayGround.ExportChromosomeData();

            //var paths = new[]
            //        {
            //            //@"C:\Users\Emil\Dropbox\BACKUP\Python\data_prod\overviews\dataSync.txt",
            //            @"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\realReference.txt",
            //            @"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\real5h.txt",
            //            @"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\real35h.txt",
            //            @"C:\Users\Emil\Dropbox\BACKUP\Python\data_dev\overviews\real5h+35h.txt"              
            //        };
            //foreach (var path in paths)
            //{
            //    var blob =
            //        FileUtils.FromJsonFile<Dictionary<string, Dictionary<double, BetaWrapper>>>(path);
            //    Tables.PrintOverviewTable(blob);
            //    Console.WriteLine();
            //}

            //        var load = CountryInfo.GetMeanLoadSum();

            //var hours = 5; // Storage capacity in hours
            //var price = 1.68; // EUR per kWh (pretty low!)

            //var capacity = 5*load; // Storage capacity in GWh
            //var cost = capacity*price*1e6; // Storage cost in EUR
            //var scale = Costs.AnnualizationFactor(25)*load*Stuff.HoursInYear;
            //var lcoe = cost/scale;

            //var hest = 0;

            //CostAnalysis.BetaWithGenetic(this, new List<int> { 1 }, true);
            //CostAnalysis.BetaWithGenetic(this, new List<int> { 1, 2, 5 }, true);

            //for (int i = 0; i < 100; i++)
            //{
            //    Optimization.Cukoo(1, 25, i.ToString());
            //}

            //var genes = new NodeGenes[30];
            //for (int i = 0; i < 30; i++)
            //{
            //    genes[i] = FileUtils.FromJsonFile<NodeGenes>(string.Format(@"C:\proto\onshoreVEgeneticConstraintTransK=1{0}.txt", i));
            //    //Optimization.Cukoo(1, 25, i.ToString());
            //}
            //var chromosomes = genes.Select(item => new NodeChromosome(item)).ToArray();
            //var calc = new ParallelCostCalculator<NodeChromosome> { Full = true, Transmission = true };
            //calc.UpdateCost(chromosomes);
            //chromosomes.OrderBy(item => item.Cost).First().Genes.ToJsonFile((string.Format(@"C:\proto\onshoreVEgeneticConstraintTransK=1{0}.txt", "BEST")));
            //chromosomes.Select(item => item.Cost).ToArray().ToJsonFile(@"C:\proto\GAcostStats.txt");

            //Optimization.Genetic(1, 1000);
            //Optimization.Cukoo(1, 500);
            //ModelYearAnalysis.DetermineModelYears(this, true);

            //var oldOpt = FileUtils.FromJsonFile<NodeGenes>(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\onshoreVEgeneticConstraintTransK=1.txt");
            //Optimization.Genetic(1,25);
            //Optimization.Cukoo(1, 25);
            //Optimization.Cukoo(2, 25);
            //Optimization.Cukoo(3, 25);
            //ModelYearAnalysis.DetermineModelYears(this, true);

            //var oldOpt = FileUtils.FromJsonFile < NodeGenes>(@"C:\Users\Emil\Dropbox\Master Thesis\Layouts\onshoreVEgeneticConstraintTransK=1.txt");
            //var newOpt = FileUtils.FromJsonFile<NodeGenes>(@"C:\chromosomes\k=1.txt");

            //var calc = new NodeCostCalculator(new ParameterEvaluator(true));
            //var oldCost = calc.SystemCost(oldOpt, true);
            //var newCost = calc.SystemCost(newOpt, true);

            //var calc = new NodeCostCalculator(new ParameterEvaluator(false));
            //for (var k = 1; k < 4; k++)
            //{
            //    var betaCost = calc.SystemCost(NodeGenesFactory.SpawnBeta(1, 1, Stuff.FindBeta(k, 1e-3, 1)));
            //    Console.WriteLine("Beta cost at k = {0} is {1}", k, betaCost);
            //    var cfCost = calc.SystemCost(NodeGenesFactory.SpawnCfMax(1, 1, k));
            //    Console.WriteLine("CF cost at k = {0} is {1}", k, cfCost);
            //}

            //// CHECK THAT RESULTS ARE CONSISTENT
            //var config = FileUtils.FromJsonFile<ModelYearConfig>(@"C:\Users\Emil\Dropbox\Master Thesis\OneYearAlpha0.5to1Gamma0.5to2Local.txt");
            //var core = new ModelYearCore(config);
            //var param = new ParameterEvaluator(core);
            //var calc = new NodeCostCalculator(param);
            //var mCf = new NodeGenes(1,1);
            ////Console.WriteLine("Homo = " + calc.DetailedSystemCosts(mCf, true).ToDebugString());

            //var ctrl = (core.BeController as SimulationController);
            //ctrl.InvalidateCache = true;
            //ctrl.NodeFuncs.Clear();
            //ctrl.NodeFuncs.Add("6h storage", input =>
            //{
            //    var nodes = ConfigurationUtils.CreateNodesNew();
            //    ConfigurationUtils.SetupHomoStuff(nodes, 1, true, false, false);
            //    return nodes;
            //});

            //Console.WriteLine("Homo with 6h = " + calc.DetailedSystemCosts(mCf, true).ToDebugString());

            //// First, let's look at the current results.
            //var calc = new NodeCostCalculator(new ParameterEvaluator(true) { CacheEnabled = true });
            //var optima = new[]
            //{
            //    new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\localK=1solar25pct.txt")),
            //    new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\localK=1solar50pct.txt")),
            //    new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\localK=2offshore25pct.txt")),
            //    new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\localK=2offshore50pct.txt")),
            //    new NodeChromosome(
            //        FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\VE50cukooK=1@solar25pct.txt")),
            //    new NodeChromosome(
            //        FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\VE50cukooK=1@solar50pct.txt")),
            //    new NodeChromosome(
            //        FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\VE50cukooK=1@offshore25pct.txt")),
            //                        new NodeChromosome(
            //        FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\VE50cukooK=1@offshore50pct.txt")),
            //};
            //// 25% solar
            //calc.SolarCostModel = new ScaledSolarCostModel(0.25);
            //Print(optima[0], calc, "SEQ-solar25");
            //Print(optima[4], calc, "CS-solar25");
            //// 50% solar
            //calc.SolarCostModel = new ScaledSolarCostModel(0.50);
            //Print(optima[1], calc, "SEQ-solar50");
            //Print(optima[5], calc, "CS-solar50");
            //// 25% offshore
            //calc.SolarCostModel = new SolarCostModelImpl();
            //GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.25);
            //GenePool.ApplyOffshoreFraction(optima[2].Genes);
            //GenePool.ApplyOffshoreFraction(optima[6].Genes);
            //Print(optima[2], calc, "SEQ-offshore25");
            //Print(optima[6], calc, "CS-offshore25");
            //// 50% offshore
            //GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.50);
            //GenePool.ApplyOffshoreFraction(optima[3].Genes);
            //GenePool.ApplyOffshoreFraction(optima[7].Genes);
            //Print(optima[3], calc, "SEQ-offshore50");
            //Print(optima[7], calc, "CS-offshore50");

            //StorageAnalysis.ExportStorageReal(new List<double> {1});

            var hydroInfo = new Dictionary<string, HydroInfo>();
            var lines = File.ReadAllLines(@"C:\Users\Emil\Dropbox\Master Thesis\HydroDataExtended.csv");
            foreach (var line in lines)
            {
                var cells = line.Split(',');
                var inflowFile = string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\HydroInflow\Hydro_Inflow_{0}.csv",
                    cells[0]);
                var inflow = new double[365];
                // Load inflow pattern if present.
                if (File.Exists(inflowFile))
                {
                    var tsLines = File.ReadLines(inflowFile).ToArray();
                    for (int i = 0; i < inflow.Length; i++)
                    {
                        var str = tsLines[1 + i + 9 * 365];
                        inflow[i] += double.Parse(str.Split(',')[3]); // Take 2012 values
                        //for (int j = 9; j < 10; j++) 
                    }
                }
                hydroInfo.Add(CountryInfo.GetName(cells[0]), new HydroInfo
                {
                    ReservoirCapacity = double.Parse(cells[1]),
                    Capacity = double.Parse(cells[2]),
                    PumpCapacity = double.Parse(cells[3]),
                    InflowPattern = inflow
                });
            }
            var resRatio =
                hydroInfo.Where(item => item.Value.ReservoirCapacity >= 0 && item.Value.InflowPattern.Average() > 0)
                    .Select(item => item.Value.ReservoirCapacity / item.Value.InflowPattern.Average()).Average();
            var capRatio =
                hydroInfo.Where(item => item.Value.Capacity >= 0 && item.Value.InflowPattern.Average() > 0)
                    .Select(item => item.Value.Capacity / item.Value.InflowPattern.Average()).Average();
            var pumpRatio =
                hydroInfo.Where(item => item.Value.PumpCapacity >= 0 && item.Value.InflowPattern.Average() > 0)
                    .Select(item => item.Value.PumpCapacity / item.Value.InflowPattern.Average()).Average();
            foreach (var info in hydroInfo)
            {
                if (info.Value.ReservoirCapacity < 0)
                {
                    info.Value.ReservoirCapacity = info.Value.InflowPattern.Average() * resRatio;
                }
                if (info.Value.Capacity < 0)
                {
                    info.Value.Capacity = info.Value.InflowPattern.Average() * capRatio;
                    info.Value.Corrected = true;
                }
                if (info.Value.PumpCapacity < 0)
                {
                    info.Value.PumpCapacity = info.Value.InflowPattern.Average() * pumpRatio;
                    info.Value.Corrected = true;
                }
            }
            hydroInfo.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\HydroDataExtended2005Kies.txt");

            Tables.PrintHydroData();

            StorageAnalysis.OptimizeStuff();

            //var sample = new NodeChromosome(new NodeGenes(1, 1));
            //var calc = new NodeCostCalculator(new ParameterEvaluator(false) { CacheEnabled = false });
            //Print(sample, calc, "delta0tmp1");

            //var calc = new ParallelNodeCostCalculator()
            //{
            //    CacheEnabled = false,
            //    Transmission = true,
            //    Full = false
            //};
            //Optimization.Sequential(2, "Test0", calc);


            #region other

            //for (int index = 0; index < optima.Length; index++)
            //{
            //    var optimum = optima[index];
            //    var cost = calc.DetailedSystemCosts(optimum.Genes, true);
            //    Console.WriteLine("Cost is {0} at {1}", cost.ToDebugString(), index);
            //    Console.WriteLine("Combined cost is {0} at {1}", cost.Values.Sum(), index);
            //}

            //////Configurations.PlayGround.ShowTimeSeris(this);

            //////var files = Directory.GetFiles(@"C:\Users\Emil\Desktop\TestSolutions");
            //////var data = new Dictionary<string, List<List<double>>>();
            //////var meta = new Dictionary<string, int>();
            //////// Read data.
            //////foreach (var file in files)
            //////{
            //////    if (!file.Contains("steps")) continue;
            //////    var key = file.Split('@')[1].Split('-')[2];
            //////    if (!data.ContainsKey(key)) data.Add(key, new List<List<double>>());
            //////    if (!meta.ContainsKey(key))
            //////    {
            //////        var multi = key.Equals("cs") ? 2 : 1;
            //////        meta.Add(key, int.Parse(file.Split('@')[2])*multi);
            //////    }
            //////    data[key].Add(FileUtils.FromJsonFile<List<double>>(file));
            //////}

            //////data.ToJsonFile(@"C:\Users\Emil\IdeaProjects\Python\optimization\data.txt");
            //////meta.ToJsonFile(@"C:\Users\Emil\IdeaProjects\Python\optimization\meta.txt");

            //int n = 1000000;
            //var rnd = new Random();
            //var symLevy = new double[n];
            //var asymLevy = new double[n];
            //for (int i = 0; i < n; i++)
            //{
            //    symLevy[i] = rnd.NextLevy(1.5, 0);
            //    asymLevy[i] = rnd.NextLevy(0.5, 1);
            //}
            //symLevy.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\levy\sym1mio.txt");
            //asymLevy.ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\levy\asym1mio.txt");

            //for (int k = 1; k < 4; k++)
            //{
            //    Optimization.Cukoo(2, 25, "TEST-NEXT" + k);
            //}

            ////for (int k = 1; k < 4; k++)
            ////{
            ////    var key = string.Format(@"VE50cukooK={0}@TRANS10k.txt", k);
            ////    var opt = FileUtils.FromJsonFile<NodeGenes>(string.Format(@"C:\proto\{0}", key));
            ////    opt.Export().ToJsonFile(string.Format(@"C:\Users\Emil\Dropbox\Master Thesis\Python\chromosomes\{0}", key));
            ////}

            //var files = Directory.GetFiles(@"C:\Users\Emil\Desktop\TestSolutions");
            //var calc = new NodeCostCalculator(new ParameterEvaluator(false));
            //foreach (var file in files)
            //{
            //    var genes = FileUtils.FromJsonFile<NodeGenes>(file);
            //    var cost = calc.SystemCost(genes);
            //    Console.WriteLine("{0} has cost {1}", file, cost);
            //}

            //Console.WriteLine("Ratio = " + CountryInfo.SolarCf.Select(item => item.Value).Average()/CountryInfo.WindOnshoreCf.Select(item => item.Value).Average());
            //var opt =
            //    FileUtils.FromJsonFile<NodeGenes>(
            //        @"C:\Users\Emil\Dropbox\Master Thesis\Python\chromosomesTest\VE50cukooWithTransK=3TEST.txt");
            //opt.Export().ToJsonFile(@"C:\Users\Emil\Dropbox\Master Thesis\Python\chromosomesTest\VE50cukooWithTransK=3TESTexport.txt");

            //var keyTemplate = @"C:\Users\Emil\Dropbox\Master Thesis\Results\Statistics\VE50cukooWithTransK={0}cukoo{1}-500.txt";
            //var results = new NodeChromosome[50];

            //var k = 3;
            //for (int i = 0; i < 50; i++)
            //{
            //    results[i] = new NodeChromosome(FileUtils.FromJsonFile<NodeGenes>(string.Format(keyTemplate, k, i)));
            //}

            //var calc = new ParallelNodeCostCalculator { CacheEnabled = false, Full = false, Transmission = false };
            //calc.UpdateCost(results);
            //results = results.OrderBy(item => item.Cost).ToArray();

            //Console.WriteLine("Cost of best = {0}", results[0].Cost);
            //Console.WriteLine("Cost of 10 best avg. = {0}", results.Take(10).Select(item => item.Cost).Average());
            //Console.WriteLine("Cost of 25 best avg. = {0}", results.Take(25).Select(item => item.Cost).Average());
            //Console.WriteLine("Cost of 50 (all) avg. = {0}", results.Select(item => item.Cost).Average());

            //Figures.PlayGround.ExportChromosomeData();
            //Console.WriteLine("Chromosomes done...");
            //Figures.PlayGround.ExportMismatchData(new List<double> { 1, 2, 3 }, true);
            //Console.WriteLine("Mismatch done...");
            //Figures.PlayGround.ExportCostDetailsData(new List<double> { 1, 2, 3 }, true);
            //Console.WriteLine("Cost details done...");
            //Figures.PlayGround.ExportParameterOverviewData(new List<double> { 1, 2, 3 }, true);
            //Console.WriteLine("Parameter overview done...");

            //Console.WriteLine("All done...");

            //var avg = data.Last().Model.WindTimeSeries.Values.Average();
            //var hest = 2;

            //Tables.PrintLinkInfo();
            //Tables.PrintCapacityFactors();
            //Tables.PrintCostInfo();

            //var genes = new NodeGenes(1, 1, 2);

            //var costCalc = new NodeCostCalculator();
            //var ks = costCalc.SystemCost(new NodeGenes(0.5, 1, 2));
            //var bs = costCalc.SystemCost(new NodeGenes(0.5, 1, BusinessLogic.Utils.Utils.FindBeta(2, 1e-3, 1)));

            //Figures.PlayGround.ParameterOverviewChartGenetic(this, new List<int> { 1, 2, 5 }, true);
            //Figures.PlayGround.ParameterOverviewChart(this, new List<double> { 0.0, 1.0, 2.0, 4.0 }, true);

            //var chromosome = FileUtils.FromJsonFile<NodeGenes>(@"C:\proto\genetic.txt");
            //CostAnalysis.PlotShit(this, chromosome);

            //var test = new NodeGenes(0.5, 1, 0);
            //test = new NodeGenes(0.5, 1, 1);
            //test = new NodeGenes(0.5, 1, 2);
            //test = new NodeGenes(0.5, 1, 3);
            //test = new NodeGenes(0.5, 1, 4);

            //var raw = FileUtils.DictionaryFromFile<string, double>(@"C:\data\CFs.txt");
            //var clean = raw.ToDictionary(item => CountryInfo.GetName(item.Key), item => item.Value);
            //clean.ToFile(@"C:\Users\Emil\Desktop\CFsClean.txt");


            //CostAnalysis.CompareBeta(this, new List<int> { 1, 2, 4 });
            //CostAnalysis.BetaWithGenetic(this, new List<int> { 1, 2, 5 }, true);

            //var hest = 0;

            // TODO: Make small util method to determine highest beta for a  given 

            //Figures.PlayGround.ParameterOverviewChartGenetic(this, new List<double>{1,2,5});
            //Figures.PlayGround.ParameterOverviewChart(this, new List<double>{0,1,2,4}, true);
            //Optimization.Genetic();

            //M.BackupAnalysis.BackupPerCountry(this);

            //ModelYearAnalysis.ErrorAnalysis(this, true);
            //Optimization.SimulatedAnnealing();
            //CostAnalysis.VaryBeta(this, true, @"C:\proto\genetic1000.txt");

            //ModelYearAnalysis.BackupAnalysis(this);

            //        var data = ProtoStore.LoadEcnData();
            //        var allBio = data.Where(item =>
            //            item.RowHeader.Equals("Biomass") &&
            //            item.ColumnHeader.Equals("Gross electricity generation") &&
            //            item.Year.Equals(2010)).Select(item => item.Value).Sum();
            //        var allHydro = data.Where(item =>
            //            item.RowHeader.Equals("Hydropower") &&
            //            item.ColumnHeader.Equals("Gross electricity generation") &&
            //            item.Year.Equals(2010)).Select(item => item.Value).Sum();
            //        var allHydroPump =
            //            data.Where(item =>
            //            item.RowHeader.Equals("Pumped storage hydropower") &&
            //            item.ColumnHeader.Equals("Gross electricity generation") &&
            //            item.Year.Equals(2010)).Select(item => item.Value).Sum();
            //        var allHydroPump2 =
            //data.Where(item =>
            //item.RowHeader.Equals("Pumped storage hydropower") &&
            //item.ColumnHeader.Equals("Installed capacity") &&
            //item.Year.Equals(2010)).Select(item => item.Value).Sum();

            //Configurations.CompareSources(this);
            //Configurations.CompareSources(this);
            //Configurations.Flo(this);

            //Figures.DrawDistributions();
            //Configurations.BackupAnalysisNoLinks(this);
            //Configurations.BackupEnergyAbsoluteWithStorage(this);
            //Configurations.BackupEnergyAbsoluteWithStorage(this);

            //Configurations.CompareSources(this);
            //Configurations.CompareFlows(this);
            //Figures.FlowAnalysis(this);

            //Configurations.BackupAnalysisWithLinksWitDelta(this);
            //Figures.FlowAnalysisNext(this);
            //ChartUtils.SaveChart(_contourView.MainChart, 800, 400, @"C:\Users\xXx\Dropbox\Master Thesis\Notes\Figures\AverageVsYearly.png");

            //var test = new MainSetupControl
            //{
            //    Dock = DockStyle.Fill,
            //    Location = new System.Drawing.Point(0, 0),
            //    Name = "setupControl",
            //};
            //test.RunSimulation += (sender, args) =>
            //{
            //    var parameters = test.ModelParameters;

            //    var nodes = ConfigurationUtils.CreateNodesWithBackup(parameters.Source, parameters.Years);
            //    var model = new NetworkModel(nodes, parameters.ExportScheme);
            //    var simulation = new Simulation(model);
            //    var mCtrl = new MixController(nodes);
            //    var watch = new Stopwatch();
            //    watch.Start();
            //    mCtrl.SetPenetration(1.032);
            //    mCtrl.SetMix(0.66);
            //    mCtrl.TimeSeriesExecution();
            //    simulation.Simulate(8765 * parameters.Years);
            //    Console.WriteLine("Mix " + mCtrl.Mixes[0] + "; Penetation " + mCtrl.Penetrations[0] + ": " +
            //          watch.ElapsedMilliseconds + ", " + (simulation.Output.Failure ? "SUCCESS" : "FAIL"));
            //    test.MainPanel.Controls.Clear();
            //    timeSeriesControl = new TimeSeriesControl
            //    {
            //        Dock = DockStyle.Fill,
            //        Location = new System.Drawing.Point(0, 0),
            //        Name = "timeSeriesControl",
            //    };
            //    timeSeriesControl.SetData(simulation.Output);
            //    test.MainPanel.Controls.Add(timeSeriesControl);
            //};
            //panel1.Controls.Add(test);

            //Configurations.ShowTimeSeris(this);

            //       var data = ProtoStore.LoadEcnData();

            //        var allBio = data.Where(item =>
            //            item.RowHeader.Equals("Biomass") &&
            //            item.ColumnHeader.Equals("Gross electricity generation") &&
            //            item.Year.Equals(2010)).Select(item => item.Value).Sum();
            //        var allHydro = data.Where(item =>
            //            item.RowHeader.Equals("Hydropower") &&
            //            item.ColumnHeader.Equals("Gross electricity generation") &&
            //            item.Year.Equals(2010)).Select(item => item.Value).Sum();
            //        var allHydroPump =
            //            data.Where(item =>
            //            item.RowHeader.Equals("Pumped storage hydropower") &&
            //            item.ColumnHeader.Equals("Gross electricity generation") &&
            //            item.Year.Equals(2010)).Select(item => item.Value).Sum();

            //        foreach (var hydro in data.Where(item =>
            //            item.RowHeader.Equals("Hydropower") &&
            //            item.ColumnHeader.Equals("Gross electricity generation") &&
            //            item.Year.Equals(2010)))
            //        {
            //            Console.WriteLine("{0} : {1}",hydro.Country, hydro.Value);
            //        }

            //        Console.WriteLine("Now to the significant ones..");

            //        foreach (var hydro in data.Where(item =>
            //item.RowHeader.Equals("Hydropower") &&
            //item.ColumnHeader.Equals("Gross electricity generation") &&
            //item.Year.Equals(2010) && item.Value > data.Where(item0 =>
            //item0.RowHeader.Equals("Hydropower") &&
            //item0.ColumnHeader.Equals("Gross electricity generation") &&
            //item0.Year.Equals(2010)).Select(item1 => item1.Value).Max()*0.05))
            //        {
            //            Console.WriteLine("{0} : {1}", hydro.Country, hydro.Value);
            //        }

            //var hest = 0;

            //var opt = new MixOptimizer(client.GetAllCountryDataOld(TsSource.ISET));  
            //Console.WriteLine("System setup: " + watch.ElapsedMilliseconds);

            //opt.OptimizeIndividually();
            ////opt.ReadMixCahce();
            ////opt.OptimizeLocally();
            //var nodes = opt.Nodes;
            //var edges = new EdgeSet(nodes.Count);
            //// For now, connect the nodes in a straight line.
            //for (int i = 0; i < nodes.Count - 1; i++) edges.Connect(i, i + 1);
            //var system = new Simulation(nodes, edges);
            //for (var pen = 1.02; pen <= 1.10; pen += 0.0025)
            //{
            //    opt.SetPenetration(pen);
            //    system.Simulate(24 * 7 * 52);
            //    Console.WriteLine("Penetation " + pen + ", " + (system.Output.Failure ? "SUCCESS" : "FAIL"));
            //}
            //DisplayTimeSeries(system.Output);

            //var edges = new EdgeSet(nodes.Count);
            //var mCtrl = new MixController(nodes);
            // For now, connect the nodes in a straight line.

            //for (int i = 0; i < nodes.Count - 1; i++) edges.Connect(i, i + 1);
            //var config = new NetworkModel(nodes, new CooperativeExportStrategy(), new SkipFlowStrategy());
            //var system = new Simulation(config);
            //Console.WriteLine("System setup: " + watch.ElapsedMilliseconds);

            #endregion

        }

        static void Print(NodeChromosome optimum, NodeCostCalculator calc, string tag)
        {
            var cost = calc.DetailedSystemCosts(optimum.Genes);
            Console.WriteLine("Cost is {0} at {1}", cost.ToDebugString(), tag);
            Console.WriteLine("Combined cost is {0} at {1}", cost.Values.Sum(), tag);
        }

        #region Contour view

        public void Show<T>(T view) where T : Control
        {
            view.Dock = DockStyle.Fill;
            panel1.Controls.Add(view);
            foreach (Control control in panel1.Controls) control.Visible = false;
            view.Visible = true;
        }

        public CostView DisplayCost()
        {
            if (_costView == null) InitializeCostControl();
            _costView.Visible = true;
            if (_contourView != null) _contourView.Visible = false;
            if (_timeSeriesControl != null) _timeSeriesControl.Visible = false;
            if (_histogramView != null) _histogramView.Visible = false;
            if (_plotView != null) _plotView.Visible = false;

            return _costView;
        }

        private void InitializeCostControl()
        {
            _costView = new CostView
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Name = "CostView",
            };
            panel1.Controls.Add(_costView);
        }

        #endregion

        #region Contour view

        public ContourView DisplayContour()
        {
            if (_contourView == null) InitializeContourControl();
            _contourView.Visible = true;
            if (_timeSeriesControl != null) _timeSeriesControl.Visible = false;
            if (_histogramView != null) _histogramView.Visible = false;
            if (_plotView != null) _plotView.Visible = false;

            return _contourView;
        }

        private void InitializeContourControl()
        {
            _contourView = new ContourView
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Name = "ContourView",
            };
            panel1.Controls.Add(_contourView);
        }

        #endregion

        #region Plot view

        public PlotView DisplayPlot()
        {
            if (_plotView == null) InitializePlotControl();
            _plotView.Visible = true;
            if (_timeSeriesControl != null) _timeSeriesControl.Visible = false;
            if (_histogramView != null) _histogramView.Visible = false;
            if (_contourView != null) _contourView.Visible = false;
            return _plotView;
        }

        private void InitializePlotControl()
        {
            _plotView = new PlotView
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Name = "PlotView",
            };
            panel1.Controls.Add(_plotView);
        }

        #endregion

        #region Ts GUI mapping

        public TimeSeriesControl DisplayTimeSeries()
        {
            if (_timeSeriesControl == null) InitializeTsControl();
            _timeSeriesControl.Visible = true;
            if (_contourView != null) _contourView.Visible = false;
            if (_histogramView != null) _histogramView.Visible = false;
            if (_plotView != null) _plotView.Visible = false;

            return _timeSeriesControl;
        }

        private void InitializeTsControl()
        {
            _timeSeriesControl = new TimeSeriesControl
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Name = "timeSeriesControl",
            };
            panel1.Controls.Add(_timeSeriesControl);
        }

        #endregion

        #region Histogram GUI mapping

        public GroupHistogramView DisplayHistogram()
        {
            if (_histogramView == null) InitializeHiControl();
            _histogramView.Visible = true;
            if (_timeSeriesControl != null) _timeSeriesControl.Visible = false;
            if (_contourView != null) _contourView.Visible = false;
            if (_plotView != null) _plotView.Visible = false;

            return _histogramView;
        }

        private void InitializeHiControl()
        {
            _histogramView = new GroupHistogramView
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Name = "histogramView",
            };
            panel1.Controls.Add(_histogramView);
        }

        #endregion

    }
}
