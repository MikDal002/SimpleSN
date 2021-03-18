using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleGA.Core
{
    public class RouletteSelection : ISelection
    {
        /// <inheritdoc />
        public IEnumerable<T> SelectChromosomes<T>(Generation<T> previousGeneration, int requiredNumberOfParents) where T : IChromosome
        {
            var sumOfFitnesse = 0.0;
            foreach (var chrom in previousGeneration)
            {
                if (!chrom.Fitness.HasValue) throw new ArgumentException("Chromosome doesn't have fitness calculated!");

                sumOfFitnesse += chrom.Fitness.Value;
            }

            var parentThresholds = new List<double>(requiredNumberOfParents);
            var rnd = new Random();
            for (int i = 0; i < requiredNumberOfParents; ++i)
            {
                parentThresholds.Add(rnd.NextDouble() * sumOfFitnesse);
            }

            parentThresholds = parentThresholds.OrderBy(d => d).ToList();

            var minimumParentThreshold = parentThresholds[0];

            var selectionProgress = 0.0;
            foreach (var chrom in previousGeneration)
            {
                selectionProgress += chrom.Fitness!.Value;
                if (minimumParentThreshold > selectionProgress) continue;

                parentThresholds.RemoveAt(0);
                yield return chrom;
                
                if (parentThresholds.Count == 0) yield break;
                minimumParentThreshold = parentThresholds[0];
            }
        }
    }
}