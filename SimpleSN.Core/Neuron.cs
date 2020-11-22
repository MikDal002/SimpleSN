using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleSN.Core
{
    public struct WeightVectorPair
    {
        public double Weight { get; }
        public double VectorEl { get; }

        public WeightVectorPair(double weight, double vectorEl) : this()
        {
            Weight = weight;
            VectorEl = vectorEl;
        }

    }
    public class Neuron : IComparable<Neuron>
    {
        #region backend of properties
        private double? _lastFitness = null;
        #endregion

        private int _cycleLeftToBeNotTired = 0;
        private int _age = 0;

        public string Name { get; set; } = string.Empty;
        public Func<WeightVectorPair, double> FitnessFunction { get; set; } = (pair) => Math.Pow(pair.VectorEl - pair.Weight, 2);
        public int CyclesNeededToBeNotTired { get; private set; }
        public double AgingFactor { get; private set; } = 0;
        public double LastFitness
        {
            get => _lastFitness == null ? throw new Exception("Nie dokonano obliczeń także nie ma wartości!") : _lastFitness.Value;
            set => _lastFitness = value;
        }

        public List<double> Weights { get; }
        public List<double> LastVector { get; private set; }
        public double LearningImpact { get; private set; }
        public bool IsTired => _cycleLeftToBeNotTired > 0;

        public Neuron(IEnumerable<double> vector, double learningImpact, int cyclesNeededToBeNotTired = 0, double agingFactor = 0)
        {
            CyclesNeededToBeNotTired = cyclesNeededToBeNotTired;
            Weights = vector.ToList();
            LearningImpact = learningImpact;
            AgingFactor = agingFactor;
        }

        public double FitnessForVector(IEnumerable<double> vector)
        {
            Debug.Assert(FitnessFunction != null);
            if (IsTired)
            {
                _cycleLeftToBeNotTired -= 1;
                return Double.NaN;
            }
            LastVector = vector.ToList();
            if (LastVector.Count != Weights.Count) throw new ArgumentException(nameof(vector));

            LastFitness = 0;
            for (int i = 0; i < LastVector.Count; i++)
            {
                LastFitness += FitnessFunction(new WeightVectorPair(Weights[i], LastVector[i]));
            }
            LastFitness = AgingFactor < 0.0001 ? LastFitness : LastFitness * ((1 + AgingFactor) * _age);
            return LastFitness;
        }

        public void Retrain()
        {
            if (IsTired) throw new NeuronTiredException();
            _age++;
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
