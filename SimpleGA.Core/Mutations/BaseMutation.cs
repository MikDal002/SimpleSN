using System;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Mutations
{
    public abstract class BaseMutation<T> : IMutation<T> where  T : IChromosome
    {
        public double MutationThreshold { get; set; } = 0.1;

        /// <inheritdoc />
        public T? Mutate(T offspring)
        {
            Random rnd = new();
            var shouldMutate = rnd.NextDouble() < MutationThreshold;
            if (shouldMutate)
            {
                return MutateImplementation(offspring);
            }
            return default(T?);
        }

        protected abstract T MutateImplementation(T offspring);
    }
}