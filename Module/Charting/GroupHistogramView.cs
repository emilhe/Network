using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;
using Utils.Statistics;

namespace Controls.Charting
{
    public partial class GroupHistogramView : UserControl
    {

        public Chart MainChart { get { return chart; } }

        private int _mLabelCount;

        public GroupHistogramView()
        {
            InitializeComponent();

            ChartUtils.StyleChart(MainChart);
            ChartUtils.EnableZooming(MainChart);
            MainChart.ChartAreas[0].AxisX.LabelStyle.Format = "0.0";

            MainChart.Series.Clear();
        }

        public void Setup(List<string> labels)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                MainChart.ChartAreas[0].AxisX.CustomLabels.Add(i, i+1, labels[i]);                
            }
            MainChart.ChartAreas[0].AxisX.Minimum = 0;
            MainChart.ChartAreas[0].AxisX.Maximum = labels.Count;
            MainChart.ChartAreas[0].AxisX.Interval = 1;
            MainChart.ChartAreas[0].AxisX.IntervalOffset = 1;
            MainChart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _mLabelCount = labels.Count;
        }

        public void AddData(double[] table, string name)
        {
            if(table.Length != _mLabelCount) throw new ArgumentException("Dimension mismatch, group hist.");

            var series = new Series { ChartType = SeriesChartType.Column, Name = name};
            for (int i = 0; i < table.Length; i++)
            {
                series.Points.AddXY(0.5 + i, table[i]);
            }
            MainChart.Series.Add(series);

            // Resize the y-axis.
            MainChart.ChartAreas[0].AxisY.Minimum = 0;
            MainChart.ChartAreas[0].AxisY.Maximum =
                MainChart.Series.SelectMany(item => item.Points).SelectMany(item => item.YValues).Max() * 1.1;
            MainChart.ChartAreas[0].AxisY.Interval =
                ChartUtils.CalcStepSize(
                    MainChart.ChartAreas[0].AxisY.Maximum - MainChart.ChartAreas[0].AxisY.Minimum, 10);
        }

    }
}
