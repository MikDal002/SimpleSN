using SimpleGA.Core.Chromosomes;

#nullable enable
namespace SimpleGA.Core.Mutations
{
    public interface IMutation<T> where T : IChromosome
    {
        T? Mutate(T offspring);
    }
}