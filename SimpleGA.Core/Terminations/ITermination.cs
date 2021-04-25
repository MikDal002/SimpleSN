namespace SimpleGA.Core.Terminations
{
    public interface ITermination
    {
        bool HasReached(IGeneticAlgorithm geneticAlgorithm);
    }
}