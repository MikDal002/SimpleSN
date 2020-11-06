using System;
using System.Collections.Generic;

namespace SimpleSN.Core
{
    public static class RandomExtension
    {
        public static double GetNextDoubleFromRange(this Random rnd, double minimum, double maximum)
        {
            return rnd.NextDouble() * (maximum - minimum) + minimum;
        }

        public static IEnumerable<double> GetManyNextDoubleFromRange(this Random rnd, double amount, double minimum, double maximum)
        {
            for (int i = 0; i < amount; i++) yield return rnd.NextDouble() * (maximum - minimum) + minimum;
        }
    }

}
