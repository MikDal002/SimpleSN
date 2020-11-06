using System;
using System.Collections.Generic;

namespace SimpleSN.Core
{
    public class NeuronFactory
    {
        public static IEnumerable<Neuron> GenerateNeurons(int numberOfNeurons, int numberOfDendryds, double minValueOfWeight = double.MinValue, double maxValueOfWeights = double.MaxValue)
        {
            var learningImpact = 0.5;
            var rnd = new Random();
            for (int i = 0; i < numberOfNeurons; i++)
                yield return new Neuron(rnd.GetManyNextDoubleFromRange(numberOfDendryds, minValueOfWeight, maxValueOfWeights), learningImpact);
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
    }

}
