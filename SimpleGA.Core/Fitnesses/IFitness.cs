using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Fitnesses
{
    public interface IFitness<T> where T : IChromosome
    {
        double Evaluate(T chromosome);
    }
}