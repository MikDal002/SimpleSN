using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGA.GUI
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this ICollection<T> input)
        {
            var allNumbers = Enumerable.Range(0, input.Count).ToList();
            Random rnd = new Random();
            for (int i = 0; i < input.Count; ++i)
            {
                var selectedNumber = rnd.Next(0, allNumbers.Count);
                yield return input.ElementAt(allNumbers[selectedNumber]);
                allNumbers.RemoveAt(selectedNumber);
            }
        }
    }
}