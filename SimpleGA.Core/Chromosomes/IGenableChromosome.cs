using System.Collections.Generic;
using Newtonsoft.Json;

namespace SimpleGA.Core.Chromosomes
{
    public interface IGenableChromosome<T> : IChromosome
    {
        [JsonIgnore] public IReadOnlyList<T> Genes { get; }
    }
}