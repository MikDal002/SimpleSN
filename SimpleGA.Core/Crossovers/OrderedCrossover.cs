using System;
using System.Collections.Generic;
using System.Linq;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Crossovers
{
    public class OrderedCrossover<T, E> : ICrossover<T> where T : IGenableChromosome<E>
    {
        private readonly IGenableChromosomeFactory<T, E> _factory;

        public OrderedCrossover(IGenableChromosomeFactory<T, E> chromosomeFactory)
        {
            _factory = chromosomeFactory;
        }

        /// <inheritdoc />
        public int RequiredNumberOfParents { get; } = 2;

        /// <inheritdoc />
        public IEnumerable<T> MakeChildren(IEnumerable<T> parentsRaw)
        {
            var parents = parentsRaw.ToList();
            if (parents.Count != RequiredNumberOfParents)
                throw new ArgumentException("The number of parents isn't sufficient", nameof(parentsRaw));
            if (parents[0].Genes.Count != parents[1].Genes.Count)
                throw new ArgumentException("Different size of genes is not supported here!");

            var maxCount = parents[0].Genes.Count;
            var random = new Random();


            var begining = random.Next((maxCount - 1) / 2);
            var end = random.Next(begining, maxCount - 1);

            var child1PrimeGenes = parents[0].Genes.Skip(begining).Take(end - begining).ToList();
            var child2PrimeGenes = parents[1].Genes.Skip(begining).Take(end - begining).ToList();

            Func<IEnumerable<E>> parent1FilteredGens = () => parents[0].Genes.Where(d => !child2PrimeGenes.Contains(d));
            Func<IEnumerable<E>> parent2FilteredGens = () => parents[1].Genes.Where(d => !child1PrimeGenes.Contains(d));

            var child1Genes =
                parent2FilteredGens().Take(begining)
                    .Concat(child1PrimeGenes)
                    .Concat(parent2FilteredGens().Skip(begining).Take(maxCount - (begining + child1PrimeGenes.Count)));

            var child2Genes =
                parent1FilteredGens().Take(begining)
                    .Concat(child2PrimeGenes)
                    .Concat(parent1FilteredGens().Skip(begining).Take(maxCount - (begining + child2PrimeGenes.Count)));

            yield return _factory.FromGenes(child1Genes.ToList());
            yield return _factory.FromGenes(child2Genes.ToList());
        }
    }
}