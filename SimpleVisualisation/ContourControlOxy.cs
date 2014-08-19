using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace SimpleVisualisation
{
    public partial class ContourControlOxy : UserControl
    {

        private PlotView plot1;

        public ContourControlOxy()
        {
            InitializeComponent();

            // For now, just bind it.
            plot1 = new PlotView
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Name = "Oxy",
            };
            Controls.Add(plot1);
        }

        public void SetData(double[] rows, double[] columns, double[,] grid)
        {

            var myModel = new PlotModel("Countour Plot");
            var contourSeries = new ContourSeries
            {
                ColumnCoordinates = columns,
                RowCoordinates = rows,
                Data = grid,
                ContourLevels = new double[] { 0, 1 },
                ContourColors = new[] { OxyColor.FromRgb(255, 0, 0), OxyColor.FromRgb(0, 255, 0) },
                
            };
            myModel.Series.Add(contourSeries);
            plot1.Model = myModel;
        }

    }
}
