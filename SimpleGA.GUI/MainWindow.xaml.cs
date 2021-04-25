using System;
using Newtonsoft.Json;
using SimpleGA.Core;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using SimpleGA.Core.Chromosomes;
using SimpleGA.Core.Crossovers;
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

        private void KnapsacknProblem()
        {
            var selection = new RouletteSelection();
            var chromosomeFactory = new KnapsackProblemFactory();
            var crossover = new OrderedCrossover<KnapsackProblemChromosome, Insert>(chromosomeFactory);
            var fitness = new KnapsackFitness();
            var mutation = new SwapMutation<KnapsackProblemChromosome, Insert>(chromosomeFactory);
            var population =
                new Population<KnapsackProblemChromosome>(1000, 2000, chromosomeFactory, crossover, mutation,
                    selection);
            var ga = new GeneticAlgorithm<KnapsackProblemChromosome>(population, fitness);
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
                    $"Generację {(sender as IGeneticAlgorithm).GenerationsNumber} wygrał {generation.BestChromosome} z profitem {generation.BestChromosome.TotalProfit}.");
                var winnerWeigth = 0.0;
                Debug.WriteLine(JsonConvert.SerializeObject(generation.BestChromosome.Genes.TakeWhile(d =>
                {
                    winnerWeigth += d.Weight;
                    return winnerWeigth < generation.BestChromosome.MaxWeight;
                }).Select(d => d.Name)));
                // mutation.MutationThreshold =
                //     ga.GenerationsNumber / (double) termination.MaxGenerationsCount + 0.1;
                // mutation.AmountOfSwaps = (ga.GenerationsNumber + 1) % 100;
            };

            Debug.WriteLine("GA running...");
            ga.Start();

            Debug.WriteLine("\r\nBest solution found has {0} profit ({1}).", ga.BestChromosome.Fitness,
                ga.BestChromosome.TotalProfit);
            Debug.WriteLine(
                $"Wygenerowano {chromosomeFactory.Counter} chromosomów vs {silnia2(chromosomeFactory.AllInserts.Count)} wszystkich możliwości.");

            var winnerWeigth = 0.0;
            Debug.WriteLine(JsonConvert.SerializeObject(ga.BestChromosome.Genes.TakeWhile(d =>
            {
                winnerWeigth += d.Weight;
                return winnerWeigth < ga.BestChromosome.MaxWeight;
            }).Select(d => d.Name)));
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
                KnapsacknProblem();
            }
            //);
            //worker.Start();
        }
    }
}
