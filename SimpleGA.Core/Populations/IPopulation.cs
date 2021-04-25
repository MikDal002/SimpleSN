using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Populations
{
    public interface IPopulation<T> where T : IChromosome
    {
        Generation<T> StartNewGeneration();
    }
}