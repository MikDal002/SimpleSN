using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Crossovers
{
    public class SinglePointCrossover<T, E> : MultiPointCrossover<T, E> where T : IGenableChromosome<E>
    {
        public SinglePointCrossover(IGenableChromosomeFactory<T, E> factory) : base(1, factory)
        {
        }
    }
}