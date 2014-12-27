using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Controls.Charting;

namespace Controls.Article
{
    public partial class ParameterOverviewChart : UserControl
    {

        public Chart MainChart { get { return chart; } }

        private readonly Dictionary<double, Color> _mColors = new Dictionary<double, Color>();

        public ParameterOverviewChart()
        {
            InitializeComponent();
            SetupChartAreas();

            ChartUtils.StyleChart(chart);
        }

        public void AddData(string key, List<BetaWrapper> values, bool dispK)
        {
            foreach (var value in values) AddData(key, value, dispK);
        }

        public void AddData(string key, BetaWrapper value, bool dispK)
        {
            // Choose color.
            var first = false;
            if (!_mColors.ContainsKey(value.Beta))
            {
                _mColors.Add(value.Beta, ColorController.NextColor());
                first = true;
            }
            var color = _mColors[value.Beta];

            // Construct Beta curve.         
            var series = new Series
            {
                Name = string.Format("β = {0}, {1}", value.Beta, key),
                LegendText = dispK ? string.Format("K = {0}", value.K) : string.Format("β = {0}", value.Beta.ToString("0.00")),
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                ChartArea = key,
                Color = color,
                IsVisibleInLegend = first
            };
            if (value.BetaY != null)
            {
                for (int i = 0; i < value.BetaX.Length; i++)
                {
                    var point = new DataPoint();
                    point.SetValueXY(value.BetaX[i], value.BetaY[i]);
                    point.ToolTip = string.Format("{0}, {1}", value.BetaX[i], value.BetaY[i]);
                    series.Points.Add(point);
                }
            }
            chart.Series.Add(series);

            // K curve.
            if (value.MaxCfX != null)
            {
                var spline = new Series(value.GeneticLabel +  ", " + key + "??")
                {
                    ChartType = SeriesChartType.Line,
                    BorderDashStyle = ChartDashStyle.Dash,
                    Color = color,
                    BorderWidth = 3,
                    ChartArea = key,
                    IsVisibleInLegend = false
                };
                for (int i = 0; i < value.MaxCfX.Length; i++)
                {
                    var point = new DataPoint();
                    point.SetValueXY(value.MaxCfX[i], value.MaxCfY[i]);
                    point.ToolTip = string.Format("{0}, {1}", value.MaxCfX[i], value.MaxCfY[i]);
                    spline.Points.Add(point);
                }
                chart.Series.Add(spline);
            }

            // Genetic point.
            if (!value.GeneticY.Equals(0))
            {
                var points = new Series(value.GeneticLabel + ", " + key)
                {
                    ChartType = SeriesChartType.Point,
                    Color = color,
                    MarkerSize = 10,
                    MarkerStyle = MarkerStyle.Circle,
                    ChartArea = key,
                    IsVisibleInLegend = false
                };
                points.Points.AddXY(value.GeneticX, value.GeneticY);
                chart.Series.Add(points);
            }

            //// Genetic TC point.
            //if (!value.MaxCfY.Equals(0))
            //{
            //    var points = new Series(value.LabelK + ", " + key)
            //    {
            //        ChartType = SeriesChartType.Point,
            //        Color = color,
            //        MarkerSize = 10,
            //        MarkerStyle = MarkerStyle.Diamond,
            //        ChartArea = key,
            //        IsVisibleInLegend = false
            //    };
            //    points.Points.AddXY(value.CustomX, value.CustomY);
            //    chart.Series.Add(points);
            //}

            RenderAxis(series.ChartArea);
        }

        //public void AddData(string key, double beta, double[] x, double[] y)
        //{
        //    var series = new Series
        //    {
        //        Name = string.Format("β = {0}, {1}", beta, key),
        //        ChartType = SeriesChartType.Line,
        //        BorderWidth = 2,
        //        ChartArea = key,
        //    };

        //    // Do beta mapping.
        //    if (_mColors.ContainsKey(beta))
        //    {
        //        series.IsVisibleInLegend = false;
        //        series.Color = _mColors[beta];
        //    }
        //    else
        //    {
        //        series.LegendText = string.Format("β = {0}", beta);
        //        series.Color = ColorController.NextColor();
        //        _mColors.Add(beta, series.Color);
        //    }

        //    for (int i = 0; i < x.Length; i++)
        //    {
        //        series.Points.AddXY(x[i], y[i]);
        //    }

        //    chart.Series.Add(series);
        //    RenderAxis(series.ChartArea);
        //}

        //public void AddData(string key, double beta, double x, double y)
        //{
        //    var series = new Series
        //    {
        //        Name = string.Format("β = {0}, {1} GA", beta.ToString("0.00"), key),
        //        ChartType = SeriesChartType.Point,
        //        MarkerStyle = MarkerStyle.Circle,
        //        MarkerSize = 8,
        //        ChartArea = key,
        //    };

        //    // Do beta mapping.
        //    if (_mColors.ContainsKey(beta))
        //    {
        //        series.IsVisibleInLegend = false;
        //        series.Color = _mColors[beta];
        //    }
        //    else
        //    {
        //        series.LegendText = string.Format("β = {0}", beta);
        //        series.Color = ColorController.NextColor();
        //        _mColors.Add(beta, series.Color);
        //    }

        //    series.Points.AddXY(x, y);

        //    chart.Series.Add(series);
        //    RenderAxis(series.ChartArea);
        //}

        private void SetupChartAreas()
        {
            chart.ChartAreas.Clear();
            chart.Series.Clear();

            // Setup individual chart areas.
            var beArea = new ChartArea("BE") { AxisY = { Title = "E^B" } };
            chart.ChartAreas.Add(beArea);
            var cfArea = new ChartArea("CF") { AxisY = { Title = "CF" } };
            chart.ChartAreas.Add(cfArea);
            var bcArea = new ChartArea("BC") { AxisY = { Title = "K^B" } };
            chart.ChartAreas.Add(bcArea);
            var tcArea = new ChartArea("TC") { AxisY = { Title = "K^T" } };
            chart.ChartAreas.Add(tcArea);
            // Common stuff.
            foreach (var chartArea in chart.ChartAreas)
            {
                chartArea.AxisX.Title = "α";
                chartArea.AxisX.Minimum = 0;
                chartArea.AxisX.Maximum = 1;
                chartArea.AxisX.Interval = 0.2;
            }
        }

        private void RenderAxis(string area)
        {
            var y = MainChart.Series.Where(item => item.ChartArea.Equals(area)).SelectMany(item => item.Points).SelectMany(item => item.YValues);

            if (!y.Any())
            {
                MainChart.ChartAreas[area].AxisY.Minimum = 0;
                MainChart.ChartAreas[area].AxisY.Maximum = 0;
                MainChart.ChartAreas[area].AxisY.Interval = 1;
            }
            else
            {
                var limits = ChartUtils.CalcAxis(y);
                MainChart.ChartAreas[area].AxisY.Minimum = limits.Min;
                MainChart.ChartAreas[area].AxisY.Maximum = limits.Max;
                MainChart.ChartAreas[area].AxisY.Interval = limits.Tick;
            }
                
            MainChart.ChartAreas[area].AxisY.IntervalOffset = MainChart.ChartAreas[0].AxisY.Interval;
        }
    }
}
