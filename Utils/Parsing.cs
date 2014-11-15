using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Parsing
    {

        public static Dictionary<T, V> DictionaryFromFile<T, V>(string path)
            where T : IConvertible
            where V : IConvertible
        {
            return File.ReadAllLines(path).Select(line => line.Split(':'))
                .ToDictionary(item => Parse<T>(item[0]), item => Parse<V>(item[1]));
        }

        private static T Parse<T>(string s) where T : IConvertible
        {
            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(s, typeof(T));
            if (typeof(T) == typeof(double)) return (T)Convert.ChangeType(double.Parse(s), typeof(T));
            if (typeof(T) == typeof(int)) return (T)Convert.ChangeType(int.Parse(s), typeof(T));
            throw new ArgumentException(string.Format("{0} is not supported by ditionary parsing.", typeof(T)));
        }

    }
}
