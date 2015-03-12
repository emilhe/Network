using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using Newtonsoft.Json;

namespace Utils
{
    public static class Extensions
    {

        public static int[] Linspace(this int[] source)
        {
            return source.Linspace(0, source.Length);
        }

        public static int[] Linspace(this int[] source, int from, int to)
        {
            for (int i = from; i < to; i++) source[i] = i;
            return source;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key.ToString() + "=" + kv.Value.ToString()).ToArray()) + "}";
        }

        #region Matrix extensions

        public static Matrix<double> PseudoInverse(this Matrix<double> A)
        {
            var evd = A.Evd();
            var D = evd.D();
            var Dplus = new DenseMatrix(D.RowCount, D.ColumnCount);
            for (int i = 0; i < D.RowCount; i++)
            {
                Dplus[i, i] = (D[i, i] < 1e-6) ? 0 : 1.0/D[i, i];
            }
            var V = evd.EigenVectors();
            return V.Multiply(Dplus.Multiply(V.Transpose()));
        }

        #endregion

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
                mean += delta/n;
                sum += delta*(value - mean);
            }
            if (1 < n)
                stdDev = Math.Sqrt(sum/(n - 1));

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

        #region Math extensions

        /// <summary>
        ///   Generates normally distributed numbers. Each operation makes two Gaussians for the price of one, and apparently they can be cached or something for better performance, but who cares.
        /// </summary>  
        /// <param name="r"></param>
        /// <param name = "mu">Mean of the distribution</param>
        /// <param name = "sigma">Standard deviation</param>
        /// <returns></returns>
        public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0*Math.Log(u1))*
                                  Math.Sin(2.0*Math.PI*u2);

            var rand_normal = mu + sigma*rand_std_normal;

            return rand_normal;
        }

        public static double NextLevy(this Random r, double alpha = 0.5, double beta = 1, double mu = 0,
            double sigma = 1)
        {
            // Uniform on the interval [-0.5 pi, 0.5 pi].
            var v = (r.NextDouble() - 0.5)*Math.PI;
            // Exponential distribution with mean 1.
            var w = -Math.Log(r.NextDouble());

            return (alpha == 1.0)
                ? LevyAlphaEqualOne(v, w, beta, mu, sigma)
                : LevyAlphaNotEqualOne(v, w, alpha, beta, mu, sigma);
        }

        private static double LevyAlphaNotEqualOne(double v, double w, double alpha, double beta, double mu, double sigma)
        {
            // Can be precalculated to decrease computation time.
            var b = Math.Atan(beta*Math.Tan(Math.PI*alpha/2))/alpha;
            var s = Math.Pow(1+Math.Pow(beta*Math.Tan(Math.PI*alpha/2),2), 1/(2*alpha));
                   
            // Lévy alpha-stable distribution with sigma = 1 and mu = 0;
            var x = s*Math.Sin(alpha*(v + b))/Math.Pow(Math.Cos(v), 1/alpha)
                    *Math.Pow(Math.Cos(v - alpha*(v + b))/w, (1 - alpha)/alpha);
            // Lévy alpha-stable distribution scaled with sigma and mu;
            var y = sigma * x + 2/Math.PI*beta*sigma*Math.Log(sigma) + mu;

            return y;
        }

        private static double LevyAlphaEqualOne(double v, double w, double beta, double mu, double sigma)
        {
            // Lévy alpha-stable distribution with sigma = 1 and mu = 0;
            var x = 2/Math.PI*
                    ((Math.PI/2 + beta*v)*Math.Tan(v) - beta*Math.Log(Math.PI/2*w*Math.Cos(v)/(Math.PI/2 + beta*v)));
            // Lévy alpha-stable distribution scaled with sigma and mu;
            var y = sigma*x + mu;

            return y;
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
            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(s, typeof(T));
            if (typeof(T) == typeof(double)) return (T)Convert.ChangeType(Double.Parse(s), typeof(T));
            if (typeof(T) == typeof(double)) return (T)Convert.ChangeType(Double.Parse(s), typeof(T));
            if (typeof(T) == typeof(int)) return (T)Convert.ChangeType(Int32.Parse(s), typeof(T));
            throw new ArgumentException(String.Format("{0} is not supported by ditionary parsing.", typeof(T)));
        }

        #endregion

    }
}

    