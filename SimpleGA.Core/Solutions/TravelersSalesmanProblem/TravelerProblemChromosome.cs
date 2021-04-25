using System;
using System.Collections.Generic;
using System.Linq;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Solutions.MyProblem
{
    public class TravelerProblemChromosome : FitnessComparableChromosome, IGenableChromosome<City>
    {
        private readonly List<City> _genes;

        public double TotalPath { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<City> Genes => _genes;

        public TravelerProblemChromosome(IEnumerable<City> cities)
        {
            _genes = cities.ToList();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 1;
            foreach (var gen in _genes) hash = HashCode.Combine(hash, gen.GetHashCode());

            return hash;
        }


        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            return -base.CompareTo(obj);
        }
    }
}