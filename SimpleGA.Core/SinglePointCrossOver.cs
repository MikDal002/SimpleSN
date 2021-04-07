using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGA.Core
{
    public class SinglePointCrossOver<T,E> : ICrossover<T> where T : IGenableChromosome<E>
    {
        private IGenableChromosomeFactory<T, E> _factory;

        /// <inheritdoc />
        public int RequiredNumberOfParents => 2;

        public SinglePointCrossOver(IGenableChromosomeFactory<T, E> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc />
        public IEnumerable<T> MakeChildren(IEnumerable<T> parents)
        {
            var list = parents.Take(RequiredNumberOfParents).ToList();
            if (list.Count != RequiredNumberOfParents)
                throw new ArgumentException("The number of parents isn't sufficient", nameof(parents));
            if (list[0].Genes.Count != list[1].Genes.Count)
                throw new ArgumentException("Different size of genes is not supported here!");

            var maxCount = list[0].Genes.Count;
            var childGenes1 = new List<E>();
            var childGenes2 = new List<E>();
            var random = new Random();

            var splitPoint = random.Next(maxCount - 1);

            for (int i = 0; i < maxCount; ++i)
            {
                int next = i > splitPoint ? 0 : 1;
                childGenes1.Add(next == 0 ? list[0].Genes[i] : list[1].Genes[i]);
                childGenes2.Add(next == 1 ? list[0].Genes[i] : list[1].Genes[i]);
            }

            yield return _factory.FromGenes(childGenes1);
            yield return _factory.FromGenes(childGenes2);
        }
    }
}