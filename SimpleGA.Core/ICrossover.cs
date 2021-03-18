using System.Collections.Generic;

namespace SimpleGA.Core
{
    public interface ICrossover<T> where T : IChromosome
    {
        int RequiredNumberOfParents { get; }
        IEnumerable<T> MakeChildren(IEnumerable<T> parents);
    }
}