using System;
using System.Collections.Generic;
using System.Linq;
using DataItems.TimeSeries;

namespace DataItems
{

    public class CountryData : ICloneable
    {
        public string Abbreviation { get; set; }
        public List<DenseTimeSeries> TimeSeries { get; set; }

        public object Clone()
        {
            return new CountryData
            {
                Abbreviation = Abbreviation,
                TimeSeries = TimeSeries.Select(item => (DenseTimeSeries) item.Clone()).ToList()
            };
        }
    }

}
