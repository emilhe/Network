using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;
using BusinessLogic.Utils;
using Controls.Charting;
using SimpleImporter;
using Utils;

namespace Main.Figures
{
    class EuropeMaps
    {

        public static void DrawCfs()
        {
            // Mean load distribution.
            var nodes = ConfigurationUtils.CreateNodesNew();
            //var ctrl = new MixController(nodes);
            //ctrl.SetMix(0.65);
            //ctrl.SetPenetration(1.026);
            //ctrl.Execute();

            var loadScaling = nodes.ToDictionary(item => item.Name, item => CountryInfo.GetOnshoreWindCf(item.Name));
            var chart = EuropeChart.DrawEurope(loadScaling, Color.Black, Color.LightBlue, Color.DarkBlue);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Slides2\Images\wCfDistribution.png");

            loadScaling = nodes.ToDictionary(item => item.Name, item => CountryInfo.GetSolarCf(item.Name));
            chart = EuropeChart.DrawEurope(loadScaling, Color.Black, Color.Yellow, Color.DarkRed);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Slides2\Images\sCfDistribution.png");
        }

        #region DistributionMaps

        public static void DrawDistributions()
        {
            // Mean load distribution.
            var nodes = ConfigurationUtils.CreateNodes(TsSource.VE);
            //var ctrl = new MixController(nodes);
            //ctrl.SetMix(0.65);
            //ctrl.SetPenetration(1.026);
            //ctrl.Execute();

            var loadScaling = ConfigurationUtils.LoadScaling(nodes);
            var chart = EuropeChart.DrawEurope(loadScaling, Color.Black, Color.Yellow, Color.DarkGreen);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\MeanLoadDistribution.png");

            //// Mismatch distribution.
            //var scaling = ConfigurationUtils.MismatchScaling(nodes);
            //DeltaScaling(loadScaling, scaling);
            //chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            //chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\MeanMismatchDistribution.png");

            #region Realistic distributions

            var scaling = ConfigurationUtils.HeterogeneousBackupScaling(nodes);
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -20, 45);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\HeterogeneousBackupDistribution.png");

            scaling = ConfigurationUtils.HeterogeneousStorageScaling(nodes);
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -15, 85);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\HeterogeneousStorageDistribution.png");

            #endregion


            #region Optimal distributions

            // Optimal distribution [TESt].
            scaling = FileUtils.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryHydrogenDelta.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryHydrogenDelta.png");

            // Optimal distribution.
            scaling = FileUtils.DictionaryFromFile<string, double>(@"C:\proto\OptimalNoStorageNoLinks.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -8, 8);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalNoStorageNoLinksNorm.png");

            // Optimal distribution without links.
            scaling = FileUtils.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryNoLinks.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -8, 8);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryNoLinksNorm.png");

            // Optimal distribution without links.
            scaling = FileUtils.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryHydrogenNoLinks.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -8, 8);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryHydrogenNoLinksNorm.png");

            // Optimal distribution.
            scaling = FileUtils.DictionaryFromFile<string, double>(@"C:\proto\OptimalNoStorage.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalNoStorageNorm.png");

            // Optimal distribution without links.
            scaling = FileUtils.DictionaryFromFile<string, double>(@"C:\proto\OptimalBattery.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryNorm.png");

            // Optimal distribution without links.
            scaling = FileUtils.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryHydrogen.txt");
            DeltaScaling(loadScaling, scaling);
            chart = EuropeChart.DrawEurope(scaling, Color.Black, Color.Yellow, Color.DarkRed, -2.5, 2.5);
            chart.Save(@"C:\Users\Emil\Dropbox\Master Thesis\Notes\Figures\OptimalBatteryHydrogenNorm.png");

            #endregion

        }

        private static void DeltaScaling(Dictionary<string, double> loadScaling, Dictionary<string, double> scaling)
        {
            var sum = scaling.Values.Sum();
            foreach (var key in loadScaling.Keys.ToArray())
            {
                if (scaling.ContainsKey(key)) scaling[key] = (scaling[key] / sum - loadScaling[key]) * 100;
                else scaling.Add(key, -loadScaling[key] * 100);
            }
        }

        #endregion

    }
}
