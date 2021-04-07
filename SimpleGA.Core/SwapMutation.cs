using System;
using System.Linq;

namespace SimpleGA.Core
{
    public class SwapMutation<T,E> : BaseMutation<T> where T : IGenableChromosome<E>
    {
        private readonly IGenableChromosomeFactory<T, E> _factory;

        public SwapMutation(IGenableChromosomeFactory<T, E> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc />
        protected override T MutateImplementation(T offspring)
        {
            var genes = offspring.Genes.ToList();
            var random = new Random();
            var one = random.Next(genes.Count);
            var two = random.Next(genes.Count);
            var tmp = genes[one];
            genes[one] = genes[two];
            genes[two] = tmp;
            return _factory.FromGenes(genes);
        }
    }
}