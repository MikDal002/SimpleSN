namespace SimpleGA.Core
{
    public interface IFitness<T> where T : IChromosome
    {
        double Evaluate(T chromosome);
    }
}