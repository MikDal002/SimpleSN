using System;

namespace SimpleGA.Core
{
    public interface IChromosome : IComparable
    {
        double? Fitness { get; set; }
    }

    public abstract class FitnessComparableChromosome : IChromosome
    {
        /// <inheritdoc />
        public virtual int CompareTo(object? obj)
        {
            if (obj is IChromosome chrom)
            {
                return Fitness!.Value.CompareTo(chrom.Fitness!.Value);
            }
            else throw new ArgumentException(nameof(obj));
        }

        /// <inheritdoc />
        public double? Fitness { get; set; }
    }
}