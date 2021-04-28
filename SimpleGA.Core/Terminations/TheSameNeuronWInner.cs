using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Terminations
{
    public class TheSameNeuronWinner : ITermination
    {
        public long AmountLastNeruonWins { get; private set; } = 0;
        private IChromosome _lastKnwonWinner = null;

        public long MaxGenerationsCount { get; }

        public TheSameNeuronWinner(long maxGenerationsCount)
        {
            MaxGenerationsCount = maxGenerationsCount;
        }

        /// <inheritdoc />
        public bool HasReached(IGeneticAlgorithm geneticAlgorithm)
        {
            if (_lastKnwonWinner != geneticAlgorithm.BestChromosome)
            {
                AmountLastNeruonWins = 0;
                _lastKnwonWinner = geneticAlgorithm.BestChromosome;
            }

            return MaxGenerationsCount < ++AmountLastNeruonWins;
        }
    }
}