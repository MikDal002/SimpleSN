using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleSN.Core
{
    public static class CollectionHelper
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection) action(item);
        }
    }

    public class IAbstractTrainer
    {
        virtual public IReadOnlyList<Neuron> Neurons { get; }
    }

    public abstract class AbstractTrainer : IAbstractTrainer
    {
        public int Iteration { get; private set; }

        public event EventHandler<AbstractTrainer> IterationStarted;
        public event EventHandler<Neuron> IterationFinished;
        public event EventHandler<AbstractTrainer> TrainingFinished;
        public event EventHandler<AbstractTrainer> TrainingStarted;

        protected void InvokeTrainingStarted() {
            Iteration = 0;
            TrainingStarted?.Invoke(this, this);
        }

        public abstract void Train(List<Neuron> neurons, IEnumerable<IReadOnlyList<double>> learningVectors);
        protected void InvokeTrainingFinished() => TrainingFinished?.Invoke(this, this);
        protected void InvokeInteractionFinished(Neuron neuron) 
        {
            Iteration += 1;
            IterationFinished?.Invoke(this, neuron);
        }
        protected void InvokeInteractionStarting()
        {
            IterationStarted?.Invoke(this, this);
        }
    }



    public class TrainArtNetwork : AbstractTrainer
    {
        private List<Neuron> _neurons;
        public List<double[]> MemoryMaps { get; } = new List<double[]>();
        public double RequiredSimilarity { get; set; }  = 0.7;
        public override IReadOnlyList<Neuron> Neurons => _neurons;

        public override void Train(List<Neuron> inputNeurons, IEnumerable<IReadOnlyList<double>> learningVectors)
        {
            _neurons = inputNeurons;
            PrepareMemoryMap(inputNeurons);
            InvokeTrainingStarted();
            foreach (var learningVector in learningVectors)
            {
                InvokeInteractionStarting();
                
                inputNeurons.ForEach(d => d.FitnessForVector(learningVector));
                (Neuron neuronWhichFits, double[] memoryMap) = FindNeuronWithTheBestMemory(inputNeurons, learningVector);
                if (neuronWhichFits == null)
                {
                    InvokeInteractionFinished(neuronWhichFits);
                    continue;
                } 

                for(int i = 0; i < memoryMap.Length; ++i)
                {
                    memoryMap[i] = learningVector[i] * memoryMap[i];
                }

                neuronWhichFits.Retrain((lastWeight, index) => {
                    var newWeight = memoryMap[index] / (0.5 + memoryMap.MultiplyEachElementWith(learningVector).Sum());
                    return newWeight;
                });
                InvokeInteractionFinished(neuronWhichFits);
            }
        }

        private (Neuron neuron, double[] memoryMap) FindNeuronWithTheBestMemory(List<Neuron> inputNeurons, IEnumerable<double> learningVector)
        {
            foreach (var winningNeuron in inputNeurons.OrderBy(d => d.LastFitness))
            {
                var index = inputNeurons.IndexOf(winningNeuron);
                var memoryMap = MemoryMaps[index];
                var inputVectorToMemorySimilarity = memoryMap.MultiplyEachElementWith(learningVector).Sum() / learningVector.Sum();
                if (inputVectorToMemorySimilarity > RequiredSimilarity)
                {
                    return (winningNeuron, memoryMap);
                }
            }
            return (null, null);
        }

        private void PrepareMemoryMap(IReadOnlyCollection<Neuron> inputNeurons)
        {
            foreach (var neurone in inputNeurons)
            {
                var map = new double[neurone.Weights.Count];
                for (int i = 0; i < map.Length; ++i) map[i] = 1;

                MemoryMaps.Add(map);
            }
        }
    }

    public class Trainer : AbstractTrainer
    {
        private List<Neuron> _neurons;
        private List<IReadOnlyList<double>> _learningVectors;
        public override IReadOnlyList<Neuron> Neurons => _neurons;

        public override void Train(List<Neuron> neurons, IEnumerable<IReadOnlyList<double>> learningVectors)
        {
            PrepareData(neurons, learningVectors);
            InvokeTrainingStarted();
            foreach (var learningVector in _learningVectors)
            {
                InvokeInteractionStarting();
                _neurons.ForEach(d => d.FitnessForVector(learningVector));
                var theBestNeurone = _neurons.Where(d => !d.IsTired).Min();

                // All neurons are tired! Awsome!
                if (theBestNeurone != null) theBestNeurone.Retrain();

                InvokeInteractionFinished(theBestNeurone);
            }
            InvokeTrainingFinished();
        }

        private void PrepareData(List<Neuron> _neurons, IEnumerable<IReadOnlyList<double>> _learningVectors)
        {
            this._neurons = _neurons;
            this._learningVectors = _learningVectors.ToList();
            var neuronsDendryds = this._neurons.First().Weights.Count;
            if (this._neurons.Any(d => d.Weights.Count != neuronsDendryds)) throw new ArgumentException(nameof(Trainer._neurons));
            if (this._learningVectors.Any(vector => vector.Count() != neuronsDendryds)) throw new ArgumentException(nameof(Trainer._learningVectors));
        }
    }

}
