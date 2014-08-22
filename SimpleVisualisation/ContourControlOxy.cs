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

        private readonly PlotView _mPlot;

        public ContourControlOxy()
        {
            InitializeComponent();

            _mPlot = new PlotView
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Name = "Oxy",
            };

            // For now, just bind it.
            Controls.Add(_mPlot);
        }

        public void SetHeatData(double[] rows, double[] columns, List<bool[,]> data)
        {
            var model = new PlotModel();
            _mPlot.Model = model;

            model.Axes.Add(new LinearColorAxis()
            {
                Position = AxisPosition.Right,
                Palette = OxyPalettes.Jet(500),
                HighColor = OxyColors.Gray,
                LowColor = OxyColors.Black
            });

            model.Series.Add(new HeatMapSeries
            {
                X0 = columns[0],
                X1 = columns[columns.Length - 1],
                Y0 = rows[0],
                Y1 = rows[rows.Length - 1],
                Data = MapGrids(data),
            });
        }

        /// <summary>
        /// It is assumed that the grid are the same size.
        /// </summary>
        private double[,] MapGrids(List<bool[,]> grid, double value = 1)
        {
            var result = new double[grid[0].GetLength(0), grid[0].GetLength(1)];
            for (int i = 0; i < grid[0].GetLength(0); i++)
            {
                for (int j = 0; j < grid[0].GetLength(1); j++)
                {
                    for (int k = 0; k < grid.Count; k++)
                    {
                        if (grid[k][i, j]) result[i, j] = k + 1;
                    }
                }
            }
            return result;
        }

    }
}
