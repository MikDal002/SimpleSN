using SimpleGA.Core;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace SimpleGA.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // FInd the best solution for \sqrt((x1-x2)^2 + (y1 - y2)^)
            var worker = new Thread(() =>
            {
                var selection = new RouletteSelection();
                //var crossover = new MyProblemChromosomeCrossover();
                var chromosomeFactory = new MyProblemChromosomeFactory();
                var crossover = new UniformCrossover<MyProblemChromosome, double>(chromosomeFactory);
                var mutation = new GenerateCompletelyNewValuesMutation();
                var fitness = new MyProblemFitness();
                var population = new Population<MyProblemChromosome>(1000, 2000, chromosomeFactory, crossover, mutation, selection);

                var ga = new GeneticAlgorithm<MyProblemChromosome>(population, fitness);
                ga.Termination = new GenerationNumberTermination(1000);

                MyProblemChromosome previousWinner = null;
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
                        $"Generację {(sender as IGeneticAlgorithm).GenerationsNumber} wygrał {generation.BestChromosome} z dopasowaniem {generation.BestChromosome.Fitness}.");
                };

                Debug.WriteLine("GA running...");
                ga.Start();

                Debug.WriteLine("Best solution found has {0} fitness ({1}).", ga.BestChromosome.Fitness, ga.BestChromosome);
            });
            worker.Start();
        }
    }
}
