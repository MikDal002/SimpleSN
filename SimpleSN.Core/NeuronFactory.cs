using System;
using System.Collections.Generic;

namespace SimpleSN.Core
{
    public class NeuronFactory
    {
        public static IEnumerable<Neuron> GenerateNeurons(int numberOfNeurons, int numberOfDendryds, int tiredness = 0, double learningImpact = 0.5, double minValueOfWeight = double.MinValue, double maxValueOfWeights = double.MaxValue, double agingFactor = 0.0,
            Func<WeightVectorPair, double>? fitnessFunction = null)
        {
            var rnd = new Random();
            for (int i = 0; i < numberOfNeurons; i++)
            {
                var nr = new Neuron(rnd.GetManyNextDoubleFromRange(numberOfDendryds, minValueOfWeight, maxValueOfWeights), learningImpact, tiredness, agingFactor);
                if (fitnessFunction != null) nr.FitnessFunction = fitnessFunction;
                nr.Name = $"Numer {i}";
                yield return nr;
            }
        }
        public static IEnumerable<Neuron> GetFromLab1(int tiredness = 0)
        {
            var matrix = new[] { new[] { 0.2, 0.8, 0.7, 0.1 },
                                 new[] { 0.4, 0.4, 0.2, 0.8 },
                                 new[] { 0.8, 0.8, 0.1, 0.5 } };
            int i = 0;
            var learningImpact = 0.5;

            foreach (var vector in matrix)
                yield return new Neuron(vector, learningImpact, tiredness) { Name = $"Neuron {++i}" };
        }


        public static IEnumerable<Neuron> FromVectors(IEnumerable<List<double>> enumerable, double learningImpact, int tiredness, double agingFactor = 0.0)
        {
            int i = 0;
            foreach (var vector in enumerable)
                yield return new Neuron(vector, learningImpact, tiredness, agingFactor) { Name = $"Nr {++i}" };
        }
    }

}
