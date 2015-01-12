using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusinessLogic;
using BusinessLogic.Cost;
using BusinessLogic.Interfaces;
using BusinessLogic.Utils;
using Controls;
using Controls.Charting;
using Main.Configurations;
using Main.Documentation;
using Main.Figures;
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

            //CostAnalysis.BetaWithGenetic(this, new List<int> { 1 }, true);
            //CostAnalysis.BetaWithGenetic(this, new List<int> { 1, 2, 5 }, true);

            for (int i = 0; i < 100; i++)
            {
                Optimization.Cukoo(1, 25, i.ToString());
            }

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
            //var newOpt = FileUtils.FromJsonFile<NodeGenes>(@"C:\chromosomes\k=1.txt");

            //var calc = new NodeCostCalculator(new ParameterEvaluator(true));
            //var oldCost = calc.SystemCost(oldOpt, true);
            //var newCost = calc.SystemCost(newOpt, true);

            Figures.PlayGround.ExportChromosomeData();
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
            //    var model = new NetworkModel(nodes, parameters.ExportStrategy);
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
            if(_timeSeriesControl == null) InitializeTsControl();
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
