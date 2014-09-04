using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Controls.Charting
{
    public partial class ContourView : UserControl
    {

        public Chart MainChart { get { return chart; } }

        private static readonly Color[] Colors =
        {
            Color.LightSkyBlue, Color.LightGreen, Color.LightCoral, Color.LightGoldenrodYellow
        };

        public ContourView()
        {
            InitializeComponent();

            ChartUtils.StyleChart(MainChart);
            ChartUtils.EnableZooming(MainChart);

            MainChart.Series.Clear();
        }

        public void AddData(double[] rows, double[] columns, bool[,] data, string name)
        {
            var series = new Series(name)
            {
                ChartType = SeriesChartType.FastLine, 
                Color = Colors[MainChart.Series.Count],
                BorderWidth = 5
            };
            MapData(data, rows, columns, series.Points);
            MainChart.Series.Add(series);

            MainChart.ChartAreas[0].AxisX.Minimum = columns[0];
            MainChart.ChartAreas[0].AxisX.Maximum = columns[columns.Length-1];
            MainChart.ChartAreas[0].AxisX.Interval = ChartUtils.CalcStepSize(columns[columns.Length - 1] - columns[0], 10);
            MainChart.ChartAreas[0].AxisY.Minimum = rows[0];
            MainChart.ChartAreas[0].AxisY.Maximum = rows[rows.Length - 1];
            MainChart.ChartAreas[0].AxisY.Interval = ChartUtils.CalcStepSize(rows[rows.Length - 1] - rows[0], 5);

        }

        private void MapData(bool[,] data, double[] rows, double[] columns, DataPointCollection points)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    if (!data[i, j]) continue;
                    points.AddXY(columns[i], rows[j]);
                    break;
                }
            }
        }
    }
}
