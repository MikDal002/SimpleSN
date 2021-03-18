﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGA.Core
{
    public class Population<T> : IPopulation<T> where T : IChromosome
    {
        private readonly IChromosomeFactory<T> _adamFactory;
        private readonly ICrossover<T> _orderedCrossover;
        private readonly IMutation<T> _mutation;
        private readonly ISelection _selection;
        private Generation<T> _previousGeneration = null;

        public int MinSize { get; }
        public int MaxSize { get; }

        public Population(int minSize, int maxSize, IChromosomeFactory<T> adamFactory,
            ICrossover<T> orderedCrossover, IMutation<T> mutation, ISelection selection)
        {
            _adamFactory = adamFactory ?? throw new ArgumentNullException(nameof(adamFactory));
            _orderedCrossover = orderedCrossover ?? throw new ArgumentNullException(nameof(orderedCrossover));
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
            } else
            {
                // TODO: This shouldn't be hardcoded!
                chromosomesForPopulation.Add(_previousGeneration.BestChromosome);
                for (int i = chromosomesForPopulation.Count; i < MinSize; ++i)
                {
                    var parents = _selection.SelectChromosomes(_previousGeneration, _orderedCrossover.RequiredNumberOfParents);
                    var offsprings = _orderedCrossover.MakeChildren(parents);


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