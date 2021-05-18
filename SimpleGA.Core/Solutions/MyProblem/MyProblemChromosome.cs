using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SimpleGA.Core.Chromosomes;
using SimpleGA.Core.Extensions;
using SimpleGA.Core.Fitnesses;

namespace SimpleGA.Core.Solutions.MyProblem
{
    /// <summary>
    ///     http://www.geatbx.com/ver_3_5/fcnfun7.html
    /// </summary>
    public class SchwefelChromosomeFitness : IFitness<SimpleChromosome>
    {
        /// <inheritdoc />
        public double Evaluate(SimpleChromosome chromosome)
        {
            if (chromosome.Genes.Count != 64) throw new Exception();

            var values = new[]
            {
                BitConverter.ToSingle(chromosome.Genes.Take(32).ToBytes()),
                BitConverter.ToSingle(chromosome.Genes.Skip(32).ToBytes()),
            };

            double sum = 0;
            for (int i = 0; i < values.Length; i++) sum += -values[i] * Math.Sin(Math.Sqrt(Math.Abs(values[i])));
            //var result = 418.9829 * values.Length + sum;
            var result = sum;
            if (double.IsNaN(result))
            {
                Debug.WriteLine("No i problem, bo wyszło NaN...");
                return double.PositiveInfinity;
            }

            if (result < -840) Debug.WriteLine("Aha?");

            return result;
        }
    }

    /// <summary>
    ///     https://www.sfu.ca/~ssurjano/mccorm.html
    /// </summary>
    public class MCCORMICKChromosomeFitness : IFitness<SimpleChromosome>
    {
        /// <inheritdoc />
        public double Evaluate(SimpleChromosome chromosome)
        {
            if (chromosome.Genes.Count != 64) throw new Exception();


            var val1 = BitConverter.ToSingle(chromosome.Genes.Take(32).ToBytes());
            var val2 = BitConverter.ToSingle(chromosome.Genes.Skip(32).Take(32).ToBytes());
            var result = Math.Sin(val1 + val2) + Math.Pow(val1 - val2, 2) - 1.5 * val1 + 2.5 * val2 + 1;
            if (double.IsNaN(result))
            {
                Debug.WriteLine("No i problem, bo wyszło NaN...");
                return double.PositiveInfinity;
            }

            return result;
        }
    }

    public class BitChromosomeFactory : IGenableChromosomeFactory<SimpleChromosome, bool>
    {
        private readonly int _length;
        private readonly Random _random;

        public BitChromosomeFactory(int length)
        {
            _length = length;
            _random = new Random();
        }

        /// <inheritdoc />
        public virtual SimpleChromosome CreateNew()
        {
            return new SimpleChromosome(Enumerable.Range(0, _length).Select(d => GetGene(d)).ToList());
        }

        /// <inheritdoc />
        public virtual SimpleChromosome FromGenes(IList<bool> genes)
        {
            if (genes.Count != _length) throw new Exception();
            return new SimpleChromosome(genes.ToList());
        }

        /// <inheritdoc />
        public virtual bool GetGene(int geneNumber)
        {
            return _random.Next() % 2 == 0;
        }
    }


    public class FloatChromosomeFactory : BitChromosomeFactory
    {
        public int Length { get; }
        public float Max { get; set; } = 500;
        public float Min { get; set; } = -500;


        public FloatChromosomeFactory() : this(2) { }

        /// <inheritdoc />
        public FloatChromosomeFactory(int length) : base(length * 32)
        {
            Length = length;
        }

        /// <inheritdoc />
        public override SimpleChromosome CreateNew()
        {
            var random = new Random();
            List<float> gensDoubles = new List<float>();
            for (int i = 0; i < Length; ++i)
            {
                var value = random.NextDouble() * (Max - Min) + Min;
                gensDoubles.Add((float) value);
            }

            var bytes = gensDoubles.SelectMany(d => BitConverter.GetBytes(d)).ToArray();
            var bitArray = new BitArray(bytes);

            var retList = new List<bool>();
            foreach (var bit in bitArray)
            {
                var boolean = (bool) bit;
                retList.Add(boolean);
            }

            return base.FromGenes(retList);
        }

        /// <inheritdoc />
        public override SimpleChromosome FromGenes(IList<bool> genes)
        {
            if (genes.Count % 32 != 0) throw new Exception("Amount of bits must be divisible by 32!");

            for (int i = 0; i < genes.Count; i += 32)
            {
                var dbl = BitConverter.ToSingle(genes.Skip(i).Take(32).ToBytes());
                if (dbl < Min || dbl > Max) return CreateNew();
                if (double.IsNaN(dbl)) return CreateNew();
            }

            return base.FromGenes(genes);
        }
    }

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

    public class MyProblemChromosome : FitnessComparableChromosome, IGenableChromosome<double>
    {
        private readonly List<double> _genes = new List<double>();

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

        public MyProblemChromosome(double x1, double x2, double y1, double y2) : this(new List<double>
        {
            x1,
            x2,
            y1,
            y2
        }) { }

        /// <inheritdoc />
        public IChromosome FromGenes(IList<double> genes)
        {
            return new MyProblemChromosome(genes);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{X1}x{X2};{Y1}x{Y2}";
        }

        /// <inheritdoc />
        public IReadOnlyList<double> Genes => _genes;
    }
}