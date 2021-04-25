using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.MyProblem
{
    public class City
    {
        public string Name { get; set; }
        public PointF Location { get; set; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (this == obj) return true;
            if (obj is City city)
            {
                return Equals(city);
            }

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

    public class TravelerProblemChromosome : FitnessComparableChromosome, IGenableChromosome<City>
    {
        private readonly List<City> _genes;

        public TravelerProblemChromosome(IEnumerable<City> cities)
        {
            _genes = cities.ToList();
        }

        public double TotalPath { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<City> Genes => _genes;

        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            return -base.CompareTo(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 1;
            foreach (var gen in _genes)
            {
                hash = HashCode.Combine(hash, gen.GetHashCode());
            }

            return hash;

        }
    }

    public class MyProblemChromosome : FitnessComparableChromosome, IGenableChromosome<double>
    {
        private readonly List<double> _genes = new List<double>();

        /// <inheritdoc />
        public IReadOnlyList<double> Genes => _genes;

        /// <inheritdoc />
        public IChromosome FromGenes(IList<double> genes)
        {
            return new MyProblemChromosome(genes);
        }

        public double X1
        {
            get => _genes[0];
            set => _genes[0] = value;
        }
        public double X2
        {
            get => _genes[1];
            set => _genes[1] = value;
        }
        public double Y1
        {
            get => _genes[2];
            set => _genes[2] = value;
        }
        public double Y2
        {
            get => _genes[3];
            set => _genes[3] = value;
        }

        public MyProblemChromosome(IList<double> genes)
        {
            if (genes.Count != 4) throw new ArgumentException();
            //if (genes.Any(d => d > max || d < min)) throw new ArgumentException();
            _genes = genes as List<double> ?? genes.ToList();
        }

        public MyProblemChromosome(double x1, double x2, double y1, double y2) : this(new List<double>()
        {
            x1,
            x2,
            y1,
            y2
        })
        {


        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{X1}x{X2};{Y1}x{Y2}";
        }
    }
}