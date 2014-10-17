using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Controls.Charting
{
    public partial class ContourView : UserControl
    {

        public Chart MainChart { get { return chart; } }

        //private readonly List<Color> _mColors = new List<Color>();

        private static readonly Color[] _mColors =
        {
            Color.LightSkyBlue, Color.LightGreen, Color.LightCoral, Color.LightGoldenrodYellow
        };

        public ContourView()
        {
            InitializeComponent();

            ChartUtils.StyleChart(MainChart);
            ChartUtils.EnableZooming(MainChart);

            //foreach (var colorValue in Enum.GetValues(typeof(KnownColor)))
            //{
            //    _mColors.Add(Color.FromKnownColor((KnownColor)colorValue));   
            //}

            MainChart.Series.Clear();
        }

        public void AddData(double[] rows, double[] columns, bool[,] data, string name)
        {
            var series = new Series(name)
            {
                ChartType = SeriesChartType.FastLine,
                Color = _mColors[MainChart.Series.Count % _mColors.Count()],
                BorderWidth = 5
            };
            MapData(data, rows, columns, series.Points);
            MainChart.Series.Add(series);

            MainChart.ChartAreas[0].AxisX.Minimum = columns[0];
            MainChart.ChartAreas[0].AxisX.Maximum = columns[columns.Length-1];
            MainChart.ChartAreas[0].AxisX.Interval = ChartUtils.CalcStepSize(columns[columns.Length - 1] - columns[0], 10);
            MainChart.ChartAreas[0].AxisY.Minimum = rows[0];
            MainChart.ChartAreas[0].AxisY.Maximum = rows[rows.Length - 1];
            MainChart.ChartAreas[0].AxisY.Interval = ChartUtils.CalcStepSize(rows[rows.Length - 1] - rows[0], 10);

            MainChart.ChartAreas[0].AxisY.IntervalOffset = MainChart.ChartAreas[0].AxisY.Interval;
            MainChart.ChartAreas[0].AxisX.IntervalOffset = MainChart.ChartAreas[0].AxisX.Interval;
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
