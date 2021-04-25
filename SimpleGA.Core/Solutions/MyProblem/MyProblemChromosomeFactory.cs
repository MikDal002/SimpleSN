using System;
using System.Collections.Generic;
using SimpleGA.Core.Chromosomes;

namespace SimpleGA.Core.Solutions.MyProblem
{
    public class MyProblemChromosomeFactory : IGenableChromosomeFactory<MyProblemChromosome, double>
    {
        static readonly Random random = new Random();
        static readonly int min = 1;
        static readonly int max = 10000;

        /// <inheritdoc />
        public MyProblemChromosome CreateNew()
        {
            return new MyProblemChromosome(GetGene(0), GetGene(1), GetGene(2), GetGene(3));
        }

        public MyProblemChromosome FromGenes(IList<double> genes)
        {
            return new MyProblemChromosome(genes);
        }

        /// <inheritdoc />
        public double GetGene(int geneNumber)
        {
            return random.Next(min, max);
        }
    }
}