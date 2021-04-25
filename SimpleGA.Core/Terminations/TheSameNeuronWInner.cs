using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Terminations
{
    public class TheSameNeuronWInner : ITermination
    {
        public long AmountLastNeruonWins { get; private set; } = 0;
        private IChromosome _lastKnwonWinner = null;

        public long MaxGenerationsCount { get; }

        public TheSameNeuronWInner(long maxGenerationsCount)
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