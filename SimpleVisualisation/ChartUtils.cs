using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SimpleVisualisation
{
    public class ChartUtils
    {

        public static void EnableZooming(Chart chart)
        {
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart.MouseClick += (sender, args) =>
            {
                if (!args.Button.Equals(MouseButtons.Right)) return;

                chart.ChartAreas[0].AxisX.ScaleView.ZoomReset();
                chart.ChartAreas[0].AxisY.ScaleView.ZoomReset();
            };
        }

    }
}
