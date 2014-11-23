using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Utils;

namespace Main.Configurations
{
    class ChartTest
    {

        #region Contour control test

        public static void TestContourControl(MainForm main)
        {
            var gridParams = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 8,
                PenetrationFrom = 1.00,
                PenetrationTo = 1.15,
                PenetrationSteps = 4
            };

            var grid = new[,]
            {
                {false, false, false, true, true, false, false , false},
                {false, false, true, true, true, true, false , false},
                {false, true, true, true, true, true, true , false},
                {true, true, true, true, true, true, true , true},
            };

            var view = main.DisplayContour();
            view.AddData(gridParams.Rows, gridParams.Cols, grid, "TEST Data");
        }

        #endregion

        #region Plot control test

        public static void TestPlotControl(MainForm main)
        {
            var view = main.DisplayPlot();
            var data = new Dictionary<double, double>();
            data.Add(1, 2);
            data.Add(3, 4);
            data.Add(7, 5);
            view.AddData(data, "Test");
        }

        #endregion

    }
}
