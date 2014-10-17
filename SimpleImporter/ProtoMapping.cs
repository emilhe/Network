using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoBuf;
using Utils;

namespace SimpleImporter
{

    public static class ProtoMapping
    {

        #region Dictionary handling

        public static ProtoArray<KeyValuePair<T,U>> ToProtoArray<T, U>(IDictionary<T, U> dict)
        {
            // Flattern "tree" here.
            return new ProtoArray<KeyValuePair<T, U>> {Data = dict.ToArray(), Dimensions = new[] {dict.Count}};
        }

        public static Dictionary<T, U> ToDictionary<T, U>(this ProtoArray<KeyValuePair<T, U>> protoArray)
        {
            // UnFlattern "tree" here.
            return protoArray.Data.ToDictionary(item => item.Key, item => item.Value);
        }

        #endregion

        #region Multidimensional array handling

        public static ProtoArray<T> ToProtoArray<T>(this Array array)
        {
            // Copy dimensions (to be used for reconstruction).
            var dims = new int[array.Rank];
            for (int i = 0; i < array.Rank; i++) dims[i] = array.GetLength(i);
            // Copy the underlying data.
            var data = new T[array.Length];
            var idx = 0;
            array.MultiLoop(indices => data[idx++] = (T) array.GetValue(indices));

            return new ProtoArray<T> {Dimensions = dims, Data = data};
        }

        public static Array ToArray<T>(this ProtoArray<T> protoArray)
        {
            // Initialize array dynamically.
            var result = Array.CreateInstance(typeof(T), protoArray.Dimensions);
            // Copy the underlying data.
            var k = 0;
            result.MultiLoop(indices => result.SetValue(protoArray.Data[k++], indices));

            return result;
        }

        #endregion

    }

}
