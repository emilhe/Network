using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;
using BusinessLogic.Utils;
using Controls.Charting;
using SimpleImporter;

namespace Main.Figures
{
    class Comparisons
    {

        public static void CompareData(MainForm main)
        {
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 40,
                PenetrationFrom = 1.00,
                PenetrationTo = 1.15,
                PenetrationSteps = 100
            };
            var ctrl = new SimulationController
            {
                ExportStrategies = new List<ExportStrategyInput>
                {
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.Cooperative
                    }
                },
                Sources = new List<TsSourceInput>
                {
                    // Simulate 8 years; the ISET data are 8 years long. Offset VE data by 21; VE are 1979-2010 while ISET are 2000-2007.
                    new TsSourceInput {Source = TsSource.ISET, Offset = 0, Length = 8},
                    new TsSourceInput {Source = TsSource.VE, Offset = 21, Length = 8},
                }
            };

            var data = ctrl.EvaluateGrid(grid);
            view.AddData(grid.Rows, grid.Cols, data[0].Grid, "ISET");
            view.AddData(grid.Rows, grid.Cols, data[1].Grid, "VE");

            // Prepare chart printing.
            view.MainChart.ChartAreas[0].AxisX.Maximum = 1.12;
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            view.MainChart.ChartAreas[0].AxisY.Title = "Mixing, α";
            ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\CompareSources.png");
        }

        public static void CompareExportSchemes(MainForm main)
        {
            var view = main.DisplayContour();
            var grid = new GridScanParameters
            {
                MixingFrom = 0.45,
                MixingTo = 0.85,
                MixingSteps = 40,
                PenetrationFrom = 1.00,
                PenetrationTo = 1.70,
                PenetrationSteps = 100
            };
            var ctrl = new SimulationController
            {
                ExportStrategies = new List<ExportStrategyInput>
                {
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.Cooperative
                    },
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.Selfish
                    },
                    new ExportStrategyInput
                    {
                        DistributionStrategy = DistributionStrategy.SkipFlow,
                        ExportStrategy = ExportStrategy.None
                    }
                },
                Sources = new List<TsSourceInput>
                {
                    new TsSourceInput {Source = TsSource.VE, Offset = 0, Length = 32},
                }
            };

            var data = ctrl.EvaluateGrid(grid);
            view.AddData(grid.Rows, grid.Cols, data[0].Grid, "Cooperative");
            view.AddData(grid.Rows, grid.Cols, data[1].Grid, "Selfish");
            view.AddData(grid.Rows, grid.Cols, data[2].Grid, "None");

            // Prepare chart printing.
            view.MainChart.ChartAreas[0].AxisX.Title = "Penetration, γ";
            view.MainChart.ChartAreas[0].AxisY.Title = "Mixing, α";
            ChartUtils.SaveChart(view.MainChart, 1000, 500, @"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\ExportSchemes.png");
        }


    }
}
