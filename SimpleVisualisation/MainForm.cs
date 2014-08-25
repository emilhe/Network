using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DataItems;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using SimpleImporter;
using SimpleNetwork;
using SimpleNetwork.ExportStrategies;
using SimpleNetwork.ExportStrategies.DistributionStrategies;
using SimpleNetwork.Interfaces;
using SimpleNetwork.Utils;

namespace SimpleVisualisation
{
    public partial class MainForm : Form
    {

        private TimeSeriesControl timeSeriesControl;
        private ContourControl contourControl;
        private ContourControlOxy contourControlOxy;

        public MainForm()
        {
            InitializeComponent();

            // Time manger start/interval MUST match time series!
            TimeManager.Instance().StartTime = new DateTime(2000, 1, 1);
            TimeManager.Instance().Interval = 60;

            Configurations.CompareExportSchemes(this);

            //var data = ProtoStore.LoadEcnData();
            //var allBio = data.Where(item =>
            //    item.RowHeader.Equals("Biomass") &&
            //    item.ColumnHeader.Equals("Gross electricity generation") &&
            //    item.Year.Equals(2010)).Select(item => item.Value).Sum();
            //var allHydro = data.Where(item =>
            //    item.RowHeader.Equals("Hydropower") &&
            //    item.ColumnHeader.Equals("Gross electricity generation") &&
            //    item.Year.Equals(2010)).Select(item => item.Value).Sum();
            //var allHydroPump =
            //    data.Where(item =>
            //    item.RowHeader.Equals("Pumped storage hydropower") &&
            //    item.ColumnHeader.Equals("Gross electricity generation") &&
            //    item.Year.Equals(2010)).Select(item => item.Value).Sum(); 

            //var hest = 0;

            //var opt = new MixOptimizer(client.GetAllCountryData(TsSource.ISET));  
            //Console.WriteLine("System setup: " + watch.ElapsedMilliseconds);

            //opt.OptimizeIndividually();
            ////opt.ReadMixCahce();
            ////opt.OptimizeLocally();
            //var nodes = opt.Nodes;
            //var edges = new EdgeSet(nodes.Count);
            //// For now, connect the nodes in a straight line.
            //for (int i = 0; i < nodes.Count - 1; i++) edges.AddEdge(i, i + 1);
            //var system = new Simulation(nodes, edges);
            //for (var pen = 1.02; pen <= 1.10; pen += 0.0025)
            //{
            //    opt.SetPenetration(pen);
            //    system.Simulate(24 * 7 * 52);
            //    Console.WriteLine("Penetation " + pen + ", " + (system.Output.Success ? "SUCCESS" : "FAIL"));
            //}
            //DisplayTimeSeries(system.Output);

            //var edges = new EdgeSet(nodes.Count);
            //var mCtrl = new MixController(nodes);
            // For now, connect the nodes in a straight line.

            //for (int i = 0; i < nodes.Count - 1; i++) edges.AddEdge(i, i + 1);
            //var config = new NetworkModel(nodes, new CooperativeExportStrategy(), new BottomUpStrategy());
            //var system = new Simulation(config);
            //Console.WriteLine("System setup: " + watch.ElapsedMilliseconds);

        }

        #region Contour view

        public ContourControlOxy DisplayContourOxy()
        {
            if (contourControlOxy == null) InitializeContourControlOxy();
            contourControlOxy.Visible = true;
            if (timeSeriesControl != null) timeSeriesControl.Visible = false;
            if (contourControl != null) contourControl.Visible = false;
            return contourControlOxy;
        }

        private void InitializeContourControlOxy()
        {
            contourControlOxy = new ContourControlOxy
            {
                Dock = DockStyle.Fill,
                Location = new System.Drawing.Point(0, 0),
                Name = "contourControlOxy",
            };
            panel1.Controls.Add(contourControlOxy);
        }

        #endregion

        #region Ts GUI mapping

        public TimeSeriesControl DisplayTimeSeries()
        {
            if(timeSeriesControl == null) InitializeTsControl();
            timeSeriesControl.Visible = true;
            if (contourControl != null) contourControl.Visible = false;

            return timeSeriesControl;
        }

        private void InitializeTsControl()
        {
            timeSeriesControl = new TimeSeriesControl
            {
                Dock = DockStyle.Fill,
                Location = new System.Drawing.Point(0, 0),
                Name = "timeSeriesControl",
            };
            panel1.Controls.Add(timeSeriesControl);
        }

        #endregion

    }
}
