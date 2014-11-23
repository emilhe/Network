using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Controls.Charting
{
    public partial class CostView : UserControl
    {

        private static readonly Dictionary<string, Color> _mColors = new Dictionary<string, Color>
        {
            {"Wind", Color.MidnightBlue},
            {"Solar", Color.Gold},
            {"Backup", Color.DarkRed},
            {"Fuel", Color.Orange},
            {"Transmission", Color.Green}
        };

        public Chart MainChart { get { return chart; } }

        public CostView()
        {
            InitializeComponent();

            ChartUtils.StyleChart(MainChart);
            ChartUtils.EnableZooming(MainChart);

            chart.ChartAreas[0].AxisY.Title = "LCOE [€/MWh]";

            MainChart.Series.Clear();
        }

        public void AddData(Dictionary<string, double[]> data, double[] xValues)
        {
            var accSum =  new double[data.First().Value.Length];
            var allSeries = new List<Series>();
            foreach (var element in data)
            {
                var series = new Series(element.Key)
                {
                    BorderWidth = 5,
                    ChartType = SeriesChartType.Area                
                };
                if (_mColors.ContainsKey(element.Key)) series.Color = _mColors[element.Key];
                for (int i = 0; i < element.Value.Length; i++)
                {
                    accSum[i] += element.Value[i];
                    series.Points.AddXY( xValues[i], accSum[i]);
                }
                allSeries.Add(series);
            }
            allSeries.Reverse();

            foreach (var series in allSeries)
            {
                MainChart.Series.Add(series);
            }

            MainChart.ChartAreas[0].AxisX.Minimum = xValues.Min();
            MainChart.ChartAreas[0].AxisX.Maximum = xValues.Max();
            MainChart.ChartAreas[0].AxisX.Interval = ChartUtils.CalcStepSize(xValues.Max() - xValues.Min(), 10);
        }
    }
}
