#nullable enable
namespace SimpleGA.Core
{
    public interface IMutation<T> where T : IChromosome
    {
        T? Mutate(T offspring);
    }
}