using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SimpleSN.Core
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<int> TheBiggestValueThreatAsOneOthersAsZeros<T>(this ICollection<T> list) where T : IComparable<T>
        {
            var biggestElement = list.Max();
            foreach (var el in list) yield return el.CompareTo(biggestElement) == 0 ? 1 : 0;
        }
        public static string ToCsvLine(this IEnumerable<double> enumerable)
        {
            return enumerable.Select(d => d.ToString()).Aggregate((left, right) => left + "; " + right);
        }

        public static int GetArea(this Size size)
        {
            return size.Width * size.Height;
        }
    }

}
