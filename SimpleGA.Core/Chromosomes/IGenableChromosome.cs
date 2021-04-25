using System.Collections.Generic;

namespace SimpleGA.Core.Chromosomes
{
    public interface IGenableChromosome<T> : IChromosome
    {
        public IReadOnlyList<T> Genes { get; }

        
    }
}