using System;
using System.Collections.Generic;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Solutions.MyProblem
{
    public class SimpleChromosome : FitnessComparableChromosome, IGenableChromosome<bool>
    {
        public SimpleChromosome(List<bool> genes)
        {
            Genes = genes;
        }

        public override int GetHashCode()
        {
            int hash = 1;
            foreach (var gen in Genes) hash = HashCode.Combine(hash, gen.GetHashCode());

            return hash;
        }

        public IReadOnlyList<bool> Genes { get; }


        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            return -base.CompareTo(obj);
        }
    }
}