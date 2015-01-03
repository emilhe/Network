using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Utils
{
    public static class Extensions
    {
        #region Hash extensions

        public static int UniqueKey<T, TU>(this Dictionary<T, TU> source)
        {
            unchecked
            {
                int hash = 17;
                foreach (var pair in source)
                {
                    hash = hash*29 + pair.Key.GetHashCode();
                    hash = hash*486187739 + pair.Value.GetHashCode();
                }
                return hash;
            }
        }

        #endregion

        #region Input/output extensions

        public static void ToJsonFile(this object obj, string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(obj));
        }

        // Matlab interface.

        public static void ToFile(this double[,] matrix, string path)
        {
            var separator = " ";
            var builder = new StringBuilder();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    builder.Append(matrix[i, j]);
                    if (j < matrix.GetLength(1) - 1) builder.Append(separator);
                }
                builder.AppendLine();
            }

            File.WriteAllText(path, MatrixToString(matrix));
        }

        private static string MatrixToString(double[,] matrix)
        {
            var separator = " ";
            var builder = new StringBuilder();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    builder.Append(matrix[i, j]);
                    if (j < matrix.GetLength(1) - 1) builder.Append(separator);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }

        #endregion

        #region Enum extension

        public static string GetDescription<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(
                typeof (DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            return source.ToString();
        }

        #endregion

        #region LinQ extensions

        public static double StdDev<T>(this IEnumerable<T> list, Func<T, double> values)
        {
            // ref: http://stackoverflow.com/questions/2253874/linq-equivalent-for-standard-deviation
            // ref: http://warrenseen.com/blog/2006/03/13/how-to-calculate-standard-deviation/ 
            var mean = 0.0;
            var sum = 0.0;
            var stdDev = 0.0;
            var n = 0;
            foreach (var value in list.Select(values))
            {
                n++;
                var delta = value - mean;
                mean += delta / n;
                sum += delta * (value - mean);
            }
            if (1 < n)
                stdDev = Math.Sqrt(sum / (n - 1));

            return stdDev;

        }

        #endregion

        #region Array extensions

        public static void MultiLoop(this Array array, Action<int[]> action)
        {
            array.RecursiveLoop(0, new int[array.Rank], action);
        }

        private static void RecursiveLoop(this Array array, int level, int[] indices, Action<int[]> action)
        {
            if (level == array.Rank)
            {
                action(indices);
            }
            else
            {
                for (indices[level] = 0; indices[level] < array.GetLength(level); indices[level]++)
                {
                    RecursiveLoop(array, level + 1, indices, action);
                }
            }
        }

        /// <summary>
        /// Convert a multidimensional array to 1D.
        /// </summary>
        public static T[] To1D<T>(this T[,] source)
        {
            var result = new T[source.Length];
            var k = 0;
            source.MultiLoop(indices => result[k++] = source[indices[0], indices[1]]);

            return result;
        }

        /// <summary>
        /// Conver the source to 2D.
        /// </summary>
        public static T[,] To2D<T>(this T[] source, int rows, int columns)
        {
            var result = new T[rows, columns];
            int k = 0;
            source.MultiLoop(indices => result[indices[0], indices[1]] = source[k++]);

            return result;
        }

        #endregion

        #region Deprecated

        public static void ToFile<T, V>(this IDictionary<T, V> dictionary, string path)
        {
            File.WriteAllLines(path, dictionary.Select(x => x.Key + ":" + x.Value));
        }

        public static IDictionary<T, V> DictionaryFromFile<T, V>(string path)
            where T : IConvertible
            where V : IConvertible
        {
            return File.ReadAllLines(path).Select(line => line.Split(':'))
                .ToDictionary(item => Parse<T>(item[0]), item => Parse<V>(item[1]));
        }

        public static T Parse<T>(string s) where T : IConvertible
        {
            if (typeof (T) == typeof (string)) return (T) Convert.ChangeType(s, typeof (T));
            if (typeof (T) == typeof (double)) return (T) Convert.ChangeType(Double.Parse(s), typeof (T));
            if (typeof (T) == typeof (double)) return (T) Convert.ChangeType(Double.Parse(s), typeof (T));
            if (typeof (T) == typeof (int)) return (T) Convert.ChangeType(Int32.Parse(s), typeof (T));
            throw new ArgumentException(String.Format("{0} is not supported by ditionary parsing.", typeof (T)));
        }

        #endregion
    }
}
