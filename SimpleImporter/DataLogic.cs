using System;
using System.Collections.Generic;
using System.Linq;
using DataItems;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace SimpleImporter
{
    public class MainAccessClient
    {
        private readonly MongoCollection<CountryItem> _mCountryItems;
        private readonly MongoCollection<TsItem> _mTimeseriesItems;

        public MainAccessClient()
        {
            // Setup connection.
            const string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var mDatabase = server.GetDatabase("local");
            // Load tables.
            _mCountryItems = mDatabase.GetCollection<CountryItem>("CountryItem");
            _mTimeseriesItems = mDatabase.GetCollection<TsItem>("TimeSeriesItem");
        }

        #region Get country item

        public List<CountryData> GetAllCountryData()
        {
            var results = new List<CountryData>();
            var matches = _mCountryItems.FindAll().ToList();
            foreach (var match in matches)
            {
                results.Add(new CountryData {Abbreviation = match.Abbreviation, TimeSeries = GetTimeseries(match)});
            }
            return results;
        }

        public CountryData GetCountryData(string abbrevation)
        {
            var query = Query<CountryItem>.Matches(item => item.Abbreviation, abbrevation);
            var match = _mCountryItems.Find(query).SingleOrDefault();
            if (match == null) return null;

            return new CountryData {Abbreviation = abbrevation, TimeSeries = GetTimeseries(match)};
        }

        private List<DenseTimeSeries> GetTimeseries(CountryItem countryItem)
        {
            return countryItem.SignalIds.Select(GetTs).ToList();
        }

        private DenseTimeSeries GetTs(ObjectId id)
        {
            var query = Query<TsItem>.EQ(item => item.Id, id);
            var match = _mTimeseriesItems.Find(query).SingleOrDefault();
            if (match == null) return null;

            return new DenseTimeSeries(match.Name, match.Data);
        }

        #endregion

        #region Save country item

        public void SaveCountryData(List<CountryData> data)
        {
            foreach (var country in data)
            {
                var item = new CountryItem
                {
                    Abbreviation = country.Abbreviation,
                    Id = ObjectId.GenerateNewId(),
                    SignalIds = SaveTimeseries(country)
                };
                _mCountryItems.Insert(item);
            }
        }

        private List<ObjectId> SaveTimeseries(CountryData countryItem)
        {
            return countryItem.TimeSeries.Select(SaveTs).ToList();
        }

        private ObjectId SaveTs(DenseTimeSeries ts)
        {
            var id = ObjectId.GenerateNewId();
            _mTimeseriesItems.Insert(new TsItem { Id = id, Name = ts.Name, Data = ts.GetAllValues()});
            return id;
        }

        #endregion

        public static void DropDatabase()
        {
            const string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            if (server.DatabaseExists("local")) server.DropDatabase("local");
        }

    }

    public class CountryItem
    {
        // Primary key.
        public ObjectId Id { get; set; }

        public string Abbreviation { get; set; }
        public List<ObjectId> SignalIds { get; set; }
    }

    public class TsItem
    {
        // Primary key.
        public ObjectId Id { get; set; }

        public string Name { get; set; }
        public List<double> Data { get; set; }
    }
}
