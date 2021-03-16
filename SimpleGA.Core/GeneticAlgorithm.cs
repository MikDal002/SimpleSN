using System;

namespace SimpleGA.Core
{
    public class GeneticAlgorithm
    {
        public IPopulation Population { get; }
        public IFitness Fitness { get; }
        public ISelection Selection { get; }
        public ICrossover Crossover { get; }
        public IMutation Mutation { get; }

        public ITermination Termination { get; set; }

        public IChromosome BestChromosome { get; private set; }

        public GeneticAlgorithm(IPopulation population, IFitness fitness, ISelection selection, ICrossover crossover,
            IMutation mutation)
        {
            Population = population;
            Fitness = fitness;
            Selection = selection;
            Crossover = crossover;
            Mutation = mutation;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }

    public interface ITermination { }

    public class GenerationNumberTermination : ITermination
    {
        public int Number { get; }

        public GenerationNumberTermination(int number)
        {
            Number = number;
        }
    }

    public interface IPopulation { }

    public class Population : IPopulation
    {
        private readonly IChromosome _adamChromosome;
        public int MinSize { get; }
        public int MaxSize { get; }

        public Population(int minSize, int maxSize, IChromosome adamChromosome)
        {
            _adamChromosome = adamChromosome;
            MinSize = minSize;
            MaxSize = maxSize;
        }


    }

    public interface IChromosome
    {
        double? Fitness { get; set; }
    }
    public class MyProblemChromosome : IChromosome
    {
        /// <inheritdoc />
        public double? Fitness { get; set; }
    }

    public interface IFitness
    {

    }

    public class MyProblemFitness : IFitness
    {

    }

    public interface IMutation
    {

    }

    public class ReverseSequenceMutation : IMutation
    {

    }

    public interface ICrossover { }

    public class OrderedCrossover : ICrossover
    {

    }

    public interface ISelection
    {

    }

    public class EliteSelection : ISelection
    {

    }

}
