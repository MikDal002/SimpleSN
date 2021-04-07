using System.Collections.Generic;

namespace SimpleGA.Core
{
    public interface IGenableChromosome<T> : IChromosome
    {
        public IReadOnlyList<T> Genes { get; }

        
    }
}