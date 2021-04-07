using System;
using System.Linq;

namespace SimpleGA.Core
{
    public class RandomResettingMutation<T,E> : BaseMutation<T> where T : IGenableChromosome<E>
    {
        private readonly IGenableChromosomeFactory<T, E> _factory;

        public RandomResettingMutation(IGenableChromosomeFactory<T, E> factory)
        {
            _factory = factory;
        }

        protected override T MutateImplementation(T offspring)
        {
            Random rnd = new();
            var genes = offspring.Genes.ToList();
            var geneToMutate = rnd.Next(genes.Count);
            genes[geneToMutate] = _factory.GetGene(geneToMutate);
            return _factory.FromGenes(genes);
        }
    }
}