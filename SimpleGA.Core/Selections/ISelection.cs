using System.Collections.Generic;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Selections
{
    public interface ISelection
    {
        IEnumerable<T> SelectChromosomes<T>(Generation<T> previousGeneration, int requiredNumberOfParents)
            where T : IChromosome;
    }
}