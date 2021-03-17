namespace SimpleGA.Core
{
    public interface ITermination
    {
        bool HasReached(IGeneticAlgorithm geneticAlgorithm);
    }
}