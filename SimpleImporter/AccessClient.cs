using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;

namespace SimpleImporter
{
    public class AccessClient
    {

        public List<CountryData> GetAllCountryData(TsSource source)
        {
            var result = ProtoStore.LoadCountries().Select(country => new CountryData {Abbreviation = country}).ToList();
            foreach (var country in result)
            {
                country.TimeSeries = new List<DenseTimeSeries>
                {
                    Map(ProtoStore.LoadTimeSeries(country.Abbreviation, TsType.Load, source)),
                    Map(ProtoStore.LoadTimeSeries(country.Abbreviation, TsType.Wind, source)),
                    Map(ProtoStore.LoadTimeSeries(country.Abbreviation, TsType.Solar, source))
                };
            }
            return result;
        }

        private DenseTimeSeries Map(TimeSeriesItemDal tsDal)
        {
            return new DenseTimeSeries(tsDal.Type.ToString(), tsDal.Data);
        }

    }
}
