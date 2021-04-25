namespace SimpleGA.Core.Terminations
{
    public class GenerationNumberTermination : ITermination
    {
        public int MaxGenerationsCount { get; }

        public GenerationNumberTermination(int maxGenerationsCount)
        {
            MaxGenerationsCount = maxGenerationsCount;
        }

        /// <inheritdoc />
        public bool HasReached(IGeneticAlgorithm geneticAlgorithm)
        {
            return geneticAlgorithm.GenerationsNumber > MaxGenerationsCount;
        }
    }
}