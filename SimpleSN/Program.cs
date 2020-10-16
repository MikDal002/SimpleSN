using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace SimpleSN
{
    public class Neuron : IComparable<Neuron>
    {
        #region backend of properties
        private double? _lastFitness = null;
        #endregion

        public string Name { get; set; } = string.Empty;
        public double LastFitness
        {
            get => _lastFitness == null ? throw new Exception("Nie dokonano obliczeń także nie ma wartości!") : _lastFitness.Value;
            set => _lastFitness = value;
        }

        public IList<double> Weights { get; }
        public List<double> LastVector { get; private set; }
        public double LearningImpact { get; }

        public Neuron(IEnumerable<double> vector, double learningImpact)
        {
            Weights = vector.ToList();
            LearningImpact = learningImpact;
        }

        public double FitnessForVector(IEnumerable<double> vector)
        {
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
        public int CompareTo([AllowNull] Neuron other)
        {
            if (other == null) return 1;
            if (other.LastFitness == LastFitness) return 0;
            if (other.LastFitness < LastFitness) return 1;
            else return -1;

        }
        #endregion
    }

    public class NeuronFactory
    {
        internal static IEnumerable<Neuron> GetFromLab1()
        {
            var matrix = new[] { new[] { 0.2, 0.8, 0.7, 0.1 }, 
                                 new[] { 0.4, 0.4, 0.2, 0.8 }, 
                                 new[] { 0.8, 0.8, 0.1, 0.5 } };
            int i = 0;
            var learningImpact = 0.5;

            foreach (var vector in matrix) 
                yield return new Neuron(vector, learningImpact) { Name = $"Neuron {++i}" };
        }
    }

    public class VectorsFactory
    {
        internal static IEnumerable<double[]> GetFromLab1()
        {
            yield return new[] { 0.4, 0.3, 0.1, 0.7 };
            yield return new[] { 0.8, 0.8, 0.8, 0.7 };
            yield return new[] { 0.2, 0.5, 0.2, 0.1 };
            yield return new[] { 0.5, 0.3, 0.2, 0.7 };
        }
    }

    class Trainer
    {
        private readonly List<Neuron> Neurons;
        private readonly List<List<double>> LearningVectors;

        public event EventHandler<Trainer> IteractionStarting;
        public event EventHandler<Trainer> IteractionFinished;
        public event EventHandler<Neuron> IteractionWinner;
        public event EventHandler<Trainer> TrainingFinished;

        public double Iteration { get; private set; }

        public Trainer(IEnumerable<Neuron> _neurons, IEnumerable<IEnumerable<double>> _learningVectors)
        {
            Neurons = _neurons.ToList();
            LearningVectors = _learningVectors.Select(d => d.ToList()).ToList();
            var neuronsDendryds = Neurons.First().Weights.Count;
            if (Neurons.Any(d => d.Weights.Count != neuronsDendryds)) throw new ArgumentException(nameof(Neurons));
            if (LearningVectors.Any(vector => vector.Count() != neuronsDendryds)) throw new ArgumentException(nameof(LearningVectors));
        }

        public void Train()
        {
            Iteration = 0;
            foreach (var learningVector in LearningVectors)
            {
                Iteration += 1;
                IteractionStarting?.Invoke(this, this);
                var theBestNeurone = Neurons.Min(d => { d.FitnessForVector(learningVector); return d; });

                theBestNeurone.Retrain();
                IteractionWinner?.Invoke(this, theBestNeurone);
                IteractionFinished?.Invoke(this, this);
            }
            TrainingFinished?.Invoke(this, this);
        }
    }

    class Program
    {
        static void Main(string[] _)
        {
            var neurons = NeuronFactory.GetFromLab1().ToList();
            var learningVectors = VectorsFactory.GetFromLab1();

            if (neurons.Select(d => d.Weights.Count).Distinct().Count() != 1) throw new Exception("Któryś neuron ma złą liczbę dendrydów");

             var trainer = new Trainer(neurons, learningVectors);
             trainer.IteractionStarting += (sender, trainer) => Console.WriteLine($"Iteration {trainer.Iteration} started…");
             trainer.IteractionWinner += (sender, winner) => Console.WriteLine($"Iteration won: {winner.ToString()}");
            trainer.TrainingFinished += (sender, trainer) =>
            {
                Console.WriteLine($"Training finished in {trainer.Iteration} iterations");
                Console.WriteLine("All vectors after learning:");
                neurons.ForEach(d => Console.WriteLine(d.ToString()));
            };
            trainer.Train();

            // foreach (var learningVector in learningVectors)
            // {
            //     var theBestNeurone = neurons.Min(d => { d.FitnessForVector(learningVector); return d; });
            //     Console.WriteLine($"Winning neuron is: {theBestNeurone.Name} with fitness {theBestNeurone.LastFitness:n}");
            //     Console.WriteLine($"Old weights are: {theBestNeurone.Weights.Aggregate("", (l, r) => $"{l:n}; {r:n}")}");
            //     theBestNeurone.Retrain();
            //     Console.WriteLine($"New weights are: {theBestNeurone.Weights.Aggregate("", (l, r) => $"{l:n}; {r:n}")}");
            //     Console.WriteLine(Environment.NewLine);
            // }
            // Console.WriteLine("All vectors after learning:");
            // neurons.ForEach(d => Console.WriteLine(d.ToString()));
        }
    }
}
