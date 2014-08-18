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

            var client = new AccessClient();

            //var opt = new MixOptimizer(client.GetAllCountryData(TsSource.ISET));
            //Console.WriteLine("System setup: " + watch.ElapsedMilliseconds);

            //opt.OptimizeIndividually();
            ////opt.ReadMixCahce();
            ////opt.OptimizeLocally();
            //var nodes = opt.Nodes;
            //var edges = new EdgeSet(nodes.Count);
            //// For now, connect the nodes in a straight line.
            //for (int i = 0; i < nodes.Count - 1; i++) edges.AddEdge(i, i + 1);
            //var system = new NetworkSystem(nodes, edges);
            //for (var pen = 1.02; pen <= 1.10; pen += 0.0025)
            //{
            //    opt.SetPenetration(pen);
            //    system.Simulate(24 * 7 * 52);
            //    Console.WriteLine("Penetation " + pen + ", " + (system.Output.Success ? "SUCCESS" : "FAIL"));
            //}
            //DisplayTs(system.Output);

            var noCol = new NodeCollection(client.GetAllCountryData(TsSource.VE));
            var nodes = noCol.Nodes;
            var edges = new EdgeSet(nodes.Count);
            // For now, connect the nodes in a straight line.
            for (int i = 0; i < nodes.Count - 1; i++) edges.AddEdge(i, i + 1);
            var system = new NetworkSystem(nodes, edges);
            Console.WriteLine("System setup: " + watch.ElapsedMilliseconds);

            ContourStuff(system, noCol);
            //TsStuff(system, noCol);
        }

        public void TsStuff(NetworkSystem sys, NodeCollection noCol)
        {
            var watch = new Stopwatch();
            watch.Start();
            noCol.Penetration = 1.15;
            noCol.Mixing = 0.65;
            sys.Simulate(24*365);
            Console.WriteLine("Mix " + noCol.Mixing + "; Penetation " + noCol.Penetration + ": " +
                  watch.ElapsedMilliseconds + ", " + (sys.Output.Success ? "SUCCESS" : "FAIL"));
            DisplayTs(sys.Output);
        }

        public void ContourStuff(NetworkSystem sys, NodeCollection noCol)
        {
            var gridParams = new GridScanParameters
            {
                MixingFrom = 0.55,
                MixingTo = 0.75,
                MixingSteps = 20, //24
                PenetrationFrom = 1.04,
                PenetrationTo = 1.10,
                PenetrationSteps = 16//15
            };

            DisplayContour(DoGridScan(gridParams, sys, noCol));
        }

        public Tuple<double, double, bool>[,] DoGridScan(GridScanParameters gridParams, NetworkSystem sys, NodeCollection noCol)
        {
            var watch = new Stopwatch();
            var tuples = new Tuple<double, double, bool>[gridParams.PenetrationSteps, gridParams.MixingSteps];
            // Eval grid.
            var grid = GridEvaluator.EvalSparse(delegate(int[] idxs)
            {
                noCol.Penetration = gridParams.PenetrationFrom + gridParams.PenetrationStep*idxs[0];
                noCol.Mixing = gridParams.MixingFrom + gridParams.MixingStep * idxs[1];
                // Do simulation.
                watch.Restart();
                sys.Simulate(24*7*52, false);
                Console.WriteLine("Mix " + noCol.Mixing + "; Penetation " + noCol.Penetration + ": " +
                                  watch.ElapsedMilliseconds + ", " + (sys.Output.Success ? "SUCCESS" : "FAIL"));
                return sys.Output.Success; 
            }, new[]{gridParams.PenetrationSteps, gridParams.MixingSteps});
            // Map coordinates.
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    tuples[i, j] = new Tuple<double, double, bool>(gridParams.PenetrationFrom + gridParams.PenetrationStep*i,
                        gridParams.MixingFrom + gridParams.MixingStep*j, grid[i, j]);
                }
                
            }
            return tuples;
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
