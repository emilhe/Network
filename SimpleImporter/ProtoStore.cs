﻿using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace SimpleImporter
{
    public class ProtoStore
    {

        public const string BaseDir = @"C:\EmherSN\";
        public const string DataDir = @"C:\EmherSN\data";
        public const string CacheDir = @"C:\EmherSN\cache";
        //public const string ResultDir = @"C:\EmherSN\results";
        public const string ImportDir = @"C:\EmherSN\imports";

        #region Countries

        public static void SaveCountries(List<string> countries)
        {
            if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);

            using (var file = File.Create(Path.Combine(DataDir, "Countries")))
            {
                Serializer.Serialize(file, countries);
            }
        }

        public static List<string> LoadCountries()
        {
            List<string> result;
            using (var file = File.OpenRead(Path.Combine(DataDir, "Countries")))
            {
                result = Serializer.Deserialize<List<string>>(file);
            }
            return result;
        }

        #endregion

        #region Time Series

        /// <summary>
        /// Method for saving time series in import folder (typically data imports).
        /// </summary>
        /// <param name="ts"> the time series to save </param>
        /// <param name="key"> the file name </param>
        public static void SaveTimeSeriesInImport(TimeSeriesDal ts, string key)
        {
            if (!Directory.Exists(ImportDir)) Directory.CreateDirectory(ImportDir);

            using (var file = File.Create(Path.Combine(ImportDir, key)))
            {
                Serializer.Serialize(file, ts);
            }
        }

        /// <summary>
        /// Method for loading time series from import folder (typically data imports).
        /// </summary>
        /// <param name="key"> file name </param>
        /// <returns> the time series </returns>
        internal static TimeSeriesDal LoadTimeSeriesFromImport(string key)
        {
            if (!File.Exists(Path.Combine(ImportDir, key))) return null;

            TimeSeriesDal result;
            using (var file = File.OpenRead(Path.Combine(ImportDir, key)))
            {
                result = Serializer.Deserialize<TimeSeriesDal>(file);
            }
            return result;
        }

        /// <summary>
        /// Method for saving a (result) time series by GUID. Should be done by the system.
        /// </summary>
        /// <param name="ts"> the time series to save </param>
        /// <returns> file name </returns>
        public static Guid SaveTimeSeries(TimeSeriesDal ts, string key)
        {
            if (!Directory.Exists(Path.Combine(CacheDir, key))) Directory.CreateDirectory(Path.Combine(CacheDir, key));
            var subKey = Guid.NewGuid();

            using (var file = File.Create(Path.Combine(CacheDir, key, subKey.ToString())))
            {
                Serializer.Serialize(file, ts);
            }

            return subKey;
        }

        /// <summary>
        /// Method for loading a (result) time series by GUID. Should be done by the system.
        /// </summary>
        /// <param name="key"> file name </param>
        /// <returns> the time series </returns>
        public static TimeSeriesDal LoadTimeSeries(Guid subKey, string key)
        {
            if (!File.Exists(Path.Combine(CacheDir, key, subKey.ToString()))) return null;

            TimeSeriesDal result;
            using (var file = File.OpenRead(Path.Combine(CacheDir, key, subKey.ToString())))
            {
                result = Serializer.Deserialize<TimeSeriesDal>(file);
            }
            return result;
        }

        #endregion

        #region ECN Data

        public static void SaveEcnData(List<EcnDataRow> data)
        {
            using (var file = File.Create(Path.Combine(DataDir, "EcnData")))
            {
                Serializer.Serialize(file, data);
            }
        }

        public static List<EcnDataRow> LoadEcnData()
        {
            List<EcnDataRow> result;
            using (var file = File.OpenRead(Path.Combine(DataDir, "EcnData")))
            {
                result = Serializer.Deserialize<List<EcnDataRow>>(file);
            }
            return result;
        }

        #endregion

        #region NTC Data

        public static void SaveLinkData(List<LinkDataRow> data, string key)
        {
            using (var file = File.Create(Path.Combine(DataDir, key)))
            {
                Serializer.Serialize(file, data);
            }
        }

        public static List<LinkDataRow> LoadLinkData(string key)
        {
            List<LinkDataRow> result;
            using (var file = File.OpenRead(Path.Combine(DataDir, key)))
            {
                result = Serializer.Deserialize<List<LinkDataRow>>(file);
            }
            return result;
        }

        #endregion

        #region Results : Grid

        // Currently simulation saving is done in the cache dir, since that's what simulation saving is used for.

        public static void SaveGridResult(bool[,] grid, double[] rows, double[] cols, string key)
        {
            if (!Directory.Exists(CacheDir)) Directory.CreateDirectory(CacheDir);

            using (var file = File.Create(Path.Combine(CacheDir, key)))
            {
                Serializer.Serialize(file, new GridResultRow {Columns = cols, Rows = rows, Grid = grid.ToProtoArray<bool>()});
            }
        }

        public static GridResultRow LoadGridResult(string key)
        {
            if (!File.Exists(Path.Combine(CacheDir, key))) return null;

            GridResultRow result;
            using (var file = File.OpenRead(Path.Combine(CacheDir, key)))
            {
                result = Serializer.Deserialize<GridResultRow>(file);
            }
            return result;
        }

        #endregion

        #region Results : Simulation output

        // Currently simulation saving is done in the cache dir, since that's what simulation saving is used for.

        public static void SaveSimulationOutput(SimulationOutputDal sim, string key)
        {
            if (!Directory.Exists(CacheDir)) Directory.CreateDirectory(CacheDir);
            if (!Directory.Exists(Path.Combine(CacheDir, key))) Directory.CreateDirectory(Path.Combine(CacheDir, key));

            using (var file = File.Create(Path.Combine(CacheDir, key, "META")))
            {
                Serializer.Serialize(file, sim);
            }   
        }

        public static SimulationOutputDal LoadSimulationOutput(string key)
        {
            if (!File.Exists(Path.Combine(CacheDir, key, "META"))) return null;

            SimulationOutputDal result;
            using (var file = File.OpenRead(Path.Combine(CacheDir, key, "META")))
            {
                result = Serializer.Deserialize<SimulationOutputDal>(file);
            }
            return result;
        }

        #endregion

    }

    #region Dynamic data elements

    /// <summary>
    /// Imported time series are typically dense.
    /// </summary>
    [ProtoContract]
    public class TimeSeriesDal
    {

        /// <summary>
        /// The actual data point (= data values).
        /// </summary>
        [ProtoMember(1)]
        public List<double> Data { get; set; }

        /// <summary>
        /// Index offsets (null if the ts is dense).
        /// </summary>
        [ProtoMember(2)]
        public List<int> DataIndices { get; set; }

        // TODO: Add start time property (not ready yet).
        ///// <summary>
        ///// The start date for the ts.
        ///// </summary>
        //[ProtoMember(3)]
        //public DateTime StartDate { get; set; }

        /// <summary>
        /// Properties denote ALL other properties than the raw data.
        /// </summary>
        [ProtoMember(4)]
        public Dictionary<string, string> Properties { get; set; }

    }

    [ProtoContract]
    public class SimulationOutputDal
    {
        [ProtoMember(1)]
        public Dictionary<string, string> Properties { get; set; }
        [ProtoMember(2)]
        public List<Guid> TimeSeriesKeys { get; set; }
    }

    [ProtoContract]
    public class GridResultRow
    {

        [ProtoMember(1)]
        public ProtoArray<bool> Grid { get; set; }
        [ProtoMember(2)]
        public double[] Rows { get; set; }
        [ProtoMember(3)]
        public double[] Columns { get; set; }

    }

    [ProtoContract]
    public class ProtoArray<T>
    {
        [ProtoMember(1)]
        public int[] Dimensions { get; set; }
        [ProtoMember(2)]
        public T[] Data { get; set; }
    }

    #endregion

    #region Static data elements

    [ProtoContract]
    public class EcnDataRow
    {
        [ProtoMember(1)]
        public string Country { get; set; }
        [ProtoMember(2)]
        public string RowHeader { get; set; }
        [ProtoMember(3)]
        public string ColumnHeader { get; set; }
        [ProtoMember(4)]
        public int Year { get; set; }
        [ProtoMember(5)]
        public string Unit { get; set; }
        [ProtoMember(6)]
        public double Value { get; set; }
    }

    [ProtoContract]
    public class LinkDataRow
    {
        [ProtoMember(1)]
        public string From { get; set; }
        [ProtoMember(2)]
        public string To { get; set; }
        [ProtoMember(3)]
        public double LinkCapacity { get; set; }
        //[ProtoMember(3)]
        public string Type { get; set; }
    }

    #endregion

}
