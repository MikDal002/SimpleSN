using System;

namespace SimpleGA.Core
{
    public class MyProblemChromosome : IChromosome
    {
        public double X1 { get; private set; }
        public double X2 { get; private set; }
        public double Y1 { get; private set; }
        public double Y2 { get; private set; }

        /// <inheritdoc />
        public double? Fitness { get; set; }


        public MyProblemChromosome(double? x1, double? x2, double? y1, double? y2)
        {
            var random = new Random();
            var min = 1;
            var max = 10000;
            X1 = x1 ?? random.Next(min, max);
            X2 = x2 ?? random.Next(min, max);
            Y1 = y1 ?? random.Next(min, max);
            Y2 = y2 ?? random.Next(min, max);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{X1}x{X2};{Y1}x{Y2}";
        }
    }
}