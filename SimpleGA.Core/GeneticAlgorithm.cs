using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleGA.Core
{
    public interface IGeneticAlgorithm
    {
        public int GenerationsNumber { get; }
    }
    public class GeneticAlgorithm<T> : IGeneticAlgorithm where T : class, IChromosome
    {
        public int GenerationsNumber { get; private set; }
        public event EventHandler<Generation<T>> GenerationHasGone;


        public IPopulation<T> Population { get; }
        public IFitness<T> Fitness { get; }
        public ISelection Selection { get; }
        public ICrossover Crossover { get; }
        public IMutation Mutation { get; }

        public ITermination Termination { get; set; }

        public T BestChromosome { get; private set; }

        public GeneticAlgorithm(IPopulation<T> population, IFitness<T> fitness, ISelection selection, ICrossover crossover,
            IMutation mutation)
        {
            Population = population ?? throw new ArgumentNullException(nameof(population));
            Fitness = fitness ?? throw new ArgumentNullException(nameof(fitness));
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
            Crossover = crossover ?? throw new ArgumentNullException(nameof(crossover));
            Mutation = mutation ?? throw new ArgumentNullException(nameof(mutation));
        }

        public void Start()
        {
            do
            {
                GenerationsNumber++;
                Generation<T> currentGeneration = Population.StartNewGeneration();
                if (currentGeneration.Count == 0)
                    throw new ArgumentException("Generation must be bigger than zero!");
                

                T bestChromosomeInGeneration = null;
                foreach (T chromosome in currentGeneration)
                {
                    chromosome.Fitness = Fitness.Evaluate(chromosome);
                    if (chromosome.Fitness == null) throw new ArgumentException("Generation must be bigger than zero!");
                    

                    if (bestChromosomeInGeneration == null) bestChromosomeInGeneration = chromosome;
                    else if (bestChromosomeInGeneration.Fitness > chromosome.Fitness)
                        bestChromosomeInGeneration = chromosome;
                }

                currentGeneration.BestChromosome = bestChromosomeInGeneration;

                if (BestChromosome == null) BestChromosome = bestChromosomeInGeneration;
                else if (bestChromosomeInGeneration!.Fitness > BestChromosome.Fitness)
                    BestChromosome = bestChromosomeInGeneration;

                GenerationHasGone?.Invoke(this, currentGeneration);

            } while (!Termination.HasReached(this));
        }

    }

    public class Generation<T> : IReadOnlyCollection<T> where T : IChromosome
    {
        private readonly IReadOnlyCollection<T> _readOnlyCollectionImplementation;

        public T BestChromosome { get; set; }
        public Generation(IReadOnlyCollection<T> list)
        {
            _readOnlyCollectionImplementation = list;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return _readOnlyCollectionImplementation.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_readOnlyCollectionImplementation).GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _readOnlyCollectionImplementation.Count;

    }

    public interface ITermination
    {
        bool HasReached(IGeneticAlgorithm geneticAlgorithm);
    }

    public class GenerationNumberTermination : ITermination
    {
        public int MaxGenerationsCount { get; }

        public GenerationNumberTermination(int maxGenerationsCount)
        {
            MaxGenerationsCount = maxGenerationsCount;
        }

        /// <inheritdoc />
        public bool HasReached(IGeneticAlgorithm geneticAlgorithm)
        {
            return geneticAlgorithm.GenerationsNumber > MaxGenerationsCount;
        }
    }

    public interface IChromosomeFactory<T> where T : IChromosome
    {
        T CreateNew();
    }


    public interface IPopulation<T> where T : IChromosome
    {
        Generation<T> StartNewGeneration();
    }

    public class Population<T> : IPopulation<T> where T : IChromosome
    {
        private readonly IChromosomeFactory<T> _adamFactory;
        private Generation<T> _previousGeneration = null;

        public int MinSize { get; }
        public int MaxSize { get; }

        public Population(int minSize, int maxSize, IChromosomeFactory<T> adamFactory)
        {
            _adamFactory = adamFactory;
            MinSize = minSize;
            MaxSize = maxSize;
        }

        /// <inheritdoc />
        public Generation<T> StartNewGeneration()
        {
            var chromosomesForPopulation = new List<T>(MinSize);
            for (int i = 0; i < MinSize; ++i)
            {
                chromosomesForPopulation.Add(_adamFactory.CreateNew());
            }

            return _previousGeneration = new Generation<T>(chromosomesForPopulation);
        }
    }

    public interface IChromosome
    {
        double? Fitness { get; set; }
    }

    public class MyProblemChromosomeFactory : IChromosomeFactory<MyProblemChromosome>
    {
        /// <inheritdoc />
        public MyProblemChromosome CreateNew()
        {

            return new MyProblemChromosome(null, null, null, null);
        }
    }

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
            var max = 1000;
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

    public interface IFitness<T> where T : IChromosome
    {
        double Evaluate(T chromosome);
    }

    public class MyProblemFitness : IFitness<MyProblemChromosome>
    {
        /// <inheritdoc />
        public double Evaluate(MyProblemChromosome chromosome)
        {
            return Math.Sqrt(Math.Pow(chromosome.X1 - chromosome.X2, 2) + Math.Pow(chromosome.Y1 - chromosome.Y2, 2));
        }
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
