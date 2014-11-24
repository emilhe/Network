﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        #region Input/output extensions

        public static void ToFile<T, V>(this Dictionary<T, V> dictionary, string path)
        {
            File.WriteAllLines(path,dictionary.Select(x =>x.Key + ":" + x.Value));
        }

        public static Dictionary<T, V> DictionaryFromFile<T,V>(string path) where T : IConvertible where V: IConvertible
        {
            return File.ReadAllLines(path).Select(line => line.Split(':'))
                .ToDictionary(item => Parse<T>(item[0]), item => Parse<V>(item[1]));
        }

        public static T Parse<T>(string s) where T : IConvertible
        {
            if (typeof (T) == typeof (string)) return (T) Convert.ChangeType(s, typeof (T));
            if (typeof (T) == typeof (double)) return (T) Convert.ChangeType(double.Parse(s), typeof (T));
            if (typeof (T) == typeof (int)) return (T) Convert.ChangeType(int.Parse(s), typeof (T));
            throw new ArgumentException(string.Format("{0} is not supported by ditionary parsing.", typeof (T)));
        }

        #endregion

    }
}