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

namespace SimpleVisualisation
{
    public partial class ContourControl : UserControl
    {
        public ContourControl()
        {
            InitializeComponent();
        }

        public void SetData(Tuple<double, double, bool>[,] grid)
        {
            // Construct new time series.
            var success = new Series("Success") {ChartType = SeriesChartType.Point, Color = Color.Green};
            var fail = new Series("Fail") {ChartType = SeriesChartType.Point, Color = Color.Red};
            foreach (var tuple in grid)
            {
                if (tuple.Item3) success.Points.AddXY(tuple.Item1, tuple.Item2);
                else fail.Points.AddXY(tuple.Item1, tuple.Item2);
            }

            chart.Series.Clear();
            chart.Series.Add(success);
            chart.Series.Add(fail);

            chart.ChartAreas[0].AxisY.Minimum = 1.10;
            chart.ChartAreas[0].AxisY.Maximum = 1.25;

            chart.ChartAreas[0].AxisX.Minimum = 0.35;
            chart.ChartAreas[0].AxisX.Maximum = 0.95;
        }
    }
}
