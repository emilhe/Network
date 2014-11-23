using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Configurations
{
    class RealData
    {

        #region Use ECN data; with and without 6 hour storage (so far, not important)

        //public static void TryEcnData(MainForm main, bool save = false)
        //{
        //    var ecnData = ProtoStore.LoadEcnData();
        //    var withBattery = ConfigurationUtils.CreateNodes(TsSource.ISET, true);
        //    var withoutBattery = ConfigurationUtils.CreateNodes(TsSource.ISET);
        //    ConfigurationUtils.SetupNodesFromEcnData(withBattery, ecnData);
        //    ConfigurationUtils.SetupNodesFromEcnData(withoutBattery, ecnData);

        //    var gridParams = new GridScanParameters
        //    {
        //        MixingFrom = 0.45,
        //        MixingTo = 0.85,
        //        MixingSteps = 40,
        //        PenetrationFrom = 0.80,
        //        PenetrationTo = 1.10,
        //        PenetrationSteps = 100
        //    };

        //    var view = main.DisplayContour();
        //    view.AddData(gridParams.Rows, gridParams.Columns, RunSimulation(gridParams, withBattery), "Incl. 6-hour battery");
        //    view.AddData(gridParams.Rows, gridParams.Columns, RunSimulation(gridParams, withoutBattery), "Excl. 6-hour battery");

        //    if(save) ChartUtils.SaveChart(view.MainChart, 800, 400, @"C:\Users\xXx\Dropbox\Master Thesis\Notes\Figures\ECN.png");
        //}

        //private static bool[,] RunSimulation(GridScanParameters gridParams, List<CountryNode> nodes)
        //{
        //    return RunSimulation(gridParams, new CooperativeExportStrategy(new SkipFlowStrategy()), nodes);
        //}

        #endregion

    }
}
