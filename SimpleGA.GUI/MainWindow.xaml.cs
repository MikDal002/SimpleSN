#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleGA.Core;
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Notifiers;

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
            FilePath = string.Empty;
            InitializeComponent();
        }

        private double Silnia2(int n)
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

        public async Task ProblemTests<TChrom, TGen, TFactory, TFitness>(double DesiredFitness)
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

                () => new TheSameNeuronWinner(100),
                () => new TheSameNeuronWinner(200),
                () => new TheSameNeuronWinner(300),
                () => new TheSameNeuronWinner(500),
                () => new TheSameNeuronWinner(800),
                () => new TheSameNeuronWinner(1300),
                () => new TheSameNeuronWinner(2100),
            };


            var populations = new[] {100, 200, 300, 500, 800, 1300};

            AllTestesCount.Value = selections.Length
                                   * crossOvers.Length
                                   * mutations.Count
                                   * populations.Length
                                   * terminationFactory.Length;
            ExecutedTestes.Value = 0;
            using var file = File.Open(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var sw = new StreamWriter(file);
            using var writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.Indented;

            try
            {
                await writer.WriteStartArrayAsync();

                foreach (var selection in selections)
                foreach (var crossover in crossOvers)
                foreach (var mutation in mutations)
                foreach (var population in populations)
                foreach (var termination in terminationFactory)
                {
                    var factory = new TFactory();
                    var crossOverReal = crossover(factory);
                    var selectionReal = selection();
                    var mutationReal = mutation(factory);
                    var terminationReal = termination();
                    IChromosome previousWinner = null;
                    var steps = new List<StepDef>();

                    var stopwatch = Stopwatch.StartNew();
                    var ga = await GenericProblem<TChrom, TGen, TFactory, TFitness>(crossOverReal, selectionReal,
                        mutationReal,
                        terminationReal, population, population * 2, factory, (sender, chroms) =>
                        {
                            Debug.Write(".");
                            if (previousWinner == chroms.BestChromosome) return;
                            previousWinner = chroms.BestChromosome;
                            steps.Add(
                                new StepDef
                                {
                                    Elapse = stopwatch.ElapsedMilliseconds,
                                    fitness = chroms.BestChromosome.Fitness!.Value,
                                    generation = (sender as GeneticAlgorithm<TChrom>)!.GenerationsNumber
                                });
                        });
                    stopwatch.Stop();
                    Debug.WriteLine("\r\n Test passed");

                    ExecutedTestes.Value = ExecutedTestes.Value + 1;

                    SaveResult(new Result<TChrom>
                    {
                        Population = population,
                        Termination = terminationReal,
                        Crossover = crossOverReal,
                        Selection = selectionReal,
                        Mutation = mutationReal,
                        AmountOfGenerations = ga.GenerationsNumber,
                        WinnerChromosome = ga.BestChromosome,
                        TotalTimeMs = stopwatch.ElapsedMilliseconds,
                        TheBestFoundFitness = ga.BestChromosome.Fitness!.Value,
                        RealTheBestValue = DesiredFitness,
                        Steps = steps
                    }, writer);
                }
            }
            finally { await writer.WriteEndArrayAsync(); }
        }

        public ReactiveProperty<long> AllTestesCount { get; } = new();
        public ReactiveProperty<int> ExecutedTestes { get; } = new(0);
        public ReactiveProperty<string> Problem { get; } = new("");
        private readonly JsonSerializer _serializer = new();


        public string FilePath { set; get; }


        private void SaveResult<TChrom>(Result<TChrom> p0, JsonWriter writer) where TChrom : class, IChromosome
        {
            Debug.WriteLine(JsonConvert.SerializeObject(p0));

            JObject obj = JObject.FromObject(p0, _serializer);
            obj.WriteTo(writer);
            writer.Flush();
        }


        private async Task<GeneticAlgorithm<TChrom>> GenericProblem<TChrom, TGen, TFactory, TFitness>(
            ICrossover<TChrom> crossover,
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
            await Task.Run(() => ga.Start());

            Debug.WriteLine("\r\nBest solution found has {0}.", ga.BestChromosome.Fitness);
            return ga;
        }

        private bool _isRunning = false;

        private async void MainWindow_OnActivated(object? sender, EventArgs e)
        {
            if (_isRunning) return;
            _isRunning = true;
            Problem.Value = "";

            var dirInfo = Directory.CreateDirectory("Results");

            Problem.Value = "Travelsman";
            FilePath = Path.Combine(dirInfo.FullName,
                $"{Problem.Value}_{DateTime.Now.ToString("yyyy-MM-dd HHmmss")}.json");
            await ProblemTests<TravelerProblemChromosome, City, TravelerProblemFactory, TravelsManFitness>(33523);

            Problem.Value = "Knapsack";
            FilePath = Path.Combine(dirInfo.FullName,
                $"{Problem.Value}_{DateTime.Now.ToString("yyyy-MM-dd HHmmss")}.json");
            await ProblemTests<KnapsackProblemChromosome, Insert, KnapsackProblemFactory, KnapsackFitness>(1634);
            _isRunning = false;
        }
    }
}
