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
            var watch = new Stopwatch();
            watch.Start();

            // First do ISET data.
            var nodes0 = ConfigurationUtils.CreateNodes(TsSource.ISET);
            var model0 = new NetworkModel(nodes0, new CooperativeExportStrategy(), new BottomUpStrategy());
            var simulation0 = new Simulation(model0);
            var mCtrl0 = new MixController(nodes0);

            var nodes1 = ConfigurationUtils.CreateNodes(TsSource.VE);
            var model1 = new NetworkModel(nodes1, new CooperativeExportStrategy(), new BottomUpStrategy());
            var simulation1 = new Simulation(model1);
            var mCtrl1 = new MixController(nodes1);

            var gridParams = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 20, //24
                PenetrationFrom = 1.00,
                PenetrationTo = 1.15,
                PenetrationSteps = 15//15
            };

            DisplayContourOxy();

            var gridISET = DoGridScan(gridParams, simulation0, mCtrl0);
            var gridVE = DoGridScan(gridParams, simulation1, mCtrl1);
            contourControlOxy.AddData(gridParams.Rows, gridParams.Columns, MapGrids(new List<bool[,]> {gridISET, gridVE}));

            //ContourStuff(simulation, mCtrl);

            // RUN CONFIGURATION HERE //
            //var simulation = Configurations.Optimization(nodes, mCtrl);

            //ConfigurationUtils.SetupNodes(nodes);
            //var model = new NetworkModel(nodes, new CooperativeExportStrategy(), new BottomUpStrategy());
            //var simulation = new Simulation(model);
            //mCtrl.SetPenetration(1.033);
            //mCtrl.SetMix(0.6);
            //mCtrl.Execute();

            // CHOOSE VIEW HERE //
            //TsStuff(simulation, mCtrl);
            //ContourStuff(system, noCol);

            //var data = ProtoStore.LoadEcnData();

            //var relevantData = data.Where(item =>
            //    item.RowHeader.Equals("Hydropower") && // Pumped storage hydropower
            //    item.ColumnHeader.Equals("Gross electricity generation") && // Installed capacity
            //    item.Year.Equals(2010)); // What year should we use?

            //var sum = relevantData.Select(item => item.Value).Sum();


            //Console.WriteLine("Time to load all parameters {0} from ECN data.", watch.ElapsedMilliseconds);

            //var client = new AccessClient();
            //var nodes = client.GetAllCountryData(TsSource.ISET);

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
            //DisplayTs(system.Output);

            //var edges = new EdgeSet(nodes.Count);
            //var mCtrl = new MixController(nodes);

            //var data = ProtoStore.LoadEcnData();
            //Utils.SetupNodesFromEcnData(nodes, data);
            //var allBio =
            //    nodes.SelectMany(item => item.Storages.Values)
            //        .Where(item => item.Name.Equals("Biomass"))
            //        .Select(item => item.Capacity)
            //        .Sum();
            //var allHydroPump =
            //    nodes.SelectMany(item => item.Storages.Values)
            //        .Where(item => item.Name.Equals("Pumped storage hydropower"))
            //        .Select(item => item.Capacity)
            //        .Sum();
            //var allHydro =
            //    nodes.SelectMany(item => item.Storages.Values)
            //        .Where(item => item.Name.Equals("Hydropower"))
            //        .Select(item => item.Capacity)
            //        .Sum();
            // For now, connect the nodes in a straight line.

            //for (int i = 0; i < nodes.Count - 1; i++) edges.AddEdge(i, i + 1);
            //var config = new NetworkModel(nodes, new CooperativeExportStrategy(), new BottomUpStrategy());
            //var system = new Simulation(config);
            //Console.WriteLine("System setup: " + watch.ElapsedMilliseconds);

        }

        public void TsStuff(Simulation sys, MixController mCtrl)
        {
            var watch = new Stopwatch();
            watch.Start();
            mCtrl.SetPenetration(1.033);
            mCtrl.SetMix(0.66);
            mCtrl.Execute();
            sys.Simulate(24*365);
            Console.WriteLine("Mix " + 0.66 + "; Penetation " + 1.033 + ": " +
                  watch.ElapsedMilliseconds + ", " + (sys.Output.Success ? "SUCCESS" : "FAIL"));
            DisplayTs(sys.Output);
        }

        public void ContourStuff(Simulation sys, MixController mCtrl)
        {
            DisplayContourOxy();

            var gridParams = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 40, //24
                PenetrationFrom = 1.50,
                PenetrationTo = 1.60,
                PenetrationSteps = 10//15
            };

            var grid = DoGridScan(gridParams, sys, mCtrl);
            //contourControlOxy.AddData(gridParams.Rows, gridParams.Columns, MapGrid(grid));            
        }

        public bool[,] DoGridScan(GridScanParameters gridParams, Simulation sys, MixController mCtrl)
        {
            var watch = new Stopwatch();
            // Eval grid.
            return GridEvaluator.EvalSparse(delegate(int[] idxs)
            {
                var pen = gridParams.PenetrationFrom + gridParams.PenetrationStep*idxs[0];
                var mix = gridParams.MixingFrom + gridParams.MixingStep*idxs[1];
                mCtrl.SetMix(mix);
                mCtrl.SetPenetration(pen);
                mCtrl.Execute();
                // Do simulation.
                watch.Restart();
                sys.Simulate(24*7*52, false);
                Console.WriteLine("Mix " + mix + "; Penetation " + pen + ": " +
                                  watch.ElapsedMilliseconds + ", " + (sys.Output.Success ? "SUCCESS" : "FAIL"));
                return sys.Output.Success;
            }, new[] {gridParams.PenetrationSteps, gridParams.MixingSteps});
        }

        #region Contour mapping

        //private void DisplayContour(Tuple<double, double, bool>[,] grid)
        //{
        //    if (contourControl == null) InitializeCountourControl();
        //    contourControl.Visible = true;
        //    if (timeSeriesControl != null) timeSeriesControl.Visible = false;
        //    if (contourControlOxy != null) contourControlOxy.Visible = false;

        //    contourControl.SetData(grid);
        //}

        private void DisplayContourOxy()
        {
            if (contourControlOxy == null) InitializeContourControlOxy();
            contourControlOxy.Visible = true;
            if (timeSeriesControl != null) timeSeriesControl.Visible = false;
            if (contourControl != null) contourControl.Visible = false;
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

        /// <summary>
        /// It is assumed that the grid are the same size.
        /// </summary>
        private double[,] MapGrids(List<bool[,]> grid, double value = 1)
        {
            var result = new double[grid[0].GetLength(0), grid[0].GetLength(1)];
            for (int i = 0; i < grid[0].GetLength(0); i++)
            {
                for (int j = 0; j < grid[0].GetLength(1); j++)
                {
                    for (int k = 0; k < grid.Count; k++)
                    {
                        if (grid[k][i, j]) result[i, j] = k+1;                        
                    }
                }
            }
            return result;
        }

        //private void InitializeCountourControl()
        //{
        //    contourControl = new ContourControl
        //    {
        //        Dock = DockStyle.Fill,
        //        Location = new System.Drawing.Point(0, 0),
        //        Name = "contourControl",
        //    };
        //    panel1.Controls.Add(contourControl);
        //}

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

            public double[] Rows
            {
                get
                {
                    var result = new double[MixingSteps];
                    for (int i = 0; i < MixingSteps; i++)
                    {
                        result[i] = MixingFrom + MixingStep * i;
                    }
                    return result;
                }
            }
            public double[] Columns
            {
                get
                {
                    var result = new double[PenetrationSteps];
                    for (int i = 0; i < PenetrationSteps; i++)
                    {
                        result[i] = PenetrationFrom + PenetrationStep * i;
                    }
                    return result;
                }
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
