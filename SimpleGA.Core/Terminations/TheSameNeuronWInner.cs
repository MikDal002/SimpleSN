using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Terminations
{
    public class TheSameNeuronWInner : ITermination
    {
        private IChromosome _lastKnwonWinner = null;
        private long _amountOfWins = 0;

        public TheSameNeuronWInner(long maxGenerationsCount)
        {
            MaxGenerationsCount = maxGenerationsCount;
        }

        public long MaxGenerationsCount { get; }
        /// <inheritdoc />
        public bool HasReached(IGeneticAlgorithm geneticAlgorithm)
        {
            if (_lastKnwonWinner != geneticAlgorithm.BestChromosome)
            {
                _amountOfWins = 0;
                _lastKnwonWinner = geneticAlgorithm.BestChromosome;
            }

            return (MaxGenerationsCount < ++_amountOfWins);
        }
    }
}