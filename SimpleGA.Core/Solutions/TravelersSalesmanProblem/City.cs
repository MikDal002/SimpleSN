using System;
using System.Drawing;

namespace SimpleGA.Core.Solutions.MyProblem
{
    public class City
    {
        public string Name { get; set; }
        public PointF Location { get; set; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (this == obj) return true;
            if (obj is City city) return Equals(city);

            throw new ArgumentException(nameof(obj));
        }

        protected bool Equals(City other)
        {
            return Location.Equals(other.Location);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Location);
        }
    }
}