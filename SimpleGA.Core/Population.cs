using System.Collections.Generic;

namespace SimpleGA.Core
{
    public class Population<T> : IPopulation<T> where T : IChromosome
    {
        private readonly IChromosomeFactory<T> _adamFactory;
        private Generation<T> _previousGeneration = null;

        public int MinSize { get; }
        public int MaxSize { get; }

        public Population(int minSize, int maxSize, IChromosomeFactory<T> adamFactory)
        {
            _adamFactory = adamFactory;
            MinSize = minSize;
            MaxSize = maxSize;
        }

        /// <inheritdoc />
        public Generation<T> StartNewGeneration()
        {
            var chromosomesForPopulation = new List<T>(MinSize);
            for (int i = 0; i < MinSize; ++i)
            {
                chromosomesForPopulation.Add(_adamFactory.CreateNew());
            }

            return _previousGeneration = new Generation<T>(chromosomesForPopulation);
        }
    }
}