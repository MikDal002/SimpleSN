using System;
using System.Diagnostics;
using System.Linq;

namespace SimpleGA.Core
{
    public class GeneticAlgorithm<T> : IGeneticAlgorithm where T : class, IChromosome
    {
        public int GenerationsNumber { get; private set; }
        public event EventHandler<Generation<T>> GenerationHasGone;


        public IPopulation<T> Population { get; }
        public IFitness<T> Fitness { get; }

        public ITermination Termination { get; set; }

        public T BestChromosome { get; private set; }

        public GeneticAlgorithm(IPopulation<T> population, IFitness<T> fitness)
        {
            Population = population ?? throw new ArgumentNullException(nameof(population));
            Fitness = fitness ?? throw new ArgumentNullException(nameof(fitness));
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
                    else if (bestChromosomeInGeneration.Fitness < chromosome.Fitness)
                        bestChromosomeInGeneration = chromosome;
                }

                currentGeneration.BestChromosome = bestChromosomeInGeneration;

                if (BestChromosome == null) BestChromosome = bestChromosomeInGeneration;
                //else if (bestChromosomeInGeneration!.Fitness > BestChromosome.Fitness)
                else if (bestChromosomeInGeneration.CompareTo(BestChromosome) > 1)
                    BestChromosome = bestChromosomeInGeneration;

                GenerationHasGone?.Invoke(this, currentGeneration);

            } while (!Termination.HasReached(this));
        }

    }
}
