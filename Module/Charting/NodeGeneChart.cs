using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BusinessLogic.Cost;
using Utils;

namespace Controls.Charting
{
    public partial class NodeGeneChart : UserControl
    {

        public Chart MainChart { get { return chart1; } }

        public NodeGeneChart()
        {
            InitializeComponent();
            ChartUtils.StyleChart(chart1);
            ChartUtils.EnableZooming(chart1);
        }

        public void SetData(NodeGenes[] geneCollection)
        { 
            chart1.Series.Clear();
            
            var wind = new Series {Color = Color.DarkBlue, ChartType = SeriesChartType.StackedColumn};
            var solar = new Series { Color = Color.Orange, ChartType = SeriesChartType.StackedColumn};
            int i = 0;
            foreach (var genes in geneCollection)
            {
                int j = 0;
                foreach (var gene in genes)
                {
                    if (i == 0)
                    {
                        chart1.ChartAreas[0].AxisX.CustomLabels.Add(j, j + 1, CountryInfo.GetShortAbbrev(gene.Key));
                    }
                    var spacing = 1/((double) geneCollection.Length + 1);
                    wind.Points.AddXY(j + spacing*(i + 1), gene.Value.Gamma*gene.Value.Alpha);
                    solar.Points.AddXY(j + spacing*(i + 1), gene.Value.Gamma*(1 - gene.Value.Alpha));
                    j++;
                }
                i++;
            }
            chart1.Series.Add(wind);
            chart1.Series.Add(solar);
            
            // Remove legends.
            chart1.Legends.Clear();

            // Remove series spacing.
            foreach (Series series in chart1.Series)
            {
                if (geneCollection.Length == 1) break;
                series["PointWidth"] = "1";
            }

            // ADJUST ANGLE HERE!
            chart1.ChartAreas[0].AxisX.LabelStyle.Angle = 0; 
            
            RenderAxis();
        }

        private void RenderAxis()
        {
            MainChart.ChartAreas[0].AxisY.Minimum = 0;
            MainChart.ChartAreas[0].AxisY.Maximum =
                Math.Ceiling(MainChart.Series.SelectMany(item => item.Points).SelectMany(item => item.YValues).Max());
            MainChart.ChartAreas[0].AxisY.Interval = 1;
        }

    }
}
