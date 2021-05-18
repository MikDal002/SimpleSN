using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleGA.Core.Extensions;

namespace SimpleGA.Core.Solutions.MyProblem
{
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
}