using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SimpleGA.Core;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using SimpleGA.Core.Chromosomes;
using SimpleGA.Core.Crossovers;
using SimpleGA.Core.Fitnesses;
using SimpleGA.Core.Mutations;
using SimpleGA.Core.Populations;
using SimpleGA.Core.Selections;
using SimpleGA.Core.Solutions.KnapsackProblem;
using SimpleGA.Core.Solutions.MyProblem;
using SimpleGA.Core.Solutions.TravelersSalesmanProblem;
using SimpleGA.Core.Terminations;

namespace SimpleGA.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Salesman dataset: https://people.sc.fsu.edu/~jburkardt/datasets/tsp/tsp.html
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private double silnia2(int n)
        {
            double result = 1;
            for (int i = 1; i <= n; i++) result *= i;
            return result;
        }

        private void SimpleFunctionOptimization()
        {
            var selection = new RouletteSelection();
            var chromosomeFactory = new MyProblemChromosomeFactory();
            //var crossover = new MyProblemChromosomeCrossover();
            //var crossover = new UniformCrossover<MyProblemChromosome, double>(chromosomeFactory);
            //var crossover = new SinglePointCrossover<MyProblemChromosome, double>(chromosomeFactory);
            var crossover = new MultiPointCrossover<MyProblemChromosome, double>(2, chromosomeFactory);
            //var mutation = new RandomResettingMutation<MyProblemChromosome, double>(chromosomeFactory);
            var mutation = new SwapMutation<MyProblemChromosome, double>(chromosomeFactory);
            var fitness = new MyProblemFitness();
            var population =
                new Population<MyProblemChromosome>(1000, 2000, chromosomeFactory, crossover, mutation, selection);

            var ga = new GeneticAlgorithm<MyProblemChromosome>(population, fitness);
            ga.Termination = new GenerationNumberTermination(1000);
        }


        public void ProblemTests<TChrom, TGen, TFactory, TFitness>(double DesiredFitness)
            where TChrom : class, IGenableChromosome<TGen>
            where TFactory : IGenableChromosomeFactory<TChrom, TGen>, new()
            where TFitness : IFitness<TChrom>, new()
        {
            var selections = new Func<ISelection>[] {() => new RouletteSelection()};

            Func<double, double, TFactory, ICrossover<TChrom>> orderedSelection = (begining, finish, factory) =>
                new OrderedCrossover<TChrom, TGen>(begining, finish, factory);
            Func<int, TFactory, ICrossover<TChrom>> cyclicSelection =
                (step, factory) => new CyclicOrderedCrossover<TChrom, TGen>(step, factory);

            var crossOvers = new Func<TFactory, ICrossover<TChrom>>[]
            {
                factory => orderedSelection(0.1, 0.9, factory),
                factory => orderedSelection(0.2, 0.8, factory),
                factory => orderedSelection(0.3, 0.7, factory),
                factory => orderedSelection(0.4, 0.6, factory),

                factory => cyclicSelection(2, factory),
                factory => cyclicSelection(3, factory),
                factory => cyclicSelection(5, factory),
                factory => cyclicSelection(8, factory),
                factory => cyclicSelection(13, factory),
            };

            Func<int, double, TFactory, IMutation<TChrom>> mutationFactory = (swaps, mutationThreshold, factory) =>
                new SwapMutation<TChrom, TGen>(factory) {MutationThreshold = mutationThreshold, AmountOfSwaps = swaps};

            var mutations = new List<Func<TFactory, IMutation<TChrom>>>();
            foreach (var swaps in new[] {1, 2, 3, 5, 8, 13})
            foreach (var threshold in new[] {0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9})
                mutations.Add(factory => mutationFactory(swaps, threshold, factory));

            var terminationFactory = new Func<ITermination>[]
            {
                () => new GenerationNumberTermination(1000),
                () => new GenerationNumberTermination(2000),
                () => new GenerationNumberTermination(3000),
                () => new GenerationNumberTermination(5000),
                () => new GenerationNumberTermination(8000),
                () => new GenerationNumberTermination(13000),

                () => new TheSameNeuronWInner(100),
                () => new TheSameNeuronWInner(200),
                () => new TheSameNeuronWInner(300),
                () => new TheSameNeuronWInner(500),
                () => new TheSameNeuronWInner(800),
                () => new TheSameNeuronWInner(1300),
                () => new TheSameNeuronWInner(2100),
            };

            var populations = new[] {100, 200, 300, 500, 800, 1300};

            foreach (var selection in selections)
            foreach (var crossover in crossOvers)
            foreach (var mutation in mutations)
            foreach (var population in populations)
            foreach (var termination in terminationFactory)
            {
                var stopwatch = new Stopwatch();
                var factory = new TFactory();
                var crossOverReal = crossover(factory);
                var selectionReal = selection();
                var mutationReal = mutation(factory);
                var terminationReal = termination();
                var ga = GenericProblem<TChrom, TGen, TFactory, TFitness>(crossOverReal, selectionReal, mutationReal,
                    terminationReal
                    ,
                    population, population * 2, factory, (sender, chroms) =>
                    {
                        SaveResult(new Result<TChrom>
                        {
                            Population = population,
                            Termination = terminationReal,
                            Crossover = crossOverReal,
                            Selection = selectionReal,
                            Mutation = mutationReal,
                            Generation = (sender as GeneticAlgorithm<TChrom>)!.GenerationsNumber,
                            WinnerChromosome = chroms.BestChromosome,
                            TimeFromBeginning = stopwatch.Elapsed,
                            TheBestFoundFitness = chroms.BestChromosome.Fitness!.Value,
                            RealTheBestValue = DesiredFitness
                        });
                    });
                SaveResult(new Result<TChrom>
                {
                    Population = population,
                    Termination = terminationReal,
                    Crossover = crossOverReal,
                    Selection = selectionReal,
                    Mutation = mutationReal,
                    Generation = ga.GenerationsNumber,
                    WinnerChromosome = ga.BestChromosome,
                    TimeFromBeginning = stopwatch.Elapsed,
                    TheBestFoundFitness = ga.BestChromosome.Fitness!.Value,
                    RealTheBestValue = DesiredFitness
                });
                stopwatch.Stop();
            }
        }

        private void SaveResult<TChrom>(Result<TChrom> p0) where TChrom : class, IChromosome
        {
            Debug.WriteLine(JsonConvert.SerializeObject(p0));
        }

        public class Result<TChrom> where TChrom : class, IChromosome
        {
            public ISelection Selection { get; set; }
            public ICrossover<TChrom> Crossover { get; set; }
            public IMutation<TChrom> Mutation { get; set; }
            public ITermination Termination { get; set; }
            public int Population { get; set; }

            public TChrom WinnerChromosome { get; set; }
            public int Generation { get; set; }

            public TimeSpan TimeFromBeginning { get; set; }

            public double RealTheBestValue { get; set; }
            public double TheBestFoundFitness { get; set; }
        }


        private GeneticAlgorithm<TChrom> GenericProblem<TChrom, TGen, TFactory, TFitness>(ICrossover<TChrom> crossover,
            ISelection selection,
            IMutation<TChrom> mutation,
            ITermination termination,
            int populationMin,
            int populationMax, IChromosomeFactory<TChrom> factory,
            EventHandler<Generation<TChrom>> generationHandler)
            where TChrom : class, IGenableChromosome<TGen>
            where TFactory : IGenableChromosomeFactory<TChrom, TGen>, new()
            where TFitness : IFitness<TChrom>, new()
        {
            if (crossover == null) throw new ArgumentNullException(nameof(crossover));
            if (selection == null) throw new ArgumentNullException(nameof(selection));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            if (termination == null) throw new ArgumentNullException(nameof(termination));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (generationHandler == null) throw new ArgumentNullException(nameof(generationHandler));
            var fitness = new TFitness();
            var population = new Population<TChrom>(populationMin, populationMax, factory, crossover,
                mutation, selection);
            var ga = new GeneticAlgorithm<TChrom>(population, fitness);
            ga.Termination = termination;
            ga.GenerationHasGone += generationHandler;
            Debug.WriteLine("GA running...");
            ga.Start();

            Debug.WriteLine("\r\nBest solution found has {0}.", ga.BestChromosome.Fitness);
            return ga;
        }

        private void KnapsacknProblem(int generationCount = 10000, double crossOverBeginning = 0.1,
            double crossOverEnd = 0.9,
            double mutationProbability = 0.1, int amountOfSwapsInMutation = 1, int populationMin = 1000,
            int populationMax = 2000)
        {
            var selection = new RouletteSelection();
            var chromosomeFactory = new KnapsackProblemFactory();
            //var crossover = new OrderedCrossover<KnapsackProblemChromosome, Insert>(crossOverBeginning, crossOverEnd, chromosomeFactory);
            var crossover = new CyclicOrderedCrossover<KnapsackProblemChromosome, Insert>(3, chromosomeFactory);
            var fitness = new KnapsackFitness();
            var mutation = new SwapMutation<KnapsackProblemChromosome, Insert>(chromosomeFactory)
            {
                MutationThreshold = mutationProbability,
                AmountOfSwaps = amountOfSwapsInMutation
            };
            var population =
                new Population<KnapsackProblemChromosome>(populationMin, populationMax, chromosomeFactory, crossover,
                    mutation,
                    selection);
            var ga = new GeneticAlgorithm<KnapsackProblemChromosome>(population, fitness);
            var termination = new GenerationNumberTermination(generationCount);
            ga.Termination = termination;

            //IChromosome previousWinner = null;
            //ga.GenerationHasGone += (sender, generation) =>
            //{
            //    if (generation.BestChromosome == previousWinner)
            //    {
            //        Debug.Write(".");
            //        return;
            //    }

            //    Debug.WriteLine("");
            //    previousWinner = generation.BestChromosome;
            //    Debug.WriteLine(
            //        $"Generację {(sender as IGeneticAlgorithm).GenerationsNumber} wygrał {generation.BestChromosome} z profitem {generation.BestChromosome.TotalProfit}.");
            //    var winnerWeigth = 0.0;
            //    Debug.WriteLine(JsonConvert.SerializeObject(generation.BestChromosome.Genes.TakeWhile(d =>
            //    {
            //        winnerWeigth += d.Weight;
            //        return winnerWeigth < generation.BestChromosome.MaxWeight;
            //    }).Select(d => d.Name)));
            //    // mutation.MutationThreshold =
            //    //     ga.GenerationsNumber / (double) termination.MaxGenerationsCount + 0.1;
            //    // mutation.AmountOfSwaps = (ga.GenerationsNumber + 1) % 100;
            //};

            //Debug.WriteLine("GA running...");
            //ga.Start();
            //
            //Debug.WriteLine("\r\nBest solution found has {0} profit ({1}).", ga.BestChromosome.Fitness,
            //    ga.BestChromosome.TotalProfit);
            ////Debug.WriteLine($"Wygenerowano {chromosomeFactory.Counter} chromosomów vs {silnia2(chromosomeFactory.AllInserts.Count)} wszystkich możliwości.");
            //
            //var winnerWeigth = 0.0;
            //Debug.WriteLine(JsonConvert.SerializeObject(ga.BestChromosome.Genes.TakeWhile(d =>
            //{
            //    winnerWeigth += d.Weight;
            //    return winnerWeigth < ga.BestChromosome.MaxWeight;
            //}).Select(d => d.Name)));
        }

        private void TravelsmanProblem()
        {
            var selection = new RouletteSelection();
            var chromosomeFactory = new TravelerProblemFactory();
            var crossover = new OrderedCrossover<TravelerProblemChromosome, City>(chromosomeFactory);
            var fitness = new TravelsManFitness();
            var mutation = new SwapMutation<TravelerProblemChromosome, City>(chromosomeFactory);
            var population =
                new Population<TravelerProblemChromosome>(1000, 2000, chromosomeFactory, crossover, mutation,
                    selection);
            var ga = new GeneticAlgorithm<TravelerProblemChromosome>(population, fitness);
            var termination = new GenerationNumberTermination(10000);
            ga.Termination = termination;

            IChromosome previousWinner = null;
            ga.GenerationHasGone += (sender, generation) =>
            {
                if (generation.BestChromosome == previousWinner)
                {
                    Debug.Write(".");
                    return;
                }

                Debug.WriteLine("");
                previousWinner = generation.BestChromosome;
                Debug.WriteLine(
                    $"Generację {(sender as IGeneticAlgorithm).GenerationsNumber} wygrał {generation.BestChromosome} z ścieżką {generation.BestChromosome.TotalPath}.");
                Debug.WriteLine(JsonConvert.SerializeObject(generation.BestChromosome.Genes.Select(d => d.Name)));
                // mutation.MutationThreshold =
                //     ga.GenerationsNumber / (double) termination.MaxGenerationsCount + 0.1;
                // mutation.AmountOfSwaps = (ga.GenerationsNumber + 1) % 100;
            };

            Debug.WriteLine("GA running...");
            ga.Start();

            Debug.WriteLine("\r\nBest solution found has {0} path ({1}).", ga.BestChromosome.Fitness,
                ga.BestChromosome.TotalPath);
            Debug.WriteLine(
                $"Wygenerowano {chromosomeFactory.Counter} chromosomów vs {silnia2(chromosomeFactory.AllCities.Count)} wszystkich możliwości.");
            Debug.WriteLine(JsonConvert.SerializeObject(ga.BestChromosome.Genes.Select(d => d.Name)));
        }

        private void MainWindow_OnActivated(object? sender, EventArgs e)
        {
            // FInd the best solution for \sqrt((x1-x2)^2 + (y1 - y2)^)
            //var worker = new Thread(() =>
            {
                // SimpleFunctionOptimization();
                // TravelsmanProblem();
                // KnapsacknProblem();
                ProblemTests<KnapsackProblemChromosome, Insert, KnapsackProblemFactory, KnapsackFitness>(2000);
            }
            //);
            //worker.Start();
        }
    }
}
