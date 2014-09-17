using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BusinessLogic.Interfaces;

namespace Controls.Charting
{
    public partial class TimeSeriesView : UserControl, ITimeSeriesView
    {

        public Chart MainChart { get { return chart; } }

        public TimeSeriesView()
        {
            InitializeComponent();

            ChartUtils.StyleChart(MainChart);
            ChartUtils.EnableZooming(MainChart);

            MainChart.Series.Clear();
        }

        public void AddData(ITimeSeries ts)
        {
            // Construct new time series.
            var series = new Series(ts.Name);
            foreach (var tsItem in ts)
            {
                series.Points.AddXY(tsItem.TimeStamp, tsItem.Value);
                series.ChartType = SeriesChartType.FastLine;
            }
            chart.Series.Add(series);

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
                var xMin = x.Min();
                var xMax = x.Max();
                var xRange = xMax - xMin;
                MainChart.ChartAreas[0].AxisX.Minimum = Math.Floor(xMin - Math.Abs(xRange) * 0.05);
                MainChart.ChartAreas[0].AxisX.Maximum = Math.Ceiling(xMax + Math.Abs(xRange) * 0.05);
                MainChart.ChartAreas[0].AxisX.Interval =
                    ChartUtils.CalcStepSize(
                        MainChart.ChartAreas[0].AxisX.Maximum - MainChart.ChartAreas[0].AxisX.Minimum, 10);   
            }
            if (!y.Any())
            {
                MainChart.ChartAreas[0].AxisY.Minimum = 0;
                MainChart.ChartAreas[0].AxisY.Maximum = 0;
                MainChart.ChartAreas[0].AxisY.Interval = 1;
            }
            else
            {
                var yMin = y.Min();
                var yMax = y.Max();
                var yRange = yMax - yMin;
                MainChart.ChartAreas[0].AxisY.Minimum = Math.Floor(yMin - Math.Abs(yRange) * 0.05);
                MainChart.ChartAreas[0].AxisY.Maximum = Math.Ceiling(yMax + Math.Abs(yRange) * 0.05);
                MainChart.ChartAreas[0].AxisY.Interval =
                    ChartUtils.CalcStepSize(
                        MainChart.ChartAreas[0].AxisY.Maximum - MainChart.ChartAreas[0].AxisY.Minimum, 10);
            }

            // Fail safe.
            if (MainChart.ChartAreas[0].AxisY.Minimum == MainChart.ChartAreas[0].AxisY.Maximum)
                MainChart.ChartAreas[0].AxisY.Maximum = MainChart.ChartAreas[0].AxisY.Minimum + 1;
            // Fail safe.
            if (MainChart.ChartAreas[0].AxisX.Minimum == MainChart.ChartAreas[0].AxisX.Maximum)
                MainChart.ChartAreas[0].AxisX.Maximum = MainChart.ChartAreas[0].AxisX.Minimum + 1;
        }

    }
}
