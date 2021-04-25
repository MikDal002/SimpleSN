using System;
using System.Drawing;

namespace SimpleGA.Core.Solutions.KnapsackProblem
{
    public class Insert
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public double Profit { get; set; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (this == obj) return true;
            if (obj is Insert city) return Equals(city);

            throw new ArgumentException(nameof(obj));
        }

        protected bool Equals(Insert other)
        {
            return Name.Equals(other.Name) && Weight.Equals(other.Weight) && Profit.Equals(other.Profit);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Profit, Weight);
        }
    }
}