using System.Collections.Generic;
using System.Linq;
using DataItems;
using SimpleImporter;
using SimpleNetwork.Generators;
using SimpleNetwork.Interfaces;
using SimpleNetwork.TimeSeries;

namespace SimpleNetwork
{
    public class AccessClient
    {

        public List<Node> GetAllCountryData(TsSource source)
        {
            var countries = ProtoStore.LoadCountries();
            var result = countries.Select(item => new Node(item, GetTs(item, TsType.Load, source))).ToList();

            foreach (var node in result)
            {
                AddGenerator(node, TsType.Wind, source);
                AddGenerator(node, TsType.Solar, source);
            }
            return result;
        }

        private void AddGenerator(Node node, TsType type, TsSource source)
        {
            node.Generators.Add(new TimeSeriesGenerator(type.GetDescription(), GetTs(node.CountryName, type, source)));
        }

        private DenseTimeSeries GetTs(string country, TsType type, TsSource source)
        {
            var tsDal = ProtoStore.LoadTimeSeries(country, type, source);
            return new DenseTimeSeries(type.GetDescription(), tsDal.Data);
        }

    }
}
