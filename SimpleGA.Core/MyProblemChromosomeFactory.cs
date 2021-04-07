using System;
using System.Collections.Generic;

namespace SimpleGA.Core
{
    public class MyProblemChromosomeFactory : IGenableChromosomeFactory<MyProblemChromosome, double>
    {
        /// <inheritdoc />
        public MyProblemChromosome CreateNew()
        {

            return new MyProblemChromosome(null, null, null, null);
        }

        public MyProblemChromosome FromGenes(IList<double> genes)
        {
            return new MyProblemChromosome(genes);
        }
    }
}