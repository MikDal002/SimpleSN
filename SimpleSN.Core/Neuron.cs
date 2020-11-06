using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleSN.Core
{
    public class Neuron : IComparable<Neuron>
    {
        #region backend of properties
        private double? _lastFitness = null;
        #endregion

        public string Name { get; set; } = string.Empty;
        public int CyclesNeededToBeNotTired { get; private set; }
        public double LastFitness
        {
            get => _lastFitness == null ? throw new Exception("Nie dokonano obliczeń także nie ma wartości!") : _lastFitness.Value;
            set => _lastFitness = value;
        }

        public IList<double> Weights { get; }
        public List<double> LastVector { get; private set; }
        public double LearningImpact { get; }
        public bool IsTired => _cycleLeftToBeNotTired > 0;
        private int _cycleLeftToBeNotTired = 0;

        public Neuron(IEnumerable<double> vector, double learningImpact, int cyclesNeededToBeNotTired = 0)
        {
            CyclesNeededToBeNotTired = cyclesNeededToBeNotTired;
            Weights = vector.ToList();
            LearningImpact = learningImpact;
        }

        public double FitnessForVector(IEnumerable<double> vector)
        {
            if (IsTired) _cycleLeftToBeNotTired -= 1;
            LastVector = vector.ToList();
            if (LastVector.Count != Weights.Count) throw new ArgumentException(nameof(vector));

            LastFitness = 0;
            for (int i = 0; i < LastVector.Count; i++)
            {
                LastFitness += Math.Pow(LastVector[i] - Weights[i], 2);
            }
            return LastFitness;
        }

        public void Retrain()
        {
            if (IsTired) throw new NeuronTiredException();

            _cycleLeftToBeNotTired = CyclesNeededToBeNotTired;
            if (LastVector.Count != Weights.Count) throw new ArgumentException(nameof(LastVector));
            for (int i = 0; i < LastVector.Count; i++)
            {
                Weights[i] = Weights[i] + LearningImpact * (LastVector[i] - Weights[i]);
            }
        }

        public override string ToString()
        {
            return $"{Name}: {Weights.Select(d => d.ToString("n")).Aggregate((l, r) => $"{l}; {r}")}";
        }

        #region IComparable implementation
        public int CompareTo(Neuron other)
        {
            if (other == null) return 1;
            if (other.LastFitness == LastFitness) return 0;
            if (other.LastFitness < LastFitness) return 1;
            else return -1;

        }
        #endregion
    }

}
