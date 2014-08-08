using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SimpleVisualisation
{
    public class ZoomChart : Chart
    {

        public void PostInit()
        {
            // Enable zooming on chart.
            ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            MouseClick += ChartOnMouseClick;
        }

        private void ChartOnMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!mouseEventArgs.Button.Equals(MouseButtons.Right)) return;

            ChartAreas[0].AxisX.ScaleView.ZoomReset();
            ChartAreas[0].AxisY.ScaleView.ZoomReset();
        }

    }
}
