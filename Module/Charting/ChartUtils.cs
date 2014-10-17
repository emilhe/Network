using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Cursor = System.Windows.Forms.DataVisualization.Charting.Cursor;

namespace Controls.Charting
{
    public class ChartUtils
    {

        #region Styling

        public static void StyleChart(Chart chart)
        {
            chart.BackColor = Color.White;
            chart.BackSecondaryColor = Color.White;
            chart.BackGradientStyle = GradientStyle.None;
            chart.BorderlineColor = Color.White;
            chart.BorderlineWidth = 0;
            chart.BorderlineDashStyle = ChartDashStyle.Solid;
            chart.BorderSkin.BackColor = Color.Transparent;
            chart.BorderSkin.BackImageTransparentColor = Color.Transparent;
            chart.BorderSkin.BackSecondaryColor = Color.Transparent;
            chart.BorderSkin.BorderColor = Color.Transparent;
            chart.BorderSkin.PageColor = Color.Transparent;

            foreach (var area in chart.ChartAreas) StyleChartArea(area);
            foreach (var legend in chart.Legends) StyleLegend(legend);
        }

        private static void StyleChartArea(ChartArea area)
        {
            area.AlignmentOrientation = AreaAlignmentOrientations.Vertical | AreaAlignmentOrientations.Horizontal;
            area.BackColor = Color.FromArgb(229, 229, 229);
            area.BackGradientStyle = GradientStyle.None;
            area.BackSecondaryColor = Color.Transparent;
            area.BorderColor = Color.Transparent;
            area.BorderDashStyle = ChartDashStyle.NotSet;

            foreach (var axis in area.Axes) StyleAxis(axis);

            StyleCursor(area.CursorX);
            StyleCursor(area.CursorY);
        }

        private static void StyleAxis(Axis axis)
        {
            axis.IsLabelAutoFit = false;
            axis.LabelStyle.Font = new Font("Microsoft Sans Serif", 14F,
                FontStyle.Regular, GraphicsUnit.Point, 0);
            axis.LabelStyle.ForeColor = Color.Black;
            axis.LabelStyle.IntervalOffsetType = DateTimeIntervalType.Auto;
            axis.LabelStyle.IntervalType = DateTimeIntervalType.Auto;
            axis.LabelStyle.TruncatedLabels = true;
            axis.LineColor = Color.Transparent;
            axis.MajorGrid.LineColor = Color.White;
            axis.MajorGrid.LineWidth = 1;
            axis.MajorTickMark.LineColor = Color.FromArgb(127, 127, 127);
            axis.MinorGrid.LineColor = Color.FromArgb(242, 242, 242);
            axis.MinorTickMark.LineColor = Color.FromArgb(127, 127, 127);
            axis.ScrollBar.Size = 15;
            axis.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            axis.TitleAlignment = StringAlignment.Center;
            axis.TitleForeColor = Color.Black;
            axis.TitleFont = new Font("Microsoft Sans Serif", 14F,
                FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void StyleLegend(Legend legend)
        {
            legend.BackColor = Color.Transparent;
            legend.ForeColor = Color.Black;
            legend.IsTextAutoFit = false;
            legend.Docking = Docking.Top;
            legend.Alignment = StringAlignment.Center;
            legend.Font = legend.IsDockedInsideChartArea
                ? new Font("Microsoft Sans Serif", 14F, FontStyle.Regular, GraphicsUnit.Point, 0)
                : new Font("Microsoft Sans Serif", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void StyleCursor(Cursor cursor)
        {
            cursor.LineColor = Color.FromArgb(2, 67, 131);
            cursor.LineWidth = 2;
            cursor.SelectionColor = Color.FromArgb(108, 145, 183);
        }

        #endregion

        public static AxisRange CalcAxis(IEnumerable<double> data, double forceMin = double.PositiveInfinity)
        {
            var min = Math.Min(data.Min(), forceMin);
            var max = data.Max();
            var yRange = max - min;
            // Add some extra space.
            min = Math.Floor(min - Math.Abs(yRange) * 0.05);
            max = Math.Ceiling(max + Math.Abs(yRange) * 0.05);
            var tick = CalcStepSize(min - max, 10);

            return new AxisRange
            {
                Min = Math.Floor(min/tick),
                Max = Math.Ceiling(max / tick),
                Tick = tick
            };
        }

        public static double CalcStepSize(double range, double targetSteps)
        {
            // calculate an initial guess at step size
            var tempStep = range / targetSteps;

            // get the magnitude of the step size
            var mag = Math.Floor(Math.Log10(tempStep));
            var magPow = Math.Pow(10, mag);

            // calculate most significant digit of the new step size
            var magMsd = (tempStep / magPow + 0.5);

            // promote the MSD to either 1, 2, or 5
            if (magMsd > 5.0)
                magMsd = 10.0;
            else if (magMsd > 2.0)
                magMsd = 5.0;
            else if (magMsd > 1.0)
                magMsd = 2.0;

            return magMsd * magPow;
        }

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

        public static void SaveChart(Control control, int width, int height, string path)
        {
            var oldWidth = control.Width;
            var oldHeight = control.Height;
            control.Visible = false;

            control.Width = width;
            control.Height = height;
            // Off screen rendering.
            Bitmap image = null;
            control.CreateGraphics();
            image = new Bitmap(control.Width, control.Height);
            control.DrawToBitmap(image, new Rectangle(0, 0, control.Width, control.Height));
            //chart.Dispose();
            if(File.Exists(path)) File.Delete(path);
            image.Save(path, ImageFormat.Png);

            control.Width = oldWidth;
            control.Height = oldHeight;
            control.Visible = true;
        }

    }

    public class AxisRange
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Tick { get; set; }
    }
}
