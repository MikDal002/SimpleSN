using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimpleGA.Core;

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

            var selection = new RouletteSelection();
            var crossover = new SinglePointCrossover();
            var mutation = new GenerateCompletelyNewValuesMutation();
            var fitness = new MyProblemFitness();
            var population = new Population<MyProblemChromosome>(1000, 2000, new MyProblemChromosomeFactory(), crossover, mutation, selection);

            var ga = new GeneticAlgorithm<MyProblemChromosome>(population, fitness);
            ga.Termination = new GenerationNumberTermination(1000);

            MyProblemChromosome previousWinner = null;
            ga.GenerationHasGone += (sender, generation) =>
            {
                if (generation.BestChromosome == previousWinner) return;
                previousWinner = generation.BestChromosome;
                Debug.WriteLine(
                    $"Generację {(sender as IGeneticAlgorithm).GenerationsNumber} wygrał {generation.BestChromosome} z dopasowaniem {generation.BestChromosome.Fitness}.");
            };

            Debug.WriteLine("GA running...");
            ga.Start();

            Debug.WriteLine("Best solution found has {0} fitness ({1}).", ga.BestChromosome.Fitness, ga.BestChromosome);
        }
    }
}
