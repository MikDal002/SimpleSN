using System;
using System.Diagnostics;
using System.Linq;
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
}