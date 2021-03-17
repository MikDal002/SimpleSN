namespace SimpleGA.Core
{
    public interface IPopulation<T> where T : IChromosome
    {
        Generation<T> StartNewGeneration();
    }
}