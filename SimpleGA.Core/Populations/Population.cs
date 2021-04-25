using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SimpleGA.Core.Chromosomes;
using SimpleGA.Core.Crossovers;
using SimpleGA.Core.Mutations;
using SimpleGA.Core.Selections;

namespace SimpleGA.Core.Populations
{
    public class Population<T> : IPopulation<T> where T : IChromosome
    {
        private readonly IChromosomeFactory<T> _adamFactory;
        private readonly ICrossover<T> _crossover;
        private readonly IMutation<T> _mutation;
        private readonly ISelection _selection;
        private Generation<T> _previousGeneration = null;

        public int MinSize { get; }
        public int MaxSize { get; }

        public Population(int minSize, int maxSize, IChromosomeFactory<T> adamFactory,
            ICrossover<T> crossover, IMutation<T> mutation, ISelection selection)
        {
            _adamFactory = adamFactory ?? throw new ArgumentNullException(nameof(adamFactory));
            _crossover = crossover ?? throw new ArgumentNullException(nameof(crossover));
            _mutation = mutation;
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            MinSize = minSize;
            MaxSize = maxSize;
        }

        /// <inheritdoc />
        public Generation<T> StartNewGeneration()
        {
            var chromosomesForPopulation = new List<T>(MinSize);
            if (_previousGeneration == null)
            {
                for (int i = 0; i < MinSize; ++i)
                {
                    chromosomesForPopulation.Add(_adamFactory.CreateNew());
                }
            }
            else
            {
                // TODO MD 24-04-2021:  This shouldn't be hardcoded!
                chromosomesForPopulation.Add(_previousGeneration.BestChromosome);
                for (int i = chromosomesForPopulation.Count; i < MinSize; ++i)
                {
                    // ToList jest tymczasowo
                    var parents = _selection.SelectChromosomes(_previousGeneration, _crossover.RequiredNumberOfParents).ToList();
                    if (parents.Count != _crossover.RequiredNumberOfParents)
                    {
                        Debug.WriteLine($"Amount of parents isn't sufficient ({parents.Count} vs {_crossover.RequiredNumberOfParents})!");
                        continue;
                    }

                    var offsprings = _crossover.MakeChildren(parents);


                    foreach (var offspring in offsprings)
                    {
                        var mutatedSprings = _mutation.Mutate(offspring);
                        if (chromosomesForPopulation.Count > MaxSize) break;
                        chromosomesForPopulation.Add(mutatedSprings ?? offspring);
                    }

                }
            }

            return _previousGeneration = new Generation<T>(chromosomesForPopulation);
        }
    }
}