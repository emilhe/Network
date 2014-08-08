using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNetwork
{
    static class Extensions
    {

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
        }

        public static void DynamicLoop<T>(this List<T>[] lists, Action action)
        {
            var current = new T[lists.Length];
            RecursiveLoop(lists, 0, current, action);
        }

        private static void RecursiveLoop<T>(List<T>[] lists, int level, T[] current, Action action)
        {
            if (level == lists.Length) action.Invoke();
            else
            {
                foreach (var s in lists[level])
                {
                    current[level] = s;
                    RecursiveLoop(lists, level + 1, current, action);
                }
            }
        }
    }
}
