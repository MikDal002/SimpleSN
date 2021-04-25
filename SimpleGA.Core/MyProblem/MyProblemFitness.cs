using System;
using SimpleGA.Core.Fitnesses;

namespace SimpleGA.Core.MyProblem
{
    public class MyProblemFitness : IFitness<MyProblemChromosome>
    {
        /// <inheritdoc />
        public double Evaluate(MyProblemChromosome chromosome)
        {
            return Math.Sqrt(Math.Pow(chromosome.X1 - chromosome.X2, 2) + Math.Pow(chromosome.Y1 - chromosome.Y2, 2));
        }
    }
}