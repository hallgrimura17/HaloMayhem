using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null || !enumerable.Any())
                return default;
            return enumerable.ElementAt(Random.Range(0, enumerable.Count()));
        }

        public static T NextElement<T>(this List<T> enumerable)
        {
            var element = enumerable.FirstOrDefault();
            if (element != null)
                enumerable.Remove(element);
            return element;
        }
    }
}