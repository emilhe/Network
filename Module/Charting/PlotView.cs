using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BusinessLogic.Interfaces;

namespace Controls.Charting
{
    public partial class PlotView : UserControl
    {

        public Chart MainChart { get { return chart; } }

        public PlotView()
        {
            InitializeComponent();

            ChartUtils.StyleChart(MainChart);
            ChartUtils.EnableZooming(MainChart);

            MainChart.Series.Clear();
        }

        public void AddData(double[] x, double[] y, string name, bool addPoints = true, bool skipLine = false)
        {
            if (!skipLine)
            {
                var line = new Series(name) { ChartType = SeriesChartType.Line };
                for (int i = 0; i < x.Length; i++)
                {
                    line.Points.AddXY(x[i], y[i]);
                }
                chart.Series.Add(line);
            }

            if (addPoints)
            {
                var points = new Series(name + ", points") { ChartType = SeriesChartType.Point };
                for (int i = 0; i < x.Length; i++)
                {
                    points.Points.AddXY(x[i], y[i]);
                }
                chart.Series.Add(points);
            }

            RenderAxis();
        }

        public void AddData(Dictionary<double, double> values, string name)
        {
            // Construct new time series.
            var spline = new Series(name + ", line") {ChartType = SeriesChartType.Line};
            var points = new Series(name + ", points"){ ChartType = SeriesChartType.Point };
            foreach (var tsItem in values)
            {
                spline.Points.AddXY(tsItem.Key, tsItem.Value);
                points.Points.AddXY(tsItem.Key, tsItem.Value);
            }
            chart.Series.Add(spline);
            chart.Series.Add(points);

            RenderAxis();
        }

        private void RenderAxis()
        {
            var x = MainChart.Series.SelectMany(item => item.Points).Select(item => item.XValue);
            var y = MainChart.Series.SelectMany(item => item.Points).SelectMany(item => item.YValues);

            if (!x.Any())
            {
                MainChart.ChartAreas[0].AxisX.Minimum = 0;
                MainChart.ChartAreas[0].AxisX.Maximum = 0;
                MainChart.ChartAreas[0].AxisX.Interval = 1;
            }
            else
            {
                var limits = ChartUtils.CalcAxis(x);
                MainChart.ChartAreas[0].AxisX.Minimum = limits.Min;
                MainChart.ChartAreas[0].AxisX.Maximum = limits.Max;
                MainChart.ChartAreas[0].AxisX.Interval = limits.Tick;
            }
            if (!y.Any())
            {
                MainChart.ChartAreas[0].AxisY.Minimum = 0;
                MainChart.ChartAreas[0].AxisY.Maximum = 0;
                MainChart.ChartAreas[0].AxisY.Interval = 1;
            }
            else
            {
                var limits = ChartUtils.CalcAxis(y);
                MainChart.ChartAreas[0].AxisY.Minimum = limits.Min;
                MainChart.ChartAreas[0].AxisY.Maximum = limits.Max;
                MainChart.ChartAreas[0].AxisY.Interval = limits.Tick;
            }

            MainChart.ChartAreas[0].AxisY.IntervalOffset = MainChart.ChartAreas[0].AxisY.Interval;
            MainChart.ChartAreas[0].AxisX.IntervalOffset = MainChart.ChartAreas[0].AxisX.Interval;

            // Fail safe.
            if (MainChart.ChartAreas[0].AxisY.Minimum == MainChart.ChartAreas[0].AxisY.Maximum)
                MainChart.ChartAreas[0].AxisY.Maximum = MainChart.ChartAreas[0].AxisY.Minimum + 1;
            // Fail safe.
            if (MainChart.ChartAreas[0].AxisX.Minimum == MainChart.ChartAreas[0].AxisX.Maximum)
                MainChart.ChartAreas[0].AxisX.Maximum = MainChart.ChartAreas[0].AxisX.Minimum + 1;
        }

    }
}
