using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SimpleSN.Core
{
    public static class IEnumerableExtension
    {
        public static string ToCsvLine(this IEnumerable<double> enumerable)
        {
            return enumerable.Select(d => d.ToString()).Aggregate((left, right) => left + "; " + right);
        }
        public static IEnumerable<double> MultiplyEachElementWith(this IEnumerable<double> leftList, IEnumerable<double> rightList)
        {
            //if (leftList.Count != rightList.Count) throw new InvalidOperationException("Cannot sum two vectors with different length");
            var leftEnum = leftList.GetEnumerator();
            var rightEnum = rightList.GetEnumerator();
            bool leftHasNext;
            bool rightHasNext;
            do
            {
                leftHasNext = leftEnum.MoveNext();
                rightHasNext = rightEnum.MoveNext();
                if ((leftHasNext && !rightHasNext) || (!leftHasNext && rightHasNext)) throw new InvalidOperationException("Cannot sum two vectors with different length");
                else if (!leftHasNext && !rightHasNext) break;
                var lc = leftEnum.Current;
                var rc = rightEnum.Current;
                yield return lc * rc;
            }
            while (leftHasNext && rightHasNext);
        }

        public static IEnumerable<double> MultiplyEachElementWith(this ICollection<double> leftList, ICollection<double> rightList)
        {
            if (leftList.Count != rightList.Count) throw new InvalidOperationException("Cannot sum two vectors with different length");
            for(int i = 0; i < leftList.Count; ++i)
            {
                yield return leftList.ElementAt(i) * rightList.ElementAt(i);
            }
        }

        public static int GetArea(this Size size)
        {
            return size.Width * size.Height;
        }
    }

}
