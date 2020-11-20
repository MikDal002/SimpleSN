using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleSN.Core
{
    public class AbstractTrainer
    {
        public int Iteration { get; protected set; }

        public event EventHandler<AbstractTrainer> IteractionStarting;
        public event EventHandler<AbstractTrainer> IteractionFinished;
        public event EventHandler<Neuron> IteractionWinner;
        public event EventHandler<AbstractTrainer> TrainingFinished;

        protected void InvokeInteractionWinner(Neuron neuron) => IteractionWinner?.Invoke(this, neuron);
        protected void InvokeTrainingFinished() => TrainingFinished?.Invoke(this, this);
        protected void InvokeInteractionFinished() => IteractionFinished?.Invoke(this, this);
        protected void InvokeInteractionStarting() => IteractionStarting?.Invoke(this, this);
    }
    public class TrainArtNetwork
    {
        public void Train(IEnumerable<Neuron> inputNeurons, IEnumerable<Neuron> outputNeurons)
        {

        }
    }
    public class Trainer : AbstractTrainer
    {
        public List<Neuron> Neurons { get; private set; }
        private List<List<double>> LearningVectors;

        public void Train(IEnumerable<Neuron> neurons, IEnumerable<IEnumerable<double>> learningVectors)
        {
            PrepareData(neurons, learningVectors);
            Iteration = 0;
            foreach (var learningVector in LearningVectors)
            {
                Iteration += 1;
                InvokeInteractionStarting();
                Neurons.ForEach(d => d.FitnessForVector(learningVector));
                var theBestNeurone = Neurons.Where(d => !d.IsTired).Min();

                // All neurons are tired! Awsome!
                if (theBestNeurone == null) continue;
                else theBestNeurone.Retrain();

                InvokeInteractionWinner(theBestNeurone);
                InvokeInteractionFinished();
            }
            InvokeTrainingFinished();
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
