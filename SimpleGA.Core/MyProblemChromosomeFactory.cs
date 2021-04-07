using System;
using System.Collections.Generic;

namespace SimpleGA.Core
{
    public class MyProblemChromosomeFactory : IGenableChromosomeFactory<MyProblemChromosome, double>
    {
        static Random random = new Random();
        static int min = 1;
        static int max = 10000;

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