using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Generators;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.Simulation;
using BusinessLogic.TimeSeries;
using NUnit.Framework.Constraints;
using SimpleImporter;
using Utils;

namespace BusinessLogic
{
    public class AccessClient
    {

        #region Country data

        public static List<CountryNode> GetAllCountryDataOld(TsSource source, int offset = 0)
        {
            var countries = ProtoStore.LoadCountries();
            var result = new List<CountryNode>(countries.Count);

            foreach (var country in countries)
            {
                var load = GetTs(country, TsType.Load, source, offset);
                var onshoreWind = GetTs(country, TsType.Wind, source, offset);
                var solar = GetTs(country, TsType.Solar, source, offset);
                result.Add(new CountryNode(new ReModel(country, load, solar, onshoreWind)));
            }

            return result;
        }

        public static List<CountryNode> GetAllCountryDataNew(TsSource source, int offset = 0)
        {
            var countries = ProtoStore.LoadCountries();
            var result = new List<CountryNode>(countries.Count);

            foreach (var country in countries)
            {
                var load = GetTs(country, TsType.Load, TsSource.VE, offset);
                var onshoreWind = GetTs(country, TsType.OnshoreWind, source, offset);
                var offshoreWind = GetTs(country, TsType.OffshoreWind, source, offset);
                var solar = GetTs(country, TsType.Solar, source, offset);
                result.Add(new CountryNode(new ReModel(country, load, solar, onshoreWind, offshoreWind)));
            }

            return result;
        }

        //private void AddGenerator(CountryNode countryNode, TsType type, TsSource source, int offset)
        //{
        //    countryNode.Generators.Add(new TimeSeriesGenerator(type.GetDescription(),
        //        GetTs(countryNode.Name, type, source, offset)));
        //}

        private static DenseTimeSeries GetTs(string country, TsType type, TsSource source, int offset)
        {
            var tsBll = new DenseTimeSeries(ProtoCache.LoadTimeSeriesFromImport(CsvImporter.GenerateFileKey(country, type, source)));
            tsBll.SetOffset(offset);

            return tsBll;
        }

        #endregion

        #region Simulation output

        public static void SaveSimulationOutput(SimulationOutput output, string key)
        {
            // First save the time series.
            var dal = new SimulationOutputDal
            {
                TimeSeriesKeys = output.TimeSeries.Select(item => ProtoStore.SaveTimeSeries(item.ToTimeSeriesDal(), key)).ToList(),
                Properties = output.Properties
            };

            // Then save the "meta data".
            ProtoStore.SaveSimulationOutput(dal, key);
        }

        public static SimulationOutput LoadSimulationOutput(string key)
        {
            // First load the data.
            var simDal = ProtoStore.LoadSimulationOutput(key);
            if (simDal == null) return null;

            var ts = simDal.TimeSeriesKeys.Select(tsKey => ProtoStore.LoadTimeSeries(tsKey, key).ToTimeSeries()).ToList();
            // The map it to the simulation output object.
            return new SimulationOutput
            {
                Properties = simDal.Properties,
                TimeSeries = ts
            };
        }

        #endregion

        #region Grid result

        public static void SaveGridResult(GridResult output, string key)
        {
            ProtoStore.SaveGridResult(output.Grid, output.Rows, output.Columns, key);
        }

        public static GridResult LoadGridResult(string key)
        {
            var result = ProtoStore.LoadGridResult(key);
            if (result == null) return null;

            return new GridResult {Columns = result.Columns, Rows = result.Rows, Grid = (bool[,]) result.Grid.ToArray()};
        }

        #endregion

    }


}
