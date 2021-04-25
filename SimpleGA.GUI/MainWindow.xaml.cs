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

            // FInd the best solution for \sqrt((x1-x2)^2 + (y1 - y2)^)
            //var worker = new Thread(() =>
            {
                // var selection = new RouletteSelection();
                // var chromosomeFactory = new MyProblemChromosomeFactory();
                // //var crossover = new MyProblemChromosomeCrossover();
                // //var crossover = new UniformCrossover<MyProblemChromosome, double>(chromosomeFactory);
                // //var crossover = new SinglePointCrossover<MyProblemChromosome, double>(chromosomeFactory);
                // var crossover = new MultiPointCrossover<MyProblemChromosome, double>(2, chromosomeFactory);
                // //var mutation = new RandomResettingMutation<MyProblemChromosome, double>(chromosomeFactory);
                // var mutation = new SwapMutation<MyProblemChromosome, double>(chromosomeFactory);
                // var fitness = new MyProblemFitness();
                // var population = new Population<MyProblemChromosome>(1000, 2000, chromosomeFactory, crossover, mutation, selection);
                // 
                // var ga = new GeneticAlgorithm<MyProblemChromosome>(population, fitness);
                // ga.Termination = new GenerationNumberTermination(1000);

                var selection = new RouletteSelection();
                var chromosomeFactory = new TravelerProblemFactory();
                var crossover = new OrderedCrossover<TravelerProblemChromosome, City>(chromosomeFactory);
                var fitness = new TravelsManFitness();
                var mutation = new SwapMutation<TravelerProblemChromosome, City>(chromosomeFactory);
                var population = new Population<TravelerProblemChromosome>(1000, 2000, chromosomeFactory, crossover, mutation, selection);
                var ga = new GeneticAlgorithm<TravelerProblemChromosome>(population, fitness);
                ga.Termination = new GenerationNumberTermination(10000);

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
                };

                Debug.WriteLine("GA running...");
                ga.Start();

                Debug.WriteLine("\r\nBest solution found has {0} path ({1}).", ga.BestChromosome.Fitness, ga.BestChromosome.TotalPath);
                Debug.WriteLine(JsonConvert.SerializeObject(ga.BestChromosome.Genes.Select(d => d.Name)));
            }
            //);
            //worker.Start();
        }
    }
}
