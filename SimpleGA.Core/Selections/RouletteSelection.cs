using System;
using System.Collections.Generic;
using System.Linq;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Selections
{
    public class RouletteSelection : ISelection
    {
        private readonly Random _random = new();
        public bool? IsReversed { get; set; } = null;

        /// <inheritdoc />
        public IEnumerable<T> SelectChromosomes<T>(Generation<T> previousGeneration, int requiredNumberOfParents)
            where T : IChromosome
        {
            if (IsReversed == null)
            {
                var min = previousGeneration.Min();
                var max = previousGeneration.Max();
                IsReversed = max.Fitness < min.Fitness;
            }

            var sumOfFitnesse = 0.0;

            foreach (var chrom in previousGeneration)
            {
                if (!chrom.Fitness.HasValue)
                    throw new ArgumentException("Chromosome doesn't have fitness calculated!");

                sumOfFitnesse += chrom.Fitness.Value;
            }

            if (IsReversed == true) sumOfFitnesse = 1.0 / sumOfFitnesse;

            var parentThresholds = new List<double>(requiredNumberOfParents);

            for (int i = 0; i < requiredNumberOfParents; ++i)
                parentThresholds.Add(_random.NextDouble() * sumOfFitnesse);

            parentThresholds = parentThresholds.OrderBy(d => d).ToList();

            var minimumParentThreshold = parentThresholds[0];

            var selectionProgress = 0.0;
            foreach (var chrom in previousGeneration)
            {
                selectionProgress += IsReversed == true ? 1.0 / chrom.Fitness!.Value : chrom.Fitness!.Value;
                if (minimumParentThreshold > selectionProgress) continue;

                parentThresholds.RemoveAt(0);
                yield return chrom;

                if (parentThresholds.Count == 0) yield break;
                minimumParentThreshold = parentThresholds[0];
            }
        }
    }
}