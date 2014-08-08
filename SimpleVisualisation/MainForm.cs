using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DataItems;
using SimpleImporter;
using SimpleNetwork;
using SimpleNetwork.Interfaces;

namespace SimpleVisualisation
{
    public partial class MainForm : Form
    {

        private TimeSeriesControl timeSeriesControl;
        private ContourControl contourControl;

        public MainForm()
        {
            InitializeComponent();

            //CsvImporter.ResetDb();

            // Time manger start/interval MUST match time series!
            TimeManager.Instance().StartTime = new DateTime(2000, 1, 1);
            TimeManager.Instance().Interval = 60;
            var watch = new Stopwatch();
            watch.Start();

            var client = new MainAccessClient();
            var noCol = new NodeCollection(client.GetAllCountryData());
            var nodes = noCol.Nodes;
            var edges = new EdgeSet(nodes.Count);
            // For now, connect the nodes in a straight line.
            for (int i = 0; i < nodes.Count - 1; i++) edges.AddEdge(i, i + 1);
            var system = new NetworkSystem(nodes, edges);
            Console.WriteLine("System setup: " + watch.ElapsedMilliseconds);

            var gridParams = new GridScanParameters
            {
                MixingFrom = 0.35,
                MixingTo = 0.95,
                MixingSteps = 24,
                PenetrationFrom = 1.1,
                PenetrationTo = 1.25,
                PenetrationSteps = 15
            };

            DisplayContour(DoGridScan(gridParams, system, noCol));
        }

        public Tuple<double, double, bool>[,] DoGridScan(GridScanParameters gridParams, NetworkSystem sys, NodeCollection noCol)
        {
            var watch = new Stopwatch();

            return GridEvaluator.EvalDense(delegate(int[] idxs)
            {
                noCol.Mixing = gridParams.MixingFrom + gridParams.MixingStep*idxs[0];
                noCol.Penetration = gridParams.PenetrationFrom + gridParams.PenetrationStep*idxs[1];
                // Do simulation.
                watch.Restart();
                sys.Simulate(24*7*52, false);
                Console.WriteLine("Mix " + noCol.Mixing + "; Penetation " + noCol.Penetration + ": " +
                                  watch.ElapsedMilliseconds + ", " + (sys.Output.Success ? "SUCCESS" : "FAIL"));
                return new Tuple<double, double, bool>(noCol.Mixing, noCol.Penetration, sys.Output.Success);
            }, new[]{gridParams.MixingSteps, gridParams.PenetrationSteps});
        }

        #region Contour mapping

        private void DisplayContour(Tuple<double, double, bool>[,] grid)
        {
            if (contourControl == null) InitializeCountourControl();
            contourControl.Visible = true;
            if (timeSeriesControl != null) timeSeriesControl.Visible = false;

            contourControl.SetData(grid);
        }

        private void InitializeCountourControl()
        {
            contourControl = new ContourControl
            {
                Dock = DockStyle.Fill,
                Location = new System.Drawing.Point(0, 0),
                Name = "contourControl",
            };
            panel1.Controls.Add(contourControl);
        }

        #endregion

        #region Ts GUI mapping

        private void DisplayTs(SimulationOutput output)
        {
            if(timeSeriesControl == null) InitializeTsControl();
            timeSeriesControl.Visible = true;
            if (contourControl != null) contourControl.Visible = false;

            var allTs = output.SystemTimeSeries.ToList().Select(item => item.Value).ToList();
            foreach (var item in output.CountryTimeSeriesMap)
            {
                foreach (var ts in item.Value)
                {
                    ts.Name = item.Key + ", " + ts.Name;
                    allTs.Add(ts);
                }
            }
            timeSeriesControl.SetData(allTs);
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

        public class GridScanParameters
        {
            public double MixingFrom { get; set; }
            public double MixingTo { get; set; }
            public int MixingSteps { get; set; }
            public double MixingStep
            {
                get { return (MixingTo - MixingFrom)/MixingSteps; }
            }

            public double PenetrationTo { get; set; }
            public double PenetrationFrom { get; set; }
            public int PenetrationSteps { get; set; }
            public double PenetrationStep
            {
                get { return (PenetrationTo - PenetrationFrom) / PenetrationSteps; }
            }
        }

    }
}
