using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

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
                    hash = hash * 29 + pair.Key.GetHashCode();
                    hash = hash * 486187739 + pair.Value.GetHashCode();
                }
                return hash;
            }
        }

        #endregion

        #region Enum extension

        public static string GetDescription<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            return source.ToString();
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

    }
}
