using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Solutions.KnapsackProblem
{
    public class KnapsackProblemChromosome : FitnessComparableChromosome, IGenableChromosome<Insert>
    {
        public double MaxWeight { get; }
        private readonly List<Insert> _genes;

        public double TotalProfit { get; set; }
        public double TotalWeight { get; set; }


        /// <inheritdoc />
        public IReadOnlyList<Insert> Genes => _genes;

        public KnapsackProblemChromosome(double maxWeight, IEnumerable<Insert> cities)
        {
            MaxWeight = maxWeight;
            _genes = cities.ToList();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 1;
            foreach (var gen in _genes) hash = HashCode.Combine(hash, gen.GetHashCode());

            return hash;
        }
    }
}