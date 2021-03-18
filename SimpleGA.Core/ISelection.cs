using System.Collections.Generic;

namespace SimpleGA.Core
{
    public interface ISelection
    {
        IEnumerable<T> SelectChromosomes<T>(Generation<T> previousGeneration, int requiredNumberOfParents) where T : IChromosome;
    }
}