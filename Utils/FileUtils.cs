using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Utils
{
    public class FileUtils
    {

        public static T FromJsonFile<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        public static Dictionary<T, V> DictionaryFromFile<T, V>(string path)
            where T : IConvertible
            where V : IConvertible
        {
            return File.ReadAllLines(path).Select(line => line.Split(':'))
                .ToDictionary<string[], T, V>(item => Parse<T>(item[0]), item => Parse<V>(item[1]));
        }

        private static T Parse<T>(string s) where T : IConvertible
        {
            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(s, typeof(T));
            if (typeof(T) == typeof(double)) return (T)Convert.ChangeType(Double.Parse(s), typeof(T));
            if (typeof(T) == typeof(int)) return (T)Convert.ChangeType(Int32.Parse(s), typeof(T));
            throw new ArgumentException(String.Format("{0} is not supported by ditionary parsing.", typeof(T)));
        }

    }
}
