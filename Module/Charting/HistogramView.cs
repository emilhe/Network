using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using Utils.Statistics;

namespace Controls.Charting
{
    public partial class HistogramView : UserControl, ITimeSeriesView
    {

        public Chart MainChart { get { return chart; } }

        public HistogramView()
        {
            InitializeComponent();

            ChartUtils.StyleChart(MainChart);
            ChartUtils.EnableZooming(MainChart);
            MainChart.ChartAreas[0].AxisX.LabelStyle.Format = "0.0";

            MainChart.Series.Clear();
        }

        public void AddData(ITimeSeries ts)
        {
            var table = ts.ToDataBinTable();
            AddData(table, ts.Name);
        }

        public void AddData(DataBinTable table, string name)
        {
            var series = new Series { ChartType = SeriesChartType.Column, Name = name};
            for (int i = 0; i < table.Midpoints.Length; i++)
            {
                series.Points.AddXY(table.Midpoints[i], table.Values[i]);
            }
            MainChart.Series.Add(series);
            // Hacking.
            HackSeries(series);
            // Render axis to fit the LAST ts (for now).
            RenderAxis(table);
        }

        private void RenderAxis(DataBinTable table)
        {
            MainChart.ChartAreas[0].AxisX.Minimum = table.Midpoints[0] - table.BinSize;
            MainChart.ChartAreas[0].AxisX.Maximum = table.Midpoints[table.Midpoints.Length-1] + table.BinSize;
            MainChart.ChartAreas[0].AxisX.Interval = table.BinSize;
            MainChart.ChartAreas[0].AxisX.IntervalOffset = table.BinSize;

            MainChart.ChartAreas[0].AxisY.Minimum = 0;
            MainChart.ChartAreas[0].AxisY.Maximum =
                MainChart.Series.SelectMany(item => item.Points).SelectMany(item => item.YValues).Max()*1.1;
            if (MainChart.ChartAreas[0].AxisY.Maximum == 0) MainChart.ChartAreas[0].AxisY.Maximum = 1;
            MainChart.ChartAreas[0].AxisY.Interval =
                ChartUtils.CalcStepSize(
                    MainChart.ChartAreas[0].AxisY.Maximum - MainChart.ChartAreas[0].AxisY.Minimum, 10);
        }

        /// <summary>
        /// There's an issue with windows forms bar view; if only one value is present, the chart renders bad. This hack fixes it.
        /// </summary>
        /// <param name="series"> the series to hack </param>
        private void HackSeries(Series series)
        {
            if (series.Points.Count() > 1) return;

            var x = series.Points[0].XValue;
            series.Points.AddXY(x - 0.5, 0);
            series.Points.AddXY(x + 0.5, 0);
        }

    }
}
