using System;

namespace SimpleGA.Core
{
    public class GenerateCompletelyNewValuesMutation : IMutation<MyProblemChromosome>
    {
        private const double MutationThreshold = 0.1;
        /// <inheritdoc />
        public MyProblemChromosome? Mutate(MyProblemChromosome offspring)
        {
            Random rnd = new Random();
            var shouldMutate = rnd.NextDouble() < MutationThreshold;
            if (shouldMutate)
            {
                return new MyProblemChromosome(null, null, null, null);
            }
            return null;
        }
    }
}