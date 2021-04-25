using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core
{
    public interface IGeneticAlgorithm
    {
        public int GenerationsNumber { get; }
        IChromosome BestChromosome { get; }
    }
}