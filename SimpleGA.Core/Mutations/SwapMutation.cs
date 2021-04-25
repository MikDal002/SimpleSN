using System;
using System.Linq;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Mutations
{
    public class SwapMutation<T, E> : BaseMutation<T> where T : IGenableChromosome<E>
    {
        private readonly IGenableChromosomeFactory<T, E> _factory;
        public int AmountOfSwaps { get; set; } = 1;

        public SwapMutation(IGenableChromosomeFactory<T, E> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc />
        protected override T MutateImplementation(T offspring)
        {
            var genes = offspring.Genes.ToList();
            var random = new Random();

            for (int i = 0; i < AmountOfSwaps; ++i)
            {
                var one = random.Next(genes.Count);
                var two = random.Next(genes.Count);
                var tmp = genes[one];
                genes[one] = genes[two];
                genes[two] = tmp;
            }

            return _factory.FromGenes(genes);
        }
    }
}