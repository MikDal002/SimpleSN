namespace SimpleGA.Core
{
    public interface IChromosomeFactory<T> where T : IChromosome
    {
        T CreateNew();
    }
}