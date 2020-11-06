using System.Collections.Generic;
using System.Linq;

namespace SimpleSN.Core
{
    public static class IEnumerableExtension
    {
        public static string ToCsvLine(this IEnumerable<double> enumerable)
        {
            return enumerable.Select(d => d.ToString()).Aggregate((left, right) => left + "; " + right);
        }
    }

}
