using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace SimpleVisualisation
{
    public partial class ContourControlOxy : UserControl
    {

        private PlotView _mPlot;
        private PlotModel _mModel;

        public ContourControlOxy()
        {
            InitializeComponent();

            _mModel = new PlotModel();
            _mPlot = new PlotView
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Name = "Oxy",
                Model = _mModel,
            };

            // For now, just bind it.
            Controls.Add(_mPlot);
        }

        public void AddData(double[] rows, double[] columns, double[,] grid)
        {
            //var contourSeries = new ContourSeries
            //{
            //    Color = OxyColors.Black,
            //    LabelBackground = OxyColors.White,
            //    ColumnCoordinates = columns,
            //    RowCoordinates = rows,
            //    Data = grid,
            //    ContourLevels = new double[] { 0, 1, 2},
            //    ContourColors = new[] { OxyColor.FromRgb(255, 0, 0), OxyColor.FromRgb(0, 255, 0), OxyColor.FromRgb(0, 0, 255) },

            //};

            _mModel.Axes.Add(new LinearColorAxis()
            {
                Position = AxisPosition.Right,
                Palette = OxyPalettes.Jet(500),
                HighColor = OxyColors.Gray,
                LowColor = OxyColors.Black
            });

            var contourSeries = new HeatMapSeries
            {
                X0 = columns[0],
                X1 = columns[columns.Length-1],
                Y0 = rows[0],
                Y1 = rows[rows.Length - 1],
                Data = grid,
            };

            _mModel.Series.Add(contourSeries);
        }

    }
}
