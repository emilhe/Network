using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using BusinessLogic.Generators;
using Controls;
using Controls.Charting;
using BusinessLogic;
using SimpleImporter;

namespace Main
{
    public partial class MainForm : Form
    {

        private TimeSeriesControl _timeSeriesControl;
        private GroupHistogramView _histogramView;
        private ContourView _contourView;
        private PlotView _plotView;

        public MainForm()
        {
            InitializeComponent();

            // Time manger start/interval MUST match time series!
            TimeManager.Instance().StartTime = new DateTime(1979, 1, 1);
            TimeManager.Instance().Interval = 60;

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
            Configurations.CompareFlows(this);

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

            //var opt = new MixOptimizer(client.GetAllCountryData(TsSource.ISET));  
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
                Location = new System.Drawing.Point(0, 0),
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
                Location = new System.Drawing.Point(0, 0),
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
                Location = new System.Drawing.Point(0, 0),
                Name = "histogramView",
            };
            panel1.Controls.Add(_histogramView);
        }

        #endregion

    }
}
