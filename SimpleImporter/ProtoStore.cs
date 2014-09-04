using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace SimpleImporter
{
    public class ProtoStore
    {

        private const string BaseDir = @"C:\proto\";
        private const string ResultDir = @"C:\proto\result";

        #region Countries

        public static void SaveCountries(List<string> countries)
        {
            if (!Directory.Exists(BaseDir)) Directory.CreateDirectory(BaseDir);

            using (var file = File.Create(Path.Combine(BaseDir, "Countries")))
            {
                Serializer.Serialize(file, countries);
            }
        }

        public static List<string> LoadCountries()
        {
            List<string> result;
            using (var file = File.OpenRead(Path.Combine(BaseDir, "Countries")))
            {
                result = Serializer.Deserialize<List<string>>(file);
            }
            return result;
        }

        #endregion

        #region Time Series

        /// <summary>
        /// Method for saving time series in root folder (typically data imports).
        /// </summary>
        /// <param name="ts"> the time series to save </param>
        /// <param name="key"> the file name </param>
        public static void SaveTimeSeriesInRoot(TimeSeriesDal ts, string key)
        {
            if (!Directory.Exists(BaseDir)) Directory.CreateDirectory(BaseDir);

            using (var file = File.Create(Path.Combine(BaseDir, key)))
            {
                Serializer.Serialize(file, ts);
            }
        }

        /// <summary>
        /// Method for loading time series from root folder (typically data imports).
        /// </summary>
        /// <param name="key"> file name </param>
        /// <returns> the time series </returns>
        public static TimeSeriesDal LoadTimeSeriesFromRoot(string key)
        {
            if (!File.Exists(Path.Combine(BaseDir, key))) return null;

            TimeSeriesDal result;
            using (var file = File.OpenRead(Path.Combine(BaseDir, key)))
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
            if (!Directory.Exists(Path.Combine(ResultDir, key))) Directory.CreateDirectory(Path.Combine(ResultDir, key));
            var subKey = Guid.NewGuid();

            using (var file = File.Create(Path.Combine(ResultDir, key, subKey.ToString())))
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
            if (!File.Exists(Path.Combine(ResultDir, key, subKey.ToString()))) return null;

            TimeSeriesDal result;
            using (var file = File.OpenRead(Path.Combine(ResultDir, key, subKey.ToString())))
            {
                result = Serializer.Deserialize<TimeSeriesDal>(file);
            }
            return result;
        }

        #endregion

        #region ECN Data

        public static void SaveEcnData(List<EcnDataRow> data)
        {
            using (var file = File.Create(Path.Combine(BaseDir, "EcnData")))
            {
                Serializer.Serialize(file, data);
            }
        }

        public static List<EcnDataRow> LoadEcnData()
        {
            List<EcnDataRow> result;
            using (var file = File.OpenRead(Path.Combine(BaseDir, "EcnData")))
            {
                result = Serializer.Deserialize<List<EcnDataRow>>(file);
            }
            return result;
        }

        #endregion

        #region NTC Data

        public static void SaveNtcData(List<NtcDataRow> data)
        {
            using (var file = File.Create(Path.Combine(BaseDir, "NtcMatrix")))
            {
                Serializer.Serialize(file, data);
            }
        }

        public static List<NtcDataRow> LoadNtcData()
        {
            List<NtcDataRow> result;
            using (var file = File.OpenRead(Path.Combine(BaseDir, "NtcMatrix")))
            {
                result = Serializer.Deserialize<List<NtcDataRow>>(file);
            }
            return result;
        }

        #endregion

        #region Results : Grid

        public static void SaveGridResult(bool[,] grid, double[] rows, double[] cols, string key)
        {
            if (!Directory.Exists(ResultDir)) Directory.CreateDirectory(ResultDir);

            using (var file = File.Create(Path.Combine(ResultDir, key)))
            {
                Serializer.Serialize(file, new GridResultRow {Columns = cols, Rows = rows, Grid = grid.ToProtoArray<bool>()});
            }
        }

        public static GridResultRow LoadGridResult(string key)
        {
            if (!File.Exists(Path.Combine(ResultDir, key))) return null;

            GridResultRow result;
            using (var file = File.OpenRead(Path.Combine(ResultDir, key)))
            {
                result = Serializer.Deserialize<GridResultRow>(file);
            }
            return result;
        }

        #endregion

        #region Results : Simulation output

        public static void SaveSimulationOutput(SimulationOutputDal sim, string key)
        {
            if (!Directory.Exists(ResultDir)) Directory.CreateDirectory(ResultDir);
            if (!Directory.Exists(Path.Combine(ResultDir, key))) Directory.CreateDirectory(Path.Combine(ResultDir, key));

            using (var file = File.Create(Path.Combine(ResultDir, key, "META")))
            {
                Serializer.Serialize(file, sim);
            }   
        }

        public static SimulationOutputDal LoadSimulationOutput(string key)
        {
            if (!File.Exists(Path.Combine(ResultDir, key, "META"))) return null;

            SimulationOutputDal result;
            using (var file = File.OpenRead(Path.Combine(ResultDir, key, "META")))
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
    public class NtcDataRow
    {
        [ProtoMember(1)]
        public string CountryFrom { get; set; }
        [ProtoMember(2)]
        public string CountryTo { get; set; }
        [ProtoMember(3)]
        public int LinkCapacity { get; set; }
    }

    #endregion

}
