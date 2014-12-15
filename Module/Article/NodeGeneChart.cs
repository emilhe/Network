using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BusinessLogic.Cost;
using Controls.Charting;
using Utils;

namespace Controls.Article
{
    public partial class NodeGeneChart : UserControl
    {

        public Chart MainChart { get { return chart; } }

        public NodeGeneChart()
        {
            InitializeComponent();
            ChartUtils.StyleChart(chart);
            ChartUtils.EnableZooming(chart);
        }

        public void SetData(NodeGenes[] geneCollection, bool showDash = true, bool log = false)
        {
            chart.Series.Clear();

            var wind = new Series { Color = Color.DarkBlue, ChartType = SeriesChartType.StackedColumn };
            var solar = new Series { Color = Color.Orange, ChartType = SeriesChartType.StackedColumn };
            int i = 0;
            foreach (var genes in geneCollection)
            {
                int j = 0;
                foreach (var gene in genes)
                {
                    if (i == 0)
                    {
                        chart.ChartAreas[0].AxisX.CustomLabels.Add(j, j + 1, CountryInfo.GetShortAbbrev(gene.Key));
                    }
                    var spacing = 1 / ((double)geneCollection.Length + 1);
                    if (log)
                    {
                        var val = Math.Log10(gene.Value.Gamma * gene.Value.Alpha);
                        if (!Double.IsInfinity(val)) wind.Points.AddXY(j + spacing * (i + 1), Math.Log10(gene.Value.Gamma * gene.Value.Alpha));
                        val = Math.Log10(gene.Value.Gamma * (1 - gene.Value.Alpha));
                        if (!Double.IsInfinity(val))
                            solar.Points.AddXY(j + spacing * (i + 1), Math.Log10(gene.Value.Gamma * (1 - gene.Value.Alpha)));
                    }
                    else
                    {
                        wind.Points.AddXY(j + spacing * (i + 1), gene.Value.Gamma * gene.Value.Alpha);
                        solar.Points.AddXY(j + spacing * (i + 1), gene.Value.Gamma * (1 - gene.Value.Alpha));
                    }
                    j++;
                }
                i++;
            }
            chart.Series.Add(wind);
            chart.Series.Add(solar);

            // Remove legends.
            chart.Legends.Clear();

            // Remove series spacing.
            foreach (Series series in chart.Series)
            {
                if (geneCollection.Length == 1) break;
                series["PointWidth"] = "1";
            }

            // ADJUST ANGLE HERE!
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = 0;

            RenderAxis();

            if (!showDash) return;

            // Add norm line.
            var norm = new Series { Color = Color.Black, ChartType = SeriesChartType.Line, BorderDashStyle = ChartDashStyle.Dash, BorderWidth = 3 };
            var xValues = chart.Series.SelectMany(item => item.Points).Select(item => item.XValue);
            norm.Points.AddXY(0, 1);
            norm.Points.AddXY(geneCollection[0].Count, 1);
            chart.Series.Add(norm);
        }

        private void RenderAxis()
        {
            MainChart.ChartAreas[0].AxisY.Minimum = 0;
            //MainChart.ChartAreas[0].AxisY.Minimum = Math.Min(0, Math.Floor(MainChart.Series.SelectMany(item => item.Points).SelectMany(item => item.YValues).Min()));
            MainChart.ChartAreas[0].AxisY.Maximum =
                Math.Ceiling(MainChart.Series.SelectMany(item => item.Points).SelectMany(item => item.YValues).Max());
            MainChart.ChartAreas[0].AxisY.Interval = 1;
        }

    }
}
