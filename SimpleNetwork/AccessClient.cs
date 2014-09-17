using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Generators;
using BusinessLogic.TimeSeries;
using SimpleImporter;
using Utils;

namespace BusinessLogic
{
    public class AccessClient
    {

        #region Country data

        public List<Node> GetAllCountryData(TsSource source, int offset = 0)
        {
            var countries = ProtoStore.LoadCountries();
            var result = countries.Select(item => new Node(item, GetTs(item, TsType.Load, source, offset))).ToList();

            foreach (var node in result)
            {
                AddGenerator(node, TsType.Wind, source, offset);
                AddGenerator(node, TsType.Solar, source, offset);
            }
            return result;
        }

        private void AddGenerator(Node node, TsType type, TsSource source, int offset)
        {
            node.Generators.Add(new TimeSeriesGenerator(type.GetDescription(),
                GetTs(node.CountryName, type, source, offset)));
        }

        private DenseTimeSeries GetTs(string country, TsType type, TsSource source, int offset)
        {
            var tsBll = new DenseTimeSeries(ProtoStore.LoadTimeSeriesFromRoot(CsvImporter.GenerateFileKey(country, type, source)));
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
