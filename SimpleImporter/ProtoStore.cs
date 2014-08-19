using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace SimpleImporter
{
    public class ProtoStore
    {

        #region File path mappings.

        private const string BaseDir = @"C:\proto\";

        private static string GetFileName(TimeSeriesItemDal ts)
        {
            return Path.Combine(BaseDir, string.Format("Ts-{0}-{1}-{2}", ts.Country, ts.Type, ts.Source));
        }

        private static string GetFileName(string country, TsType type, TsSource source)
        {
            return Path.Combine(BaseDir, string.Format("Ts-{0}-{1}-{2}", country, type, source));
        }

        #endregion

        #region Time Series

        public static void SaveTimeSeries(TimeSeriesItemDal ts)
        {
            if (!Directory.Exists(BaseDir)) Directory.CreateDirectory(BaseDir);

            using (var file = File.Create(GetFileName(ts)))
            {
                Serializer.Serialize(file, ts);
            }
        }

        public static TimeSeriesItemDal LoadTimeSeries(string country, TsType type, TsSource source)
        {
            TimeSeriesItemDal result;
            using (var file = File.OpenRead(GetFileName(country, type, source)))
            {
                result = Serializer.Deserialize<TimeSeriesItemDal>(file);
            }
            return result;
        }

        #endregion

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

    }

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
    public class TimeSeriesItemDal
    {
        [ProtoMember(1)]
        public string Country { get; set; }
        [ProtoMember(2)]
        public List<double> Data { get; set; }
        [ProtoMember(3)]
        private byte MSource { get; set; }
        [ProtoMember(4)]
        private byte MType { get; set; }

        public TsSource Source
        {
            get { return ((TsSource) MSource); }
            set { MSource = (byte) value; }
        }
        public TsType Type
        {
            get { return ((TsType)MType); }
            set { MType = (byte)value; }
        }
    }

    public enum TsSource : byte
    {
        ISET = 1, VE = 2
    }

    public enum TsType : byte
    {
        Load = 1, Wind = 2, Solar = 3
    }

}
