using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleSN.Core
{
    public class Trainer
    {
        public List<Neuron> Neurons { get; private set; }
        private List<List<double>> LearningVectors;

        public event EventHandler<Trainer> IteractionStarting;
        public event EventHandler<Trainer> IteractionFinished;
        public event EventHandler<Neuron> IteractionWinner;
        public event EventHandler<Trainer> TrainingFinished;

        public int Iteration { get; private set; }

        public Trainer()
        {

        }

        public void Train(IEnumerable<Neuron> _neurons, IEnumerable<IEnumerable<double>> _learningVectors)
        {
            PrepareData(_neurons, _learningVectors);
            Iteration = 0;
            foreach (var learningVector in LearningVectors)
            {
                Iteration += 1;
                IteractionStarting?.Invoke(this, this);
                Neurons.ForEach(d => d.FitnessForVector(learningVector));
                var theBestNeurone = Neurons.Where(d => !d.IsTired).Min();

                // All neurons are tired! Awsome!
                if (theBestNeurone == null) continue;
                else theBestNeurone.Retrain();

                IteractionWinner?.Invoke(this, theBestNeurone);
                IteractionFinished?.Invoke(this, this);
            }
            TrainingFinished?.Invoke(this, this);
        }

        private void PrepareData(IEnumerable<Neuron> _neurons, IEnumerable<IEnumerable<double>> _learningVectors)
        {
            Neurons = _neurons.ToList();
            LearningVectors = _learningVectors.Select(d => d.ToList()).ToList();
            var neuronsDendryds = Neurons.First().Weights.Count;
            if (Neurons.Any(d => d.Weights.Count != neuronsDendryds)) throw new ArgumentException(nameof(Neurons));
            if (LearningVectors.Any(vector => vector.Count() != neuronsDendryds)) throw new ArgumentException(nameof(LearningVectors));
        }
    }

}
